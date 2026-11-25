using KhoyaLedger.Data;
using KhoyaLedger.Models;
using KhoyaLedger.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KhoyaLedger.Controllers;

[ApiController]
[Route("api/mobile")]
public class MobileApiController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IMilkRateService _rateService;
    private readonly IStockService _stockService;

    public MobileApiController(ApplicationDbContext db, IMilkRateService rateService, IStockService stockService)
    {
        _db = db;
        _rateService = rateService;
        _stockService = stockService;
    }

    [HttpGet("ping")]
    public IActionResult Ping() => Ok(new { status = "online", timestamp = DateTime.UtcNow });

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var todayCollections = await _db.MilkCollections
            .Where(c => c.CollectionDate == today)
            .ToListAsync();

        var morningLitres = todayCollections.Where(c => c.Shift == Shift.Morning).Sum(c => c.Litres);
        var eveningLitres = todayCollections.Where(c => c.Shift == Shift.Evening).Sum(c => c.Litres);
        var totalGross = todayCollections.Sum(c => c.GrossMilkValue);

        var activeFarmers = await _db.Farmers.CountAsync(f => f.IsActive);
        var khoyaStock = await _stockService.GetCurrentStockKgAsync();

        return Ok(new
        {
            date = today,
            morningLitres,
            eveningLitres,
            totalLitres = morningLitres + eveningLitres,
            totalGross,
            activeFarmers,
            khoyaStock
        });
    }

    [HttpGet("farmers")]
    public async Task<IActionResult> GetFarmers()
    {
        var farmers = await _db.Farmers
            .Where(f => f.IsActive)
            .OrderBy(f => f.FarmerCode)
            .Select(f => new
            {
                f.Id,
                f.FarmerCode,
                f.Name,
                f.Village,
                mobile = f.Mobile,
                milkType = f.MilkType.ToString()
            })
            .ToListAsync();

        return Ok(farmers);
    }

    [HttpGet("rate-preview")]
    public async Task<IActionResult> PreviewRate([FromQuery] string milkType, [FromQuery] decimal fat)
    {
        if (!Enum.TryParse<MilkType>(milkType, true, out var mType))
        {
            return BadRequest(new { message = "Invalid milk type. Must be Cow or Buffalo." });
        }

        var today = DateOnly.FromDateTime(DateTime.Today);
        var (slab, rate) = await _rateService.GetRateAsync(mType, fat, today);

        return Ok(new
        {
            rate,
            slabId = slab?.Id,
            isFlat = slab?.IsFlat ?? false
        });
    }

    [HttpPost("collections")]
    public async Task<IActionResult> SaveCollection([FromBody] MobileCollectionDto dto)
    {
        var farmer = await _db.Farmers.FindAsync(dto.FarmerId);
        if (farmer == null) return NotFound(new { message = "Farmer not found." });

        var date = dto.Date ?? DateOnly.FromDateTime(DateTime.Today);
        if (!Enum.TryParse<Shift>(dto.Shift, true, out var shift))
        {
            shift = Shift.Morning;
        }

        var (slab, rate) = await _rateService.GetRateAsync(farmer.MilkType, dto.Fat, date);
        var grossValue = _rateService.ComputeGrossValue(dto.Litres, rate);

        var todayCount = await _db.MilkCollections.CountAsync(c => c.CollectionDate == date);
        var receiptNumber = $"RC{date:yyyyMMdd}{(todayCount + 1):D4}";

        var collection = new MilkCollection
        {
            FarmerId = dto.FarmerId,
            CollectionDate = date,
            Shift = shift,
            Litres = dto.Litres,
            Fat = dto.Fat,
            SnfPercent = dto.Snf,
            AppliedRateId = slab?.Id,
            AppliedRate = rate,
            GrossMilkValue = grossValue,
            ReceiptNumber = receiptNumber,
            Status = CollectionStatus.Pending
        };

        _db.MilkCollections.Add(collection);
        await _db.SaveChangesAsync();

        return Ok(new
        {
            id = collection.Id,
            receiptNumber = collection.ReceiptNumber,
            grossValue = collection.GrossMilkValue,
            appliedRate = collection.AppliedRate,
            farmerName = farmer.Name,
            farmerCode = farmer.FarmerCode,
            litres = collection.Litres,
            fat = collection.Fat,
            shift = collection.Shift.ToString()
        });
    }

    [HttpGet("collections/today")]
    public async Task<IActionResult> GetTodayCollections([FromQuery] string? shift)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var query = _db.MilkCollections
            .Include(c => c.Farmer)
            .Where(c => c.CollectionDate == today);

        if (!string.IsNullOrEmpty(shift) && Enum.TryParse<Shift>(shift, true, out var s))
        {
            query = query.Where(c => c.Shift == s);
        }

        var list = await query
            .OrderByDescending(c => c.Id)
            .Select(c => new
            {
                c.Id,
                c.ReceiptNumber,
                farmerCode = c.Farmer!.FarmerCode,
                farmerName = c.Farmer.Name,
                c.Litres,
                c.Fat,
                c.AppliedRate,
                c.GrossMilkValue,
                shift = c.Shift.ToString(),
                status = c.Status.ToString()
            })
            .ToListAsync();

        return Ok(list);
    }
}

public class MobileCollectionDto
{
    public int FarmerId { get; set; }
    public decimal Litres { get; set; }
    public decimal Fat { get; set; }
    public decimal? Snf { get; set; }
    public string Shift { get; set; } = "Morning";
    public DateOnly? Date { get; set; }
}
