using KhoyaLedger.Models;

namespace KhoyaLedger.Services;

public interface IMilkRateService
{
    Task<(MilkRateSlab? slab, decimal rate)> GetRateAsync(MilkType milkType, decimal fat, DateOnly date);
    decimal ComputeGrossValue(decimal litres, decimal rate);
}
