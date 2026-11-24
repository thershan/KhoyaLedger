using KhoyaLedger.Data;
using KhoyaLedger.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KhoyaLedger.Controllers;

[Authorize]
public class CustomersController : Controller
{
    private readonly ApplicationDbContext _db;

    public CustomersController(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var customers = await _db.Customers.OrderBy(c => c.Name).ToListAsync();
        return View(customers);
    }

    [Authorize(Roles = "Owner")]
    public IActionResult Create() => View(new Customer());

    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Owner")]
    public async Task<IActionResult> Create(Customer customer)
    {
        customer.CreatedAt = DateTimeOffset.UtcNow;
        _db.Customers.Add(customer);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Owner")]
    public async Task<IActionResult> Edit(int id)
    {
        var customer = await _db.Customers.FindAsync(id);
        if (customer == null) return NotFound();
        return View(customer);
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Owner")]
    public async Task<IActionResult> Edit(int id, Customer customer)
    {
        if (id != customer.Id) return BadRequest();
        _db.Customers.Update(customer);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Statement(int id, DateOnly? fromDate, DateOnly? toDate)
    {
        fromDate ??= DateOnly.FromDateTime(DateTime.Today.AddMonths(-3));
        toDate ??= DateOnly.FromDateTime(DateTime.Today);

        var customer = await _db.Customers.FindAsync(id);
        if (customer == null) return NotFound();

        var sales = await _db.Sales
            .Where(s => s.CustomerId == id && s.SaleDate >= fromDate && s.SaleDate <= toDate)
            .Include(s => s.Payments)
            .OrderBy(s => s.SaleDate)
            .ToListAsync();

        ViewBag.Customer = customer;
        ViewBag.FromDate = fromDate;
        ViewBag.ToDate = toDate;
        ViewBag.TotalOutstanding = await _db.Sales.Where(s => s.CustomerId == id).SumAsync(s => (decimal?)s.OutstandingAmount) ?? 0m;
        return View(sales);
    }
}
