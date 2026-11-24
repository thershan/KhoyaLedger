using System.ComponentModel.DataAnnotations.Schema;

namespace KhoyaLedger.Models;

public class FarmerSettlement
{
    public int Id { get; set; }
    public int FarmerId { get; set; }
    public Farmer? Farmer { get; set; }

    public DateOnly PeriodFrom { get; set; }
    public DateOnly PeriodTo { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal GrossMilkValue { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal AdvanceRecovered { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal NetPayable { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal AmountPaid { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal OutstandingPayable { get; set; }

    public SettlementStatus Status { get; set; } = SettlementStatus.Draft;

    public DateTimeOffset? PostedAt { get; set; }
    public string? PostedByUserId { get; set; }
    public ApplicationUser? PostedByUser { get; set; }

    public ICollection<SettlementCollection> SettlementCollections { get; set; } = new List<SettlementCollection>();
    public ICollection<SettlementPayment> Payments { get; set; } = new List<SettlementPayment>();
    public ICollection<Advance> Advances { get; set; } = new List<Advance>();
}
