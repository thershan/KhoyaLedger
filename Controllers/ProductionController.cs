using KhoyaLedger.Data;
using KhoyaLedger.Models;
using KhoyaLedger.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KhoyaLedger.Controllers;

[Authorize(Roles = "Owner")]
public class ProductionController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IStockService _stockService;
    private readonly UserManager<ApplicationUser> _userManager;

    public ProductionController(ApplicationDbContext db, IStockService stockService, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _stockService = stockService;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var batches = await _db.ProductionBatches.OrderByDescending(b => b.BatchDate).ToListAsync();
        var currentStock = await _stockService.GetCurrentStockKgAsync();
        ViewBag.CurrentStock = currentStock;
        return View(batches);
    }

    public IActionResult Create() => View(new ProductionBatch { BatchDate = DateOnly.FromDateTime(DateTime.Today) });

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductionBatch batch)
    {
        var dayCount = await _db.ProductionBatches.CountAsync(b => b.BatchDate == batch.BatchDate);
        batch.BatchNumber = $"B{batch.BatchDate:yyyyMMdd}{(dayCount + 1):D3}";
        batch.Status = BatchStatus.Draft;
        _db.ProductionBatches.Add(batch);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var batch = await _db.ProductionBatches.FindAsync(id);
        if (batch == null) return NotFound();
        if (batch.Status == BatchStatus.Posted) return BadRequest("Cannot edit a posted batch.");
        return View(batch);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ProductionBatch batch)
    {
        if (id != batch.Id) return BadRequest();
        batch.TotalCostRs = batch.WoodCostRs + batch.LabourCostRs + batch.TransportCostRs + batch.OtherCostRs;
        _db.ProductionBatches.Update(batch);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Post(int id)
    {
        var batch = await _db.ProductionBatches.FindAsync(id);
        if (batch == null) return NotFound();

        batch.TotalCostRs = Math.Round(batch.WoodCostRs + batch.LabourCostRs + batch.TransportCostRs + batch.OtherCostRs, 2, MidpointRounding.AwayFromZero);
        batch.CostPerKg = batch.KhoyaProducedKg > 0
            ? Math.Round(batch.TotalCostRs / batch.KhoyaProducedKg, 4, MidpointRounding.AwayFromZero)
            : 0m;
        batch.YieldPercent = batch.MilkUsedLitres > 0
            ? Math.Round((batch.KhoyaProducedKg / batch.MilkUsedLitres) * 100m, 2, MidpointRounding.AwayFromZero)
            : 0m;
        batch.Status = BatchStatus.Posted;
        batch.PostedAt = DateTimeOffset.UtcNow;
        batch.PostedByUserId = _userManager.GetUserId(User);

        await _db.SaveChangesAsync();
        var userId = _userManager.GetUserId(User) ?? string.Empty;
        await _stockService.PostProductionAsync(batch.Id, userId);
        TempData["Success"] = $"Batch {batch.BatchNumber} posted. Stock +{batch.KhoyaProducedKg:F2} kg.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Spoilage(int batchId, decimal kg, string? notes)
    {
        var userId = _userManager.GetUserId(User) ?? string.Empty;
        try
        {
            await _stockService.PostSpoilageAsync(batchId, kg, notes, userId);
            TempData["Success"] = $"Spoilage of {kg:F2} kg recorded.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Index));
    }
}
