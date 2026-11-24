using KhoyaLedger.Data;
using KhoyaLedger.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KhoyaLedger.Controllers;

[Authorize(Roles = "Owner")]
public class AdvancesController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdvancesController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index(int? farmerId)
    {
        var query = _db.Advances.Include(a => a.Farmer).AsQueryable();
        if (farmerId.HasValue) query = query.Where(a => a.FarmerId == farmerId.Value);
        var advances = await query.OrderByDescending(a => a.Id).ToListAsync();
        ViewBag.Farmers = await _db.Farmers.Where(f => f.IsActive).OrderBy(f => f.Name).ToListAsync();
        ViewBag.FarmerId = farmerId;
        return View(advances);
    }

    public async Task<IActionResult> Create(int? farmerId)
    {
        ViewBag.Farmers = await _db.Farmers.Where(f => f.IsActive).OrderBy(f => f.Name).ToListAsync();
        return View(new Advance { FarmerId = farmerId ?? 0, TransactionDate = DateOnly.FromDateTime(DateTime.Today), PostedAt = DateTimeOffset.UtcNow });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Advance advance)
    {
        var farmer = await _db.Farmers.FindAsync(advance.FarmerId);
        if (farmer == null)
        {
            ModelState.AddModelError("FarmerId", "Farmer not found.");
            ViewBag.Farmers = await _db.Farmers.Where(f => f.IsActive).OrderBy(f => f.Name).ToListAsync();
            return View(advance);
        }

        using var tx = await _db.Database.BeginTransactionAsync();
        try
        {
            advance.PostedByUserId = _userManager.GetUserId(User) ?? string.Empty;
            advance.PostedAt = DateTimeOffset.UtcNow;
            _db.Advances.Add(advance);

            // Update farmer balance
            if (advance.Type == AdvanceType.Disbursement)
                farmer.AdvanceBalance = Math.Round(farmer.AdvanceBalance + advance.Amount, 2, MidpointRounding.AwayFromZero);
            else if (advance.Type == AdvanceType.DirectRepayment)
                farmer.AdvanceBalance = Math.Round(farmer.AdvanceBalance - advance.Amount, 2, MidpointRounding.AwayFromZero);

            farmer.UpdatedAt = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync();
            await tx.CommitAsync();
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }

        TempData["Success"] = "Advance recorded.";
        return RedirectToAction(nameof(Index), new { farmerId = advance.FarmerId });
    }

    public async Task<IActionResult> SetRecoveryRule(int farmerId)
    {
        var farmer = await _db.Farmers.FindAsync(farmerId);
        if (farmer == null) return NotFound();
        var lastRule = await _db.Advances
            .Where(a => a.FarmerId == farmerId && a.RecoveryType != RecoveryType.None)
            .OrderByDescending(a => a.Id)
            .FirstOrDefaultAsync();
        ViewBag.Farmer = farmer;
        return View(lastRule ?? new Advance { FarmerId = farmerId, TransactionDate = DateOnly.FromDateTime(DateTime.Today) });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SetRecoveryRule(int farmerId, RecoveryType recoveryType, decimal? recoveryValue, string? notes)
    {
        var farmer = await _db.Farmers.FindAsync(farmerId);
        if (farmer == null) return NotFound();

        var rule = new Advance
        {
            FarmerId = farmerId,
            TransactionDate = DateOnly.FromDateTime(DateTime.Today),
            Type = AdvanceType.Disbursement,
            Amount = 0m,
            Notes = notes ?? "Recovery rule update",
            RecoveryType = recoveryType,
            RecoveryValue = recoveryValue,
            PostedByUserId = _userManager.GetUserId(User) ?? string.Empty,
            PostedAt = DateTimeOffset.UtcNow
        };
        _db.Advances.Add(rule);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Recovery rule updated.";
        return RedirectToAction(nameof(Index), new { farmerId });
    }
}
