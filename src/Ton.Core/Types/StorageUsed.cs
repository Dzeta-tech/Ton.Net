using System.Numerics;
using Ton.Core.Boc;

namespace Ton.Core.Types;

/// <summary>
///     Storage usage statistics.
///     Source:
///     https://github.com/ton-blockchain/ton/blob/3fbab2c601380eba5ba68048f45d24a359bd2936/crypto/block/block.tlb#L253
///     storage_used$_ cells:(VarUInteger 7) bits:(VarUInteger 7) = StorageUsed;
/// </summary>
public record StorageUsed(BigInteger Cells, BigInteger Bits)
{
    /// <summary>
    ///     Loads StorageUsed from a Slice.
    /// </summary>
    public static StorageUsed Load(Slice slice)
    {
        return new StorageUsed(
            slice.LoadVarUintBig(3),
            slice.LoadVarUintBig(3)
        );
    }

    /// <summary>
    ///     Stores StorageUsed into a Builder.
    /// </summary>
    public void Store(Builder builder)
    {
        builder.StoreVarUint(Cells, 3);
        builder.StoreVarUint(Bits, 3);
    }
}