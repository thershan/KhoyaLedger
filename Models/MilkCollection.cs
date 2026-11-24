using System.ComponentModel.DataAnnotations.Schema;

namespace KhoyaLedger.Models;

public class MilkCollection
{
    public int Id { get; set; }
    public int FarmerId { get; set; }
    public Farmer? Farmer { get; set; }

    public DateOnly CollectionDate { get; set; }
    public Shift Shift { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Litres { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Fat { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? SnfPercent { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? MeasuredWeightKg { get; set; }

    public int? AppliedRateId { get; set; }
    public MilkRateSlab? AppliedRateSlab { get; set; }

    [Column(TypeName = "decimal(18,4)")]
    public decimal AppliedRate { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal GrossMilkValue { get; set; }

    public CollectionStatus Status { get; set; } = CollectionStatus.Pending;
    public string ReceiptNumber { get; set; } = string.Empty;

    public string CollectedByUserId { get; set; } = string.Empty;
    public ApplicationUser? CollectedByUser { get; set; }

    public DateTimeOffset? PostedAt { get; set; }
    public string? PostedByUserId { get; set; }
    public ApplicationUser? PostedByUser { get; set; }

    public string? Notes { get; set; }
    public string? CorrectionReason { get; set; }

    public int? OriginalCollectionId { get; set; }
    public MilkCollection? OriginalCollection { get; set; }
}
