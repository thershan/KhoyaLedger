using KhoyaLedger.Data;
using KhoyaLedger.Models;
using Microsoft.EntityFrameworkCore;

namespace KhoyaLedger.Services;

public class StockService : IStockService
{
    private readonly ApplicationDbContext _db;

    public StockService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<decimal> GetCurrentStockKgAsync()
    {
        return await _db.KhoyaStockMovements.SumAsync(m => (decimal?)m.QuantityKg) ?? 0m;
    }

    public async Task PostProductionAsync(int batchId, string userId)
    {
        var batch = await _db.ProductionBatches.FindAsync(batchId)
            ?? throw new InvalidOperationException("Batch not found.");

        var movement = new KhoyaStockMovement
        {
            MovementDate = DateOnly.FromDateTime(DateTime.Today),
            Type = StockMovementType.Production,
            BatchId = batchId,
            QuantityKg = batch.KhoyaProducedKg,
            Notes = $"Batch {batch.BatchNumber}",
            PostedByUserId = userId,
            PostedAt = DateTimeOffset.UtcNow
        };
        _db.KhoyaStockMovements.Add(movement);
        await _db.SaveChangesAsync();
    }

    public async Task PostSaleDispatchAsync(int saleId, string userId)
    {
        var sale = await _db.Sales.FindAsync(saleId)
            ?? throw new InvalidOperationException("Sale not found.");

        var currentStock = await GetCurrentStockKgAsync();
        if (currentStock < sale.QuantityKg)
            throw new InvalidOperationException($"Insufficient stock. Available: {currentStock:F2} kg, Required: {sale.QuantityKg:F2} kg.");

        var movement = new KhoyaStockMovement
        {
            MovementDate = DateOnly.FromDateTime(DateTime.Today),
            Type = StockMovementType.Sale,
            SaleId = saleId,
            QuantityKg = -sale.QuantityKg,
            Notes = $"Sale {sale.InvoiceNumber}",
            PostedByUserId = userId,
            PostedAt = DateTimeOffset.UtcNow
        };
        _db.KhoyaStockMovements.Add(movement);

        sale.DeliveryStatus = DeliveryStatus.Delivered;
        sale.PostedAt = DateTimeOffset.UtcNow;
        sale.PostedByUserId = userId;
        await _db.SaveChangesAsync();
    }

    public async Task PostSpoilageAsync(int batchId, decimal kg, string? notes, string userId)
    {
        var currentStock = await GetCurrentStockKgAsync();
        if (currentStock < kg)
            throw new InvalidOperationException($"Insufficient stock for spoilage. Available: {currentStock:F2} kg.");

        var movement = new KhoyaStockMovement
        {
            MovementDate = DateOnly.FromDateTime(DateTime.Today),
            Type = StockMovementType.Spoilage,
            BatchId = batchId,
            QuantityKg = -kg,
            Notes = notes,
            PostedByUserId = userId,
            PostedAt = DateTimeOffset.UtcNow
        };
        _db.KhoyaStockMovements.Add(movement);
        await _db.SaveChangesAsync();
    }
}
