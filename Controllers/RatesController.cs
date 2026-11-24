using KhoyaLedger.Data;
using KhoyaLedger.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KhoyaLedger.Controllers;

[Authorize(Roles = "Owner")]
public class RatesController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public RatesController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var slabs = await _db.MilkRateSlabs.OrderByDescending(s => s.EffectiveFrom).ToListAsync();
        return View(slabs);
    }

    public IActionResult Create() => View(new MilkRateSlab { EffectiveFrom = DateTime.UtcNow });

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MilkRateSlab slab)
    {
        slab.CreatedByUserId = _userManager.GetUserId(User) ?? string.Empty;
        _db.MilkRateSlabs.Add(slab);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var slab = await _db.MilkRateSlabs.FindAsync(id);
        if (slab == null) return NotFound();
        return View(slab);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, MilkRateSlab slab)
    {
        if (id != slab.Id) return BadRequest();
        _db.MilkRateSlabs.Update(slab);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var slab = await _db.MilkRateSlabs.FindAsync(id);
        if (slab != null)
        {
            _db.MilkRateSlabs.Remove(slab);
            await _db.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }
}
