namespace KhoyaLedger.Services;

public interface IStockService
{
    Task<decimal> GetCurrentStockKgAsync();
    Task PostProductionAsync(int batchId, string userId);
    Task PostSaleDispatchAsync(int saleId, string userId);
    Task PostSpoilageAsync(int batchId, decimal kg, string? notes, string userId);
}
