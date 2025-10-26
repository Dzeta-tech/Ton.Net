using System.Numerics;
using Ton.Core.Boc;

namespace Ton.Core.Types;

/// <summary>
///     Transaction credit phase.
///     Source:
///     https://github.com/ton-blockchain/ton/blob/24dc184a2ea67f9c47042b4104bbb4d82289fac1/crypto/block/block.tlb#L293
///     tr_phase_credit$_ due_fees_collected:(Maybe Grams)
///     credit:CurrencyCollection = TrCreditPhase;
/// </summary>
public record TransactionCreditPhase(
    BigInteger? DueFeesCollected,
    CurrencyCollection Credit
)
{
    /// <summary>
    ///     Loads TransactionCreditPhase from a Slice.
    /// </summary>
    public static TransactionCreditPhase Load(Slice slice)
    {
        BigInteger? dueFeesCollected = slice.LoadBit() ? slice.LoadCoins() : null;
        CurrencyCollection credit = CurrencyCollection.Load(slice);

        return new TransactionCreditPhase(dueFeesCollected, credit);
    }

    /// <summary>
    ///     Stores TransactionCreditPhase into a Builder.
    /// </summary>
    public void Store(Builder builder)
    {
        if (DueFeesCollected == null)
        {
            builder.StoreBit(false);
        }
        else
        {
            builder.StoreBit(true);
            builder.StoreCoins(DueFeesCollected.Value);
        }

        Credit.Store(builder);
    }
}