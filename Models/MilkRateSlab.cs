using System.ComponentModel.DataAnnotations.Schema;

namespace KhoyaLedger.Models;

public class MilkRateSlab
{
    public int Id { get; set; }
    public MilkType MilkType { get; set; }
    public bool IsFlat { get; set; }

    [Column(TypeName = "decimal(18,4)")]
    public decimal? FlatRate { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? MinFat { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? MaxFat { get; set; }

    [Column(TypeName = "decimal(18,4)")]
    public decimal? Rate { get; set; }

    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }

    public string CreatedByUserId { get; set; } = string.Empty;
    public ApplicationUser? CreatedByUser { get; set; }
}
