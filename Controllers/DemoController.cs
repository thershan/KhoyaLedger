using KhoyaLedger.Data;
using KhoyaLedger.Models;
using KhoyaLedger.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KhoyaLedger.Controllers;

[Authorize(Roles = "Owner")]
public class DemoController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IMilkRateService _rateService;
    private readonly ISettlementService _settlementService;
    private readonly UserManager<ApplicationUser> _userManager;

    public DemoController(ApplicationDbContext db, IMilkRateService rateService, ISettlementService settlementService, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _rateService = rateService;
        _settlementService = settlementService;
        _userManager = userManager;
    }

    public async Task<IActionResult> VerifyScenario()
    {
        var results = new List<(string Step, bool Pass, string Expected, string Actual)>();

        using var tx = await _db.Database.BeginTransactionAsync();
        try
        {
            // Setup
            var farmer = await _db.Farmers.FirstOrDefaultAsync(f => f.FarmerCode == "F001");
            if (farmer == null)
            {
                results.Add(("Farmer F001 exists", false, "F001", "not found"));
                ViewBag.Results = results;
                return View();
            }

            // Reset farmer balance for the test
            var origAdvance = farmer.AdvanceBalance;
            var origMilkPayable = farmer.MilkPayable;
            farmer.AdvanceBalance = 1000m;
            farmer.MilkPayable = 0m;
            await _db.SaveChangesAsync();

            // Add recovery rule: ₹5/L
            var rule = new Advance
            {
                FarmerId = farmer.Id,
                TransactionDate = DateOnly.FromDateTime(DateTime.Today),
                Type = AdvanceType.Disbursement,
                Amount = 0m,
                Notes = "Demo test recovery rule",
                RecoveryType = RecoveryType.FixedPerLitre,
                RecoveryValue = 5m,
                PostedByUserId = _userManager.GetUserId(User) ?? "demo",
                PostedAt = DateTimeOffset.UtcNow
            };
            _db.Advances.Add(rule);
            await _db.SaveChangesAsync();

            // 30L collection
            var today = DateOnly.FromDateTime(DateTime.Today);
            var (slab, rate) = await _rateService.GetRateAsync(MilkType.Buffalo, 6m, today);
            var grossValue = _rateService.ComputeGrossValue(30m, rate);

            results.Add(("Rate is ₹42/L", rate == 42m, "42.00", $"{rate}"));
            results.Add(("GrossMilkValue = 30 × 42 = ₹1260", grossValue == 1260m, "1260.00", $"{grossValue}"));

            // Simulate settlement
            var todayCount = await _db.MilkCollections.CountAsync(c => c.CollectionDate == today);
            var receipt = $"RC{today:yyyyMMdd}{(todayCount + 1):D4}";
            var collection = new MilkCollection
            {
                FarmerId = farmer.Id,
                CollectionDate = today,
                Shift = Shift.Morning,
                Litres = 30m,
                Fat = 6m,
                AppliedRateId = slab?.Id,
                AppliedRate = rate,
                GrossMilkValue = grossValue,
                Status = CollectionStatus.Posted,
                ReceiptNumber = receipt,
                CollectedByUserId = _userManager.GetUserId(User) ?? "demo",
                PostedAt = DateTimeOffset.UtcNow,
                PostedByUserId = _userManager.GetUserId(User) ?? "demo"
            };
            _db.MilkCollections.Add(collection);
            await _db.SaveChangesAsync();

            // Create settlement draft
            var draft = await _settlementService.CreateDraftAsync(farmer.Id, today, today);

            results.Add(("AdvanceRecovered = ₹150 (capped)", draft.AdvanceRecovered == 150m, "150.00", $"{draft.AdvanceRecovered}"));
            results.Add(("NetPayable = ₹1110", draft.NetPayable == 1110m, "1110.00", $"{draft.NetPayable}"));

            // Post settlement
            draft.FarmerId = farmer.Id;
            _db.FarmerSettlements.Add(draft);
            await _db.SaveChangesAsync();
            await _settlementService.PostSettlementAsync(draft.Id, _userManager.GetUserId(User) ?? "demo");

            await _db.Entry(farmer).ReloadAsync();
            results.Add(("AdvanceBalance after = ₹850", farmer.AdvanceBalance == 850m, "850.00", $"{farmer.AdvanceBalance}"));
            results.Add(("MilkPayable = ₹1110", farmer.MilkPayable == 1110m, "1110.00", $"{farmer.MilkPayable}"));

            // Record ₹600 payment
            await _settlementService.RecordPaymentAsync(draft.Id, 600m, PaymentMode.Cash, _userManager.GetUserId(User) ?? "demo", today, "Demo payment");

            await _db.Entry(draft).ReloadAsync();
            results.Add(("OutstandingPayable after ₹600 = ₹510", draft.OutstandingPayable == 510m, "510.00", $"{draft.OutstandingPayable}"));
        }
        finally
        {
            // Rollback all test data
            await tx.RollbackAsync();
        }

        ViewBag.Results = results;
        return View();
    }
}
