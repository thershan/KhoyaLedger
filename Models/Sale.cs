using System.ComponentModel.DataAnnotations.Schema;

namespace KhoyaLedger.Models;

public class Sale
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public int BatchId { get; set; }
    public ProductionBatch? Batch { get; set; }

    public DateOnly SaleDate { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal QuantityKg { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal PricePerKg { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal PaidAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal OutstandingAmount { get; set; }

    public SaleStatus Status { get; set; } = SaleStatus.Pending;
    public DateOnly? DueDate { get; set; }
    public DeliveryStatus DeliveryStatus { get; set; } = DeliveryStatus.Pending;
    public string InvoiceNumber { get; set; } = string.Empty;

    public DateTimeOffset? PostedAt { get; set; }
    public string? PostedByUserId { get; set; }
    public ApplicationUser? PostedByUser { get; set; }

    public ICollection<SalePayment> Payments { get; set; } = new List<SalePayment>();
    public ICollection<KhoyaStockMovement> StockMovements { get; set; } = new List<KhoyaStockMovement>();
}
