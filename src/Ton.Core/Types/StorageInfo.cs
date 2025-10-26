using System.Numerics;
using Ton.Core.Boc;

namespace Ton.Core.Types;

/// <summary>
///     Storage information for an account.
///     Source:
///     https://github.com/ton-blockchain/ton/blob/3fbab2c601380eba5ba68048f45d24a359bd2936/crypto/block/block.tlb#L255
///     storage_info$_ used:StorageUsed storage_extra:StorageExtraInfo last_paid:uint32
///     due_payment:(Maybe Grams) = StorageInfo;
/// </summary>
public record StorageInfo(
    StorageUsed Used,
    StorageExtraInfo? StorageExtra,
    uint LastPaid,
    BigInteger? DuePayment = null)
{
    /// <summary>
    ///     Loads StorageInfo from a Slice.
    /// </summary>
    public static StorageInfo Load(Slice slice)
    {
        return new StorageInfo(
            StorageUsed.Load(slice),
            StorageExtraInfo.Load(slice),
            (uint)slice.LoadUint(32),
            slice.LoadMaybeCoins()
        );
    }

    /// <summary>
    ///     Stores StorageInfo into a Builder.
    /// </summary>
    public void Store(Builder builder)
    {
        Used.Store(builder);
        StorageExtraInfo.Store(builder, StorageExtra);
        builder.StoreUint(LastPaid, 32);
        builder.StoreMaybeCoins(DuePayment);
    }
}