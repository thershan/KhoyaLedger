using System.ComponentModel.DataAnnotations.Schema;

namespace KhoyaLedger.Models;

public class SettlementPayment
{
    public int Id { get; set; }
    public int SettlementId { get; set; }
    public FarmerSettlement? Settlement { get; set; }

    public DateOnly PaidDate { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    public PaymentMode PaymentMode { get; set; }
    public string? Notes { get; set; }

    public string PostedByUserId { get; set; } = string.Empty;
    public ApplicationUser? PostedByUser { get; set; }

    public DateTimeOffset PostedAt { get; set; } = DateTimeOffset.UtcNow;
}
