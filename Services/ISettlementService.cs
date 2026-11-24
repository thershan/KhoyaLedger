using KhoyaLedger.Models;
using KhoyaLedger.Data;

namespace KhoyaLedger.Services;

public interface ISettlementService
{
    Task<FarmerSettlement> CreateDraftAsync(int farmerId, DateOnly fromDate, DateOnly toDate);
    Task PostSettlementAsync(int settlementId, string postedByUserId);
    Task RecordPaymentAsync(int settlementId, decimal amount, PaymentMode mode, string userId, DateOnly date, string? notes);
}
