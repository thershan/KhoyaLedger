namespace KhoyaLedger.Models;

public class SettlementCollection
{
    public int Id { get; set; }
    public int SettlementId { get; set; }
    public FarmerSettlement? Settlement { get; set; }
    public int CollectionId { get; set; }
    public MilkCollection? Collection { get; set; }
}
