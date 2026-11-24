using System.ComponentModel.DataAnnotations.Schema;

namespace KhoyaLedger.Models;

public class BatchMilkLink
{
    public int Id { get; set; }
    public int BatchId { get; set; }
    public ProductionBatch? Batch { get; set; }
    public int CollectionId { get; set; }
    public MilkCollection? Collection { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal LitresUsed { get; set; }
}
