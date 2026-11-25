using KhoyaLedger.Data;
using KhoyaLedger.Models;
using KhoyaLedger.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// DB - SQLite
var dbPath = Path.Combine(builder.Environment.ContentRootPath, "khoya.db");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite($"Data Source={dbPath}"));

// Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders()
.AddDefaultUI();

// Fix login/logout paths to match the scaffolded Identity Razor Pages area
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Logout";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// App services
builder.Services.AddScoped<IMilkRateService, MilkRateService>();
builder.Services.AddScoped<ISettlementService, SettlementService>();
builder.Services.AddScoped<IStockService, StockService>();

var app = builder.Build();

// Migrate and seed
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
    await SeedData.InitializeAsync(scope.ServiceProvider);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseRouting();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

// Minimal API: Farmer search
app.MapGet("/api/farmers/search", async (string q, ApplicationDbContext db) =>
{
    var farmers = await db.Farmers
        .Where(f => f.IsActive && (f.FarmerCode.Contains(q) || f.Name.Contains(q)))
        .Take(10)
        .Select(f => new { f.Id, f.FarmerCode, f.Name, f.Village, f.MilkType })
        .ToListAsync();
    return Results.Ok(farmers);
}).RequireAuthorization();

app.MapFallbackToFile("mobile/{*path:nonfile}", "mobile/index.html");

app.Run();
