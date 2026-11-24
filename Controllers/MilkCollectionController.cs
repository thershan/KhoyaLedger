using KhoyaLedger.Data;
using KhoyaLedger.Models;
using KhoyaLedger.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KhoyaLedger.Controllers;

[Authorize]
public class MilkCollectionController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IMilkRateService _rateService;
    private readonly UserManager<ApplicationUser> _userManager;

    public MilkCollectionController(ApplicationDbContext db, IMilkRateService rateService, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _rateService = rateService;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index(DateOnly? date, Shift? shift)
    {
        date ??= DateOnly.FromDateTime(DateTime.Today);
        var query = _db.MilkCollections
            .Include(c => c.Farmer)
            .Where(c => c.CollectionDate == date);
        if (shift.HasValue) query = query.Where(c => c.Shift == shift.Value);
        var collections = await query.OrderBy(c => c.Shift).ThenBy(c => c.Farmer!.FarmerCode).ToListAsync();
        var totalLitres = collections.Sum(c => c.Litres);
        var totalValue = collections.Sum(c => c.GrossMilkValue);
        ViewBag.Date = date;
        ViewBag.Shift = shift;
        ViewBag.TotalLitres = totalLitres;
        ViewBag.TotalValue = totalValue;
        return View(collections);
    }

    public async Task<IActionResult> Collect()
    {
        ViewBag.Farmers = await _db.Farmers.Where(f => f.IsActive).OrderBy(f => f.FarmerCode).ToListAsync();
        return View(new MilkCollection { CollectionDate = DateOnly.FromDateTime(DateTime.Today), Shift = Shift.Morning });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Collect(MilkCollection model)
    {
        // Check duplicate
        var exists = await _db.MilkCollections.AnyAsync(c =>
            c.FarmerId == model.FarmerId &&
            c.CollectionDate == model.CollectionDate &&
            c.Shift == model.Shift &&
            c.Status == CollectionStatus.Posted);
        if (exists)
        {
            ModelState.AddModelError("", "A posted entry already exists for this farmer/date/shift.");
            ViewBag.Farmers = await _db.Farmers.Where(f => f.IsActive).OrderBy(f => f.FarmerCode).ToListAsync();
            return View(model);
        }

        var farmer = await _db.Farmers.FindAsync(model.FarmerId);
        if (farmer == null)
        {
            ModelState.AddModelError("FarmerId", "Farmer not found.");
            ViewBag.Farmers = await _db.Farmers.Where(f => f.IsActive).OrderBy(f => f.FarmerCode).ToListAsync();
            return View(model);
        }

        var (slab, rate) = await _rateService.GetRateAsync(farmer.MilkType, model.Fat, model.CollectionDate);
        if (rate == 0 && slab == null)
        {
            ModelState.AddModelError("", "No milk rate slab found for this farmer type and date.");
            ViewBag.Farmers = await _db.Farmers.Where(f => f.IsActive).OrderBy(f => f.FarmerCode).ToListAsync();
            return View(model);
        }

        // Generate ReceiptNumber
        var todayCount = await _db.MilkCollections.CountAsync(c => c.CollectionDate == model.CollectionDate);
        var receiptNumber = $"RC{model.CollectionDate:yyyyMMdd}{(todayCount + 1):D4}";

        var userId = _userManager.GetUserId(User) ?? string.Empty;
        model.AppliedRateId = slab?.Id;
        model.AppliedRate = rate;
        model.GrossMilkValue = _rateService.ComputeGrossValue(model.Litres, rate);
        model.ReceiptNumber = receiptNumber;
        model.Status = CollectionStatus.Pending;
        model.CollectedByUserId = userId;

        _db.MilkCollections.Add(model);
        await _db.SaveChangesAsync();
        TempData["Success"] = $"Collected: {receiptNumber}, Gross: ₹{model.GrossMilkValue:F2}";
        return RedirectToAction(nameof(Index), new { date = model.CollectionDate, shift = model.Shift });
    }

    [Authorize(Roles = "Owner")]
    public async Task<IActionResult> Post(int id)
    {
        var collection = await _db.MilkCollections.Include(c => c.Farmer).FirstOrDefaultAsync(c => c.Id == id);
        if (collection == null) return NotFound();
        return View(collection);
    }

    [HttpPost, ActionName("Post"), ValidateAntiForgeryToken, Authorize(Roles = "Owner")]
    public async Task<IActionResult> PostConfirmed(int id)
    {
        var collection = await _db.MilkCollections.FindAsync(id);
        if (collection == null) return NotFound();
        collection.Status = CollectionStatus.Posted;
        collection.PostedAt = DateTimeOffset.UtcNow;
        collection.PostedByUserId = _userManager.GetUserId(User);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Entry posted.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> RequestCorrection(int id)
    {
        var collection = await _db.MilkCollections.Include(c => c.Farmer).FirstOrDefaultAsync(c => c.Id == id);
        if (collection == null) return NotFound();
        return View(collection);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> RequestCorrection(int id, string correctionReason, decimal litres, decimal fat)
    {
        var original = await _db.MilkCollections.Include(c => c.Farmer).FirstOrDefaultAsync(c => c.Id == id);
        if (original == null) return NotFound();
        var farmer = original.Farmer!;

        var (slab, rate) = await _rateService.GetRateAsync(farmer.MilkType, fat, original.CollectionDate);
        var userId = _userManager.GetUserId(User) ?? string.Empty;
        var todayCount = await _db.MilkCollections.CountAsync(c => c.CollectionDate == original.CollectionDate);
        var receiptNumber = $"RC{original.CollectionDate:yyyyMMdd}{(todayCount + 1):D4}";

        var corrected = new MilkCollection
        {
            FarmerId = original.FarmerId,
            CollectionDate = original.CollectionDate,
            Shift = original.Shift,
            Litres = litres,
            Fat = fat,
            AppliedRateId = slab?.Id,
            AppliedRate = rate,
            GrossMilkValue = _rateService.ComputeGrossValue(litres, rate),
            Status = CollectionStatus.Corrected,
            ReceiptNumber = receiptNumber,
            CollectedByUserId = userId,
            CorrectionReason = correctionReason,
            OriginalCollectionId = id
        };

        _db.MilkCollections.Add(corrected);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Correction requested.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Owner")]
    public async Task<IActionResult> ApproveCorrection(int id)
    {
        var corrected = await _db.MilkCollections
            .Include(c => c.OriginalCollection)
            .FirstOrDefaultAsync(c => c.Id == id && c.Status == CollectionStatus.Corrected);
        if (corrected == null) return NotFound();

        // Update original to reflect correction
        if (corrected.OriginalCollection != null)
        {
            corrected.OriginalCollection.Status = CollectionStatus.Corrected;
        }
        corrected.Status = CollectionStatus.Posted;
        corrected.PostedAt = DateTimeOffset.UtcNow;
        corrected.PostedByUserId = _userManager.GetUserId(User);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Correction approved.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> ShiftTally(DateOnly? date, Shift? shift)
    {
        date ??= DateOnly.FromDateTime(DateTime.Today);
        shift ??= Shift.Morning;

        var sumFromEntries = await _db.MilkCollections
            .Where(c => c.CollectionDate == date && c.Shift == shift && c.Status != CollectionStatus.Corrected)
            .SumAsync(c => (decimal?)c.Litres) ?? 0m;

        ViewBag.Date = date;
        ViewBag.Shift = shift;
        ViewBag.SumFromEntries = sumFromEntries;
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ShiftTally(DateOnly date, Shift shift, decimal measuredLitres, string? notes)
    {
        var sumFromEntries = await _db.MilkCollections
            .Where(c => c.CollectionDate == date && c.Shift == shift && c.Status != CollectionStatus.Corrected)
            .SumAsync(c => (decimal?)c.Litres) ?? 0m;

        var variance = measuredLitres - sumFromEntries;
        var userId = _userManager.GetUserId(User) ?? string.Empty;

        var tally = new ShiftTally
        {
            TallyDate = date,
            Shift = shift,
            MeasuredLitres = measuredLitres,
            SumFromEntries = sumFromEntries,
            Variance = variance,
            Notes = notes,
            EnteredByUserId = userId
        };
        _db.ShiftTallies.Add(tally);
        await _db.SaveChangesAsync();
        TempData["Success"] = $"Tally saved. Variance: {variance:F2} L";
        return RedirectToAction(nameof(Index));
    }
}
