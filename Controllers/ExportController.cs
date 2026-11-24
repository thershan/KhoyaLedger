using CsvHelper;
using KhoyaLedger.Data;
using KhoyaLedger.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace KhoyaLedger.Controllers;

[Authorize(Roles = "Owner")]
public class ExportController : Controller
{
    private readonly ApplicationDbContext _db;

    public ExportController(ApplicationDbContext db)
    {
        _db = db;
    }

    public IActionResult Index() => View();

    public async Task<IActionResult> ExportMilkCsv(DateOnly fromDate, DateOnly toDate)
    {
        var collections = await _db.MilkCollections
            .Include(c => c.Farmer)
            .Where(c => c.CollectionDate >= fromDate && c.CollectionDate <= toDate && c.Status == CollectionStatus.Posted)
            .OrderBy(c => c.CollectionDate).ThenBy(c => c.Farmer!.FarmerCode)
            .Select(c => new
            {
                c.ReceiptNumber,
                c.CollectionDate,
                Shift = c.Shift.ToString(),
                FarmerCode = c.Farmer != null ? c.Farmer.FarmerCode : "",
                FarmerName = c.Farmer != null ? c.Farmer.Name : "",
                c.Litres,
                c.Fat,
                c.SnfPercent,
                c.AppliedRate,
                c.GrossMilkValue
            })
            .ToListAsync();

        using var ms = new MemoryStream();
        using var writer = new StreamWriter(ms);
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
        csv.WriteRecords(collections);
        await writer.FlushAsync();
        return File(ms.ToArray(), "text/csv", $"MilkCollections_{fromDate:yyyyMMdd}_{toDate:yyyyMMdd}.csv");
    }

    public async Task<IActionResult> ExportFarmerStatement(int id, DateOnly fromDate, DateOnly toDate)
    {
        var farmer = await _db.Farmers.FindAsync(id);
        if (farmer == null) return NotFound();

        var collections = await _db.MilkCollections
            .Where(c => c.FarmerId == id && c.CollectionDate >= fromDate && c.CollectionDate <= toDate && c.Status == CollectionStatus.Posted)
            .OrderBy(c => c.CollectionDate)
            .Select(c => new
            {
                c.CollectionDate,
                Shift = c.Shift.ToString(),
                c.Litres,
                c.Fat,
                c.AppliedRate,
                c.GrossMilkValue,
                c.ReceiptNumber
            })
            .ToListAsync();

        using var ms = new MemoryStream();
        using var writer = new StreamWriter(ms);
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
        csv.WriteRecords(collections);
        await writer.FlushAsync();
        return File(ms.ToArray(), "text/csv", $"FarmerStatement_{farmer.FarmerCode}_{fromDate:yyyyMMdd}_{toDate:yyyyMMdd}.csv");
    }
}
