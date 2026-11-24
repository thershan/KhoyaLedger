using KhoyaLedger.Data;
using KhoyaLedger.Models;
using KhoyaLedger.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KhoyaLedger.Controllers;

[Authorize(Roles = "Owner")]
public class SalesController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IStockService _stockService;
    private readonly UserManager<ApplicationUser> _userManager;

    public SalesController(ApplicationDbContext db, IStockService stockService, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _stockService = stockService;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var sales = await _db.Sales
            .Include(s => s.Customer)
            .Include(s => s.Batch)
            .OrderByDescending(s => s.SaleDate)
            .ToListAsync();
        return View(sales);
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.Customers = await _db.Customers.Where(c => c.IsActive).OrderBy(c => c.Name).ToListAsync();
        ViewBag.Batches = await _db.ProductionBatches.Where(b => b.Status == BatchStatus.Posted).OrderByDescending(b => b.BatchDate).ToListAsync();
        ViewBag.CurrentStock = await _stockService.GetCurrentStockKgAsync();
        return View(new Sale { SaleDate = DateOnly.FromDateTime(DateTime.Today) });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Sale sale)
    {
        var currentStock = await _stockService.GetCurrentStockKgAsync();
        if (sale.QuantityKg > currentStock)
        {
            ModelState.AddModelError("QuantityKg", $"Insufficient stock. Available: {currentStock:F2} kg.");
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Customers = await _db.Customers.Where(c => c.IsActive).OrderBy(c => c.Name).ToListAsync();
            ViewBag.Batches = await _db.ProductionBatches.Where(b => b.Status == BatchStatus.Posted).OrderByDescending(b => b.BatchDate).ToListAsync();
            ViewBag.CurrentStock = currentStock;
            return View(sale);
        }

        var dayCount = await _db.Sales.CountAsync(s => s.SaleDate == sale.SaleDate);
        sale.InvoiceNumber = $"INV{sale.SaleDate:yyyyMMdd}{(dayCount + 1):D4}";
        sale.TotalAmount = Math.Round(sale.QuantityKg * sale.PricePerKg, 2, MidpointRounding.AwayFromZero);
        sale.OutstandingAmount = sale.TotalAmount;
        sale.Status = SaleStatus.Pending;
        _db.Sales.Add(sale);
        await _db.SaveChangesAsync();
        TempData["Success"] = $"Sale {sale.InvoiceNumber} created.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Post(int id)
    {
        var userId = _userManager.GetUserId(User) ?? string.Empty;
        try
        {
            await _stockService.PostSaleDispatchAsync(id, userId);
            TempData["Success"] = "Sale dispatched. Stock updated.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> RecordPayment(int saleId)
    {
        var sale = await _db.Sales.Include(s => s.Customer).FirstOrDefaultAsync(s => s.Id == saleId);
        if (sale == null) return NotFound();
        return View(sale);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> RecordPayment(int saleId, decimal amount, PaymentMode mode, string? notes)
    {
        using var tx = await _db.Database.BeginTransactionAsync();
        try
        {
            var sale = await _db.Sales.FindAsync(saleId)
                ?? throw new InvalidOperationException("Sale not found.");

            var payment = new SalePayment
            {
                SaleId = saleId,
                PaidDate = DateOnly.FromDateTime(DateTime.Today),
                Amount = amount,
                Mode = mode,
                Notes = notes,
                PostedByUserId = _userManager.GetUserId(User) ?? string.Empty,
                PostedAt = DateTimeOffset.UtcNow
            };
            _db.SalePayments.Add(payment);

            sale.PaidAmount = Math.Round(sale.PaidAmount + amount, 2, MidpointRounding.AwayFromZero);
            sale.OutstandingAmount = Math.Round(sale.OutstandingAmount - amount, 2, MidpointRounding.AwayFromZero);
            if (sale.OutstandingAmount <= 0)
            {
                sale.OutstandingAmount = 0m;
                sale.Status = SaleStatus.FullyPaid;
            }
            else
            {
                sale.Status = SaleStatus.PartiallyPaid;
            }

            await _db.SaveChangesAsync();
            await tx.CommitAsync();
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }

        TempData["Success"] = $"Payment of ₹{amount:F2} recorded.";
        return RedirectToAction(nameof(Index));
    }
}
