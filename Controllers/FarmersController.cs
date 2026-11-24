using KhoyaLedger.Data;
using KhoyaLedger.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KhoyaLedger.Controllers;

[Authorize]
public class FarmersController : Controller
{
    private readonly ApplicationDbContext _db;

    public FarmersController(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index(string? search)
    {
        var query = _db.Farmers.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(f => f.FarmerCode.Contains(search) || f.Name.Contains(search) || f.Village.Contains(search));
        var farmers = await query.OrderBy(f => f.FarmerCode).ToListAsync();
        ViewBag.Search = search;
        return View(farmers);
    }

    [Authorize(Roles = "Owner")]
    public IActionResult Create() => View(new Farmer());

    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Owner")]
    public async Task<IActionResult> Create(Farmer farmer)
    {
        if (!ModelState.IsValid) return View(farmer);
        farmer.CreatedAt = DateTimeOffset.UtcNow;
        farmer.UpdatedAt = DateTimeOffset.UtcNow;
        _db.Farmers.Add(farmer);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Owner")]
    public async Task<IActionResult> Edit(int id)
    {
        var farmer = await _db.Farmers.FindAsync(id);
        if (farmer == null) return NotFound();
        return View(farmer);
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Owner")]
    public async Task<IActionResult> Edit(int id, Farmer farmer)
    {
        if (id != farmer.Id) return BadRequest();
        if (!ModelState.IsValid) return View(farmer);
        farmer.UpdatedAt = DateTimeOffset.UtcNow;
        _db.Farmers.Update(farmer);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Owner")]
    public async Task<IActionResult> Delete(int id)
    {
        var farmer = await _db.Farmers.FindAsync(id);
        if (farmer == null) return NotFound();
        return View(farmer);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken, Authorize(Roles = "Owner")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var farmer = await _db.Farmers.FindAsync(id);
        if (farmer != null)
        {
            farmer.IsActive = false;
            await _db.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Statement(int id, DateOnly? fromDate, DateOnly? toDate)
    {
        var farmer = await _db.Farmers.FindAsync(id);
        if (farmer == null) return NotFound();

        fromDate ??= DateOnly.FromDateTime(DateTime.Today.AddMonths(-1));
        toDate ??= DateOnly.FromDateTime(DateTime.Today);

        var collections = await _db.MilkCollections
            .Where(c => c.FarmerId == id && c.CollectionDate >= fromDate && c.CollectionDate <= toDate && c.Status == CollectionStatus.Posted)
            .OrderBy(c => c.CollectionDate).ThenBy(c => c.Shift)
            .ToListAsync();

        var advances = await _db.Advances
            .Where(a => a.FarmerId == id && a.TransactionDate >= fromDate && a.TransactionDate <= toDate)
            .OrderBy(a => a.TransactionDate)
            .ToListAsync();

        var payments = await _db.SettlementPayments
            .Include(sp => sp.Settlement)
            .Where(sp => sp.Settlement!.FarmerId == id && sp.PaidDate >= fromDate && sp.PaidDate <= toDate)
            .OrderBy(sp => sp.PaidDate)
            .ToListAsync();

        ViewBag.Farmer = farmer;
        ViewBag.FromDate = fromDate;
        ViewBag.ToDate = toDate;
        ViewBag.Advances = advances;
        ViewBag.Payments = payments;
        return View(collections);
    }

    [Authorize(Roles = "Owner")]
    public async Task<IActionResult> OpeningBalance(int id)
    {
        var farmer = await _db.Farmers.FindAsync(id);
        if (farmer == null) return NotFound();
        return View(farmer);
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Owner")]
    public async Task<IActionResult> OpeningBalance(int id, decimal advanceBalance, decimal milkPayable, string? notes)
    {
        var farmer = await _db.Farmers.FindAsync(id);
        if (farmer == null) return NotFound();
        farmer.AdvanceBalance = Math.Round(advanceBalance, 2, MidpointRounding.AwayFromZero);
        farmer.MilkPayable = Math.Round(milkPayable, 2, MidpointRounding.AwayFromZero);
        farmer.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync();
        TempData["Success"] = "Opening balances updated.";
        return RedirectToAction(nameof(Statement), new { id });
    }
}
