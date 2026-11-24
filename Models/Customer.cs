using System.ComponentModel.DataAnnotations.Schema;

namespace KhoyaLedger.Models;

public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Mobile { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    [Column(TypeName = "decimal(18,2)")]
    public decimal OpeningReceivable { get; set; } = 0m;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public ICollection<Sale> Sales { get; set; } = new List<Sale>();
}
