using KhoyaLedger.Data;
using KhoyaLedger.Models;
using Microsoft.EntityFrameworkCore;

namespace KhoyaLedger.Services;

public class MilkRateService : IMilkRateService
{
    private readonly ApplicationDbContext _db;

    public MilkRateService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<(MilkRateSlab? slab, decimal rate)> GetRateAsync(MilkType milkType, decimal fat, DateOnly date)
    {
        var dateTime = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

        // Fetch by EffectiveFrom server-side; apply nullable EffectiveTo check in memory
        var slabs = await _db.MilkRateSlabs
            .Where(s => s.MilkType == milkType && s.EffectiveFrom <= dateTime)
            .OrderByDescending(s => s.EffectiveFrom)
            .ToListAsync();

        slabs = slabs
            .Where(s => s.EffectiveTo == null || s.EffectiveTo >= dateTime)
            .ToList();

        // Flat rate slabs first, then fat-based
        var flatSlab = slabs.Where(s => s.IsFlat).FirstOrDefault();
        var fatSlab = slabs.Where(s => !s.IsFlat
                                      && s.MinFat.HasValue && s.MaxFat.HasValue
                                      && fat >= s.MinFat.Value && fat <= s.MaxFat.Value)
                           .FirstOrDefault();

        // Prefer fat-based if matched, else flat
        MilkRateSlab? chosen = fatSlab ?? flatSlab;
        if (chosen == null) return (null, 0m);

        decimal rate = chosen.IsFlat
            ? (chosen.FlatRate ?? 0m)
            : (chosen.Rate ?? 0m);

        return (chosen, rate);
    }

    public decimal ComputeGrossValue(decimal litres, decimal rate)
    {
        return Math.Round(litres * rate, 2, MidpointRounding.AwayFromZero);
    }
}
