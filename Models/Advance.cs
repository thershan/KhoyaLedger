using System.ComponentModel.DataAnnotations.Schema;

namespace KhoyaLedger.Models;

public class Advance
{
    public int Id { get; set; }
    public int FarmerId { get; set; }
    public Farmer? Farmer { get; set; }

    public DateOnly TransactionDate { get; set; }
    public AdvanceType Type { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    public string? Notes { get; set; }

    public RecoveryType RecoveryType { get; set; } = RecoveryType.None;

    [Column(TypeName = "decimal(18,4)")]
    public decimal? RecoveryValue { get; set; }

    public string PostedByUserId { get; set; } = string.Empty;
    public ApplicationUser? PostedByUser { get; set; }

    public DateTimeOffset PostedAt { get; set; } = DateTimeOffset.UtcNow;

    public int? RelatedSettlementId { get; set; }
    public FarmerSettlement? RelatedSettlement { get; set; }
}
