using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KhoyaLedger.Models;

public class Farmer
{
    public int Id { get; set; }

    [Required]
    public string FarmerCode { get; set; } = string.Empty;

    [Required]
    public string Name { get; set; } = string.Empty;

    public string Village { get; set; } = string.Empty;
    public string Mobile { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public MilkType MilkType { get; set; } = MilkType.Cow;

    [Column(TypeName = "decimal(18,2)")]
    public decimal AdvanceBalance { get; set; } = 0m;

    [Column(TypeName = "decimal(18,2)")]
    public decimal MilkPayable { get; set; } = 0m;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    // Navigation
    public ICollection<MilkCollection> MilkCollections { get; set; } = new List<MilkCollection>();
    public ICollection<Advance> Advances { get; set; } = new List<Advance>();
    public ICollection<FarmerSettlement> Settlements { get; set; } = new List<FarmerSettlement>();
}
