using KhoyaLedger.Data;
using KhoyaLedger.Models;
using KhoyaLedger.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KhoyaLedger.Controllers;

[Authorize(Roles = "Owner")]
public class SettlementsController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly ISettlementService _settlementService;
    private readonly UserManager<ApplicationUser> _userManager;

    public SettlementsController(ApplicationDbContext db, ISettlementService settlementService, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _settlementService = settlementService;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var settlements = await _db.FarmerSettlements
            .Include(s => s.Farmer)
            .OrderByDescending(s => s.Id)
            .ToListAsync();
        ViewBag.Farmers = await _db.Farmers.Where(f => f.IsActive).OrderBy(f => f.Name).ToListAsync();
        return View(settlements);
    }

    public async Task<IActionResult> Preview(int farmerId, DateOnly fromDate, DateOnly toDate)
    {
        var draft = await _settlementService.CreateDraftAsync(farmerId, fromDate, toDate);
        draft.FarmerId = farmerId;
        draft.Farmer = await _db.Farmers.FindAsync(farmerId);
        ViewBag.FromDate = fromDate;
        ViewBag.ToDate = toDate;
        return View(draft);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Post(int farmerId, DateOnly fromDate, DateOnly toDate)
    {
        var draft = await _settlementService.CreateDraftAsync(farmerId, fromDate, toDate);
        draft.FarmerId = farmerId;
        _db.FarmerSettlements.Add(draft);
        await _db.SaveChangesAsync();
        var userId = _userManager.GetUserId(User) ?? string.Empty;
        await _settlementService.PostSettlementAsync(draft.Id, userId);
        TempData["Success"] = $"Settlement posted. Net payable: ₹{draft.NetPayable:F2}";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> RecordPayment(int settlementId)
    {
        var settlement = await _db.FarmerSettlements.Include(s => s.Farmer).FirstOrDefaultAsync(s => s.Id == settlementId);
        if (settlement == null) return NotFound();
        return View(settlement);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> RecordPayment(int settlementId, decimal amount, PaymentMode mode, string? notes)
    {
        var userId = _userManager.GetUserId(User) ?? string.Empty;
        await _settlementService.RecordPaymentAsync(settlementId, amount, mode, userId, DateOnly.FromDateTime(DateTime.Today), notes);
        TempData["Success"] = $"Payment of ₹{amount:F2} recorded.";
        return RedirectToAction(nameof(Index));
    }
}
