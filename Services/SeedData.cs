using KhoyaLedger.Data;
using KhoyaLedger.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace KhoyaLedger.Services;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var db = services.GetRequiredService<ApplicationDbContext>();

        // Create roles
        foreach (var role in new[] { "Owner", "Collector" })
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        // Create owner user
        var ownerEmail = "owner@khoya";
        var ownerUser = await userManager.FindByNameAsync(ownerEmail);
        if (ownerUser == null)
        {
            ownerUser = new ApplicationUser
            {
                UserName = ownerEmail,
                Email = ownerEmail,
                DisplayName = "मालक (Owner)",
                IsActive = true,
                EmailConfirmed = true
            };
            var result = await userManager.CreateAsync(ownerUser, "Owner@123");
            if (result.Succeeded)
                await userManager.AddToRoleAsync(ownerUser, "Owner");
        }

        // Demo farmer
        if (!await db.Farmers.AnyAsync(f => f.FarmerCode == "F001"))
        {
            db.Farmers.Add(new Farmer
            {
                FarmerCode = "F001",
                Name = "राम पाटील",
                Village = "Shirpur",
                Mobile = "9999999999",
                MilkType = MilkType.Buffalo,
                IsActive = true,
                AdvanceBalance = 0m,
                MilkPayable = 0m,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            });
        }

        // Demo rate slab: flat ₹42/L Buffalo from 2024-01-01
        if (!await db.MilkRateSlabs.AnyAsync())
        {
            db.MilkRateSlabs.Add(new MilkRateSlab
            {
                MilkType = MilkType.Buffalo,
                IsFlat = true,
                FlatRate = 42m,
                EffectiveFrom = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                EffectiveTo = null,
                CreatedByUserId = ownerUser?.Id ?? string.Empty
            });
        }

        // Demo customer
        if (!await db.Customers.AnyAsync(c => c.Name == "रमेश व्यापारी"))
        {
            db.Customers.Add(new Customer
            {
                Name = "रमेश व्यापारी",
                Mobile = "8888888888",
                Address = "Main Market, Shirpur",
                IsActive = true,
                OpeningReceivable = 0m,
                CreatedAt = DateTimeOffset.UtcNow
            });
        }

        await db.SaveChangesAsync();
    }
}
