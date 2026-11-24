using System.ComponentModel.DataAnnotations.Schema;

namespace KhoyaLedger.Models;

public class ShiftTally
{
    public int Id { get; set; }
    public DateOnly TallyDate { get; set; }
    public Shift Shift { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal MeasuredLitres { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal SumFromEntries { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Variance { get; set; }

    public string? Notes { get; set; }

    public string EnteredByUserId { get; set; } = string.Empty;
    public ApplicationUser? EnteredByUser { get; set; }
}
