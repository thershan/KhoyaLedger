using KhoyaLedger.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace KhoyaLedger.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Farmer> Farmers => Set<Farmer>();
    public DbSet<MilkRateSlab> MilkRateSlabs => Set<MilkRateSlab>();
    public DbSet<MilkCollection> MilkCollections => Set<MilkCollection>();
    public DbSet<Advance> Advances => Set<Advance>();
    public DbSet<FarmerSettlement> FarmerSettlements => Set<FarmerSettlement>();
    public DbSet<SettlementCollection> SettlementCollections => Set<SettlementCollection>();
    public DbSet<SettlementPayment> SettlementPayments => Set<SettlementPayment>();
    public DbSet<ProductionBatch> ProductionBatches => Set<ProductionBatch>();
    public DbSet<BatchMilkLink> BatchMilkLinks => Set<BatchMilkLink>();
    public DbSet<KhoyaStockMovement> KhoyaStockMovements => Set<KhoyaStockMovement>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Sale> Sales => Set<Sale>();
    public DbSet<SalePayment> SalePayments => Set<SalePayment>();
    public DbSet<ShiftTally> ShiftTallies => Set<ShiftTally>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Unique indexes
        builder.Entity<Farmer>().HasIndex(f => f.FarmerCode).IsUnique();
        builder.Entity<MilkCollection>().HasIndex(m => m.ReceiptNumber).IsUnique();
        builder.Entity<ProductionBatch>().HasIndex(p => p.BatchNumber).IsUnique();
        builder.Entity<Sale>().HasIndex(s => s.InvoiceNumber).IsUnique();

        // Decimal precision
        builder.Entity<MilkCollection>().Property(m => m.GrossMilkValue).HasPrecision(18, 2);
        builder.Entity<MilkCollection>().Property(m => m.AppliedRate).HasPrecision(18, 4);
        builder.Entity<Farmer>().Property(f => f.AdvanceBalance).HasPrecision(18, 2);
        builder.Entity<Farmer>().Property(f => f.MilkPayable).HasPrecision(18, 2);
        builder.Entity<Sale>().Property(s => s.TotalAmount).HasPrecision(18, 2);
        builder.Entity<Sale>().Property(s => s.PaidAmount).HasPrecision(18, 2);
        builder.Entity<Sale>().Property(s => s.OutstandingAmount).HasPrecision(18, 2);
        builder.Entity<FarmerSettlement>().Property(f => f.GrossMilkValue).HasPrecision(18, 2);
        builder.Entity<FarmerSettlement>().Property(f => f.AdvanceRecovered).HasPrecision(18, 2);
        builder.Entity<FarmerSettlement>().Property(f => f.NetPayable).HasPrecision(18, 2);
        builder.Entity<FarmerSettlement>().Property(f => f.AmountPaid).HasPrecision(18, 2);
        builder.Entity<FarmerSettlement>().Property(f => f.OutstandingPayable).HasPrecision(18, 2);

        // Self-referential FK for MilkCollection corrections
        builder.Entity<MilkCollection>()
            .HasOne(m => m.OriginalCollection)
            .WithMany()
            .HasForeignKey(m => m.OriginalCollectionId)
            .OnDelete(DeleteBehavior.Restrict);

        // Advance -> FarmerSettlement (avoid cycles)
        builder.Entity<Advance>()
            .HasOne(a => a.RelatedSettlement)
            .WithMany(s => s.Advances)
            .HasForeignKey(a => a.RelatedSettlementId)
            .OnDelete(DeleteBehavior.Restrict);

        // KhoyaStockMovement -> Sale
        builder.Entity<KhoyaStockMovement>()
            .HasOne(k => k.Sale)
            .WithMany(s => s.StockMovements)
            .HasForeignKey(k => k.SaleId)
            .OnDelete(DeleteBehavior.Restrict);

        // KhoyaStockMovement -> ProductionBatch
        builder.Entity<KhoyaStockMovement>()
            .HasOne(k => k.Batch)
            .WithMany(b => b.StockMovements)
            .HasForeignKey(k => k.BatchId)
            .OnDelete(DeleteBehavior.Restrict);

        // Sale -> ProductionBatch
        builder.Entity<Sale>()
            .HasOne(s => s.Batch)
            .WithMany(b => b.Sales)
            .HasForeignKey(s => s.BatchId)
            .OnDelete(DeleteBehavior.Restrict);

        // MilkCollection -> ApplicationUser (CollectedBy) - no cascade
        builder.Entity<MilkCollection>()
            .HasOne(m => m.CollectedByUser)
            .WithMany()
            .HasForeignKey(m => m.CollectedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<MilkCollection>()
            .HasOne(m => m.PostedByUser)
            .WithMany()
            .HasForeignKey(m => m.PostedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
