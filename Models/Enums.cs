namespace KhoyaLedger.Models;

public enum MilkType
{
    Cow = 1,
    Buffalo = 2
}

public enum Shift
{
    Morning = 1,
    Evening = 2
}

public enum CollectionStatus
{
    Pending = 1,
    Posted = 2,
    Corrected = 3
}

public enum AdvanceType
{
    Disbursement = 1,
    DirectRepayment = 2,
    SettlementDeduction = 3
}

public enum RecoveryType
{
    None = 0,
    FixedPerLitre = 1,
    PercentOfGross = 2
}

public enum SettlementStatus
{
    Draft = 1,
    Posted = 2,
    PartiallyPaid = 3,
    FullyPaid = 4
}

public enum PaymentMode
{
    Cash = 1,
    UPI = 2,
    Bank = 3
}

public enum BatchStatus
{
    Draft = 1,
    Posted = 2
}

public enum StockMovementType
{
    Production = 1,
    Sale = 2,
    Spoilage = 3,
    Adjustment = 4
}

public enum SaleStatus
{
    Pending = 1,
    PartiallyPaid = 2,
    FullyPaid = 3
}

public enum DeliveryStatus
{
    Pending = 1,
    Delivered = 2
}
