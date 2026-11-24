using System.ComponentModel.DataAnnotations.Schema;

namespace KhoyaLedger.Models;

public class KhoyaStockMovement
{
    public int Id { get; set; }
    public DateOnly MovementDate { get; set; }
    public StockMovementType Type { get; set; }
    public int? BatchId { get; set; }
    public ProductionBatch? Batch { get; set; }
    public int? SaleId { get; set; }
    public Sale? Sale { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal QuantityKg { get; set; }

    public string? Notes { get; set; }

    public string PostedByUserId { get; set; } = string.Empty;
    public ApplicationUser? PostedByUser { get; set; }

    public DateTimeOffset PostedAt { get; set; } = DateTimeOffset.UtcNow;
}
