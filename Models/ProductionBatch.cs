using System.ComponentModel.DataAnnotations.Schema;

namespace KhoyaLedger.Models;

public class ProductionBatch
{
    public int Id { get; set; }
    public DateOnly BatchDate { get; set; }
    public string BatchNumber { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,2)")]
    public decimal MilkUsedLitres { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal KhoyaProducedKg { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal WoodCostRs { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal LabourCostRs { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TransportCostRs { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal OtherCostRs { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalCostRs { get; set; }

    [Column(TypeName = "decimal(18,4)")]
    public decimal CostPerKg { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal YieldPercent { get; set; }

    public BatchStatus Status { get; set; } = BatchStatus.Draft;
    public string? Notes { get; set; }

    public DateTimeOffset? PostedAt { get; set; }
    public string? PostedByUserId { get; set; }
    public ApplicationUser? PostedByUser { get; set; }

    public ICollection<BatchMilkLink> BatchMilkLinks { get; set; } = new List<BatchMilkLink>();
    public ICollection<Sale> Sales { get; set; } = new List<Sale>();
    public ICollection<KhoyaStockMovement> StockMovements { get; set; } = new List<KhoyaStockMovement>();
}
