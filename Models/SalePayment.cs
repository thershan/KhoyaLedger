using System.ComponentModel.DataAnnotations.Schema;

namespace KhoyaLedger.Models;

public class SalePayment
{
    public int Id { get; set; }
    public int SaleId { get; set; }
    public Sale? Sale { get; set; }

    public DateOnly PaidDate { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    public PaymentMode Mode { get; set; }
    public string? Notes { get; set; }

    public string PostedByUserId { get; set; } = string.Empty;
    public ApplicationUser? PostedByUser { get; set; }

    public DateTimeOffset PostedAt { get; set; } = DateTimeOffset.UtcNow;
}
