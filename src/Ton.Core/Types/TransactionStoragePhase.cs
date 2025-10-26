using System.Numerics;
using Ton.Core.Boc;

namespace Ton.Core.Types;

/// <summary>
///     Transaction storage phase.
///     Source:
///     https://github.com/ton-blockchain/ton/blob/24dc184a2ea67f9c47042b4104bbb4d82289fac1/crypto/block/block.tlb#L284
///     tr_phase_storage$_ storage_fees_collected:Grams
///     storage_fees_due:(Maybe Grams)
///     status_change:AccStatusChange
///     = TrStoragePhase;
/// </summary>
public record TransactionStoragePhase(
    BigInteger StorageFeesCollected,
    BigInteger? StorageFeesDue,
    AccountStatusChange StatusChange
)
{
    /// <summary>
    ///     Loads TransactionStoragePhase from a Slice.
    /// </summary>
    public static TransactionStoragePhase Load(Slice slice)
    {
        BigInteger storageFeesCollected = slice.LoadCoins();
        BigInteger? storageFeesDue = slice.LoadBit() ? slice.LoadCoins() : null;
        AccountStatusChange statusChange = slice.LoadAccountStatusChange();

        return new TransactionStoragePhase(storageFeesCollected, storageFeesDue, statusChange);
    }

    /// <summary>
    ///     Stores TransactionStoragePhase into a Builder.
    /// </summary>
    public void Store(Builder builder)
    {
        builder.StoreCoins(StorageFeesCollected);

        if (StorageFeesDue == null)
        {
            builder.StoreBit(false);
        }
        else
        {
            builder.StoreBit(true);
            builder.StoreCoins(StorageFeesDue.Value);
        }

        builder.StoreAccountStatusChange(StatusChange);
    }
}