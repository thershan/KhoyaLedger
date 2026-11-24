using KhoyaLedger.Data;
using KhoyaLedger.Models;
using Microsoft.EntityFrameworkCore;

namespace KhoyaLedger.Services;

public class SettlementService : ISettlementService
{
    private readonly ApplicationDbContext _db;

    public SettlementService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<FarmerSettlement> CreateDraftAsync(int farmerId, DateOnly fromDate, DateOnly toDate)
    {
        var farmer = await _db.Farmers.FindAsync(farmerId)
            ?? throw new InvalidOperationException($"Farmer {farmerId} not found.");

        // Find posted collections not in an active settlement
        var alreadySettled = await _db.SettlementCollections
            .Where(sc => sc.Settlement != null &&
                         (sc.Settlement.Status == SettlementStatus.Posted ||
                          sc.Settlement.Status == SettlementStatus.PartiallyPaid ||
                          sc.Settlement.Status == SettlementStatus.FullyPaid))
            .Select(sc => sc.CollectionId)
            .ToListAsync();

        var collections = await _db.MilkCollections
            .Where(c => c.FarmerId == farmerId
                     && c.Status == CollectionStatus.Posted
                     && c.CollectionDate >= fromDate
                     && c.CollectionDate <= toDate
                     && !alreadySettled.Contains(c.Id))
            .ToListAsync();

        var grossMilkValue = collections.Sum(c => c.GrossMilkValue);

        // Find active recovery rule (last Advance with RecoveryType != None)
        var recoveryRule = await _db.Advances
            .Where(a => a.FarmerId == farmerId && a.RecoveryType != RecoveryType.None)
            .OrderByDescending(a => a.Id)
            .FirstOrDefaultAsync();

        decimal computedRecovery = 0m;
        if (recoveryRule != null && recoveryRule.RecoveryValue.HasValue)
        {
            var totalLitres = collections.Sum(c => c.Litres);
            computedRecovery = recoveryRule.RecoveryType switch
            {
                RecoveryType.FixedPerLitre =>
                    Math.Round(totalLitres * recoveryRule.RecoveryValue.Value, 2, MidpointRounding.AwayFromZero),
                RecoveryType.PercentOfGross =>
                    Math.Round(grossMilkValue * recoveryRule.RecoveryValue.Value / 100m, 2, MidpointRounding.AwayFromZero),
                _ => 0m
            };
        }

        // Cap: min(farmerAdvanceBalance, min(computedRecovery, grossMilkValue))
        var advanceRecovered = Math.Min(farmer.AdvanceBalance,
                               Math.Min(computedRecovery, grossMilkValue));
        advanceRecovered = Math.Round(advanceRecovered, 2, MidpointRounding.AwayFromZero);

        var netPayable = Math.Round(grossMilkValue - advanceRecovered, 2, MidpointRounding.AwayFromZero);

        var settlement = new FarmerSettlement
        {
            FarmerId = farmerId,
            PeriodFrom = fromDate,
            PeriodTo = toDate,
            GrossMilkValue = grossMilkValue,
            AdvanceRecovered = advanceRecovered,
            NetPayable = netPayable,
            AmountPaid = 0m,
            OutstandingPayable = netPayable,
            Status = SettlementStatus.Draft
        };

        settlement.SettlementCollections = collections.Select(c => new SettlementCollection
        {
            CollectionId = c.Id
        }).ToList();

        return settlement;
    }

    public async Task PostSettlementAsync(int settlementId, string postedByUserId)
    {
        var isLocalTx = _db.Database.CurrentTransaction == null;
        var tx = isLocalTx ? await _db.Database.BeginTransactionAsync() : null;
        try
        {
            var settlement = await _db.FarmerSettlements
                .Include(s => s.SettlementCollections)
                .FirstOrDefaultAsync(s => s.Id == settlementId)
                ?? throw new InvalidOperationException("Settlement not found.");

            var farmer = await _db.Farmers.FindAsync(settlement.FarmerId)
                ?? throw new InvalidOperationException("Farmer not found.");

            // Create Advance record for SettlementDeduction
            if (settlement.AdvanceRecovered > 0)
            {
                var advance = new Advance
                {
                    FarmerId = settlement.FarmerId,
                    TransactionDate = DateOnly.FromDateTime(DateTime.Today),
                    Type = AdvanceType.SettlementDeduction,
                    Amount = settlement.AdvanceRecovered,
                    Notes = $"Settlement #{settlementId}",
                    RecoveryType = RecoveryType.None,
                    PostedByUserId = postedByUserId,
                    PostedAt = DateTimeOffset.UtcNow,
                    RelatedSettlementId = settlementId
                };
                _db.Advances.Add(advance);
                farmer.AdvanceBalance = Math.Round(farmer.AdvanceBalance - settlement.AdvanceRecovered, 2, MidpointRounding.AwayFromZero);
            }

            farmer.MilkPayable = Math.Round(farmer.MilkPayable + settlement.NetPayable, 2, MidpointRounding.AwayFromZero);
            farmer.UpdatedAt = DateTimeOffset.UtcNow;

            settlement.Status = SettlementStatus.Posted;
            settlement.PostedAt = DateTimeOffset.UtcNow;
            settlement.PostedByUserId = postedByUserId;

            await _db.SaveChangesAsync();
            if (tx != null) await tx.CommitAsync();
        }
        catch
        {
            if (tx != null) await tx.RollbackAsync();
            throw;
        }
        finally
        {
            if (tx != null) await tx.DisposeAsync();
        }
    }

    public async Task RecordPaymentAsync(int settlementId, decimal amount, PaymentMode mode, string userId, DateOnly date, string? notes)
    {
        var isLocalTx = _db.Database.CurrentTransaction == null;
        var tx = isLocalTx ? await _db.Database.BeginTransactionAsync() : null;
        try
        {
            var settlement = await _db.FarmerSettlements.FindAsync(settlementId)
                ?? throw new InvalidOperationException("Settlement not found.");

            var farmer = await _db.Farmers.FindAsync(settlement.FarmerId)
                ?? throw new InvalidOperationException("Farmer not found.");

            var payment = new SettlementPayment
            {
                SettlementId = settlementId,
                PaidDate = date,
                Amount = amount,
                PaymentMode = mode,
                Notes = notes,
                PostedByUserId = userId,
                PostedAt = DateTimeOffset.UtcNow
            };
            _db.SettlementPayments.Add(payment);

            settlement.AmountPaid = Math.Round(settlement.AmountPaid + amount, 2, MidpointRounding.AwayFromZero);
            settlement.OutstandingPayable = Math.Round(settlement.OutstandingPayable - amount, 2, MidpointRounding.AwayFromZero);
            if (settlement.OutstandingPayable <= 0)
            {
                settlement.OutstandingPayable = 0m;
                settlement.Status = SettlementStatus.FullyPaid;
            }
            else
            {
                settlement.Status = SettlementStatus.PartiallyPaid;
            }

            farmer.MilkPayable = Math.Round(farmer.MilkPayable - amount, 2, MidpointRounding.AwayFromZero);
            if (farmer.MilkPayable < 0) farmer.MilkPayable = 0m;
            farmer.UpdatedAt = DateTimeOffset.UtcNow;

            await _db.SaveChangesAsync();
            if (tx != null) await tx.CommitAsync();
        }
        catch
        {
            if (tx != null) await tx.RollbackAsync();
            throw;
        }
        finally
        {
            if (tx != null) await tx.DisposeAsync();
        }
    }
}
