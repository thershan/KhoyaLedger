using KhoyaLedger.Data;
using KhoyaLedger.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KhoyaLedger.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IStockService _stockService;

    public DashboardController(ApplicationDbContext db, IStockService stockService)
    {
        _db = db;
        _stockService = stockService;
    }

    public async Task<IActionResult> Index(DateOnly? fromDate, DateOnly? toDate)
    {
        fromDate ??= DateOnly.FromDateTime(DateTime.Today.AddDays(-30));
        toDate ??= DateOnly.FromDateTime(DateTime.Today);

        // Milk
        var collectionQuery = _db.MilkCollections
            .Where(c => c.Status == KhoyaLedger.Models.CollectionStatus.Posted
                     && c.CollectionDate >= fromDate && c.CollectionDate <= toDate);
        var totalLitres = await collectionQuery.SumAsync(c => (decimal?)c.Litres) ?? 0m;
        var totalMilkValue = await collectionQuery.SumAsync(c => (decimal?)c.GrossMilkValue) ?? 0m;

        // Farmers
        var totalAdvanceBalance = await _db.Farmers.SumAsync(f => (decimal?)f.AdvanceBalance) ?? 0m;
        var totalMilkPayable = await _db.Farmers.SumAsync(f => (decimal?)f.MilkPayable) ?? 0m;

        // Production
        var productionQuery = _db.ProductionBatches
            .Where(b => b.Status == KhoyaLedger.Models.BatchStatus.Posted
                     && b.BatchDate >= fromDate && b.BatchDate <= toDate);
        var totalKhoyaProduced = await productionQuery.SumAsync(b => (decimal?)b.KhoyaProducedKg) ?? 0m;
        var currentStock = await _stockService.GetCurrentStockKgAsync();

        // Customer receivables
        var totalOutstanding = await _db.Sales.SumAsync(s => (decimal?)s.OutstandingAmount) ?? 0m;
        var overdueOutstanding = await _db.Sales
            .Where(s => s.DueDate != null && s.DueDate < DateOnly.FromDateTime(DateTime.Today) && s.OutstandingAmount > 0)
            .SumAsync(s => (decimal?)s.OutstandingAmount) ?? 0m;

        // Cash movements
        var settPayments = await _db.SettlementPayments
            .Where(p => p.PaidDate >= fromDate && p.PaidDate <= toDate)
            .SumAsync(p => (decimal?)p.Amount) ?? 0m;
        var salePayments = await _db.SalePayments
            .Where(p => p.PaidDate >= fromDate && p.PaidDate <= toDate)
            .SumAsync(p => (decimal?)p.Amount) ?? 0m;

        ViewBag.FromDate = fromDate;
        ViewBag.ToDate = toDate;
        ViewBag.TotalLitres = totalLitres;
        ViewBag.TotalMilkValue = totalMilkValue;
        ViewBag.TotalAdvanceBalance = totalAdvanceBalance;
        ViewBag.TotalMilkPayable = totalMilkPayable;
        ViewBag.TotalKhoyaProduced = totalKhoyaProduced;
        ViewBag.CurrentStock = currentStock;
        ViewBag.TotalOutstanding = totalOutstanding;
        ViewBag.OverdueOutstanding = overdueOutstanding;
        ViewBag.SettPayments = settPayments;
        ViewBag.SalePayments = salePayments;

        return View();
    }
}
