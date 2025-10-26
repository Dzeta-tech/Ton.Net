using System.Numerics;
using Ton.Core.Boc;

namespace Ton.Core.Types;

/// <summary>
///     Extra storage information.
///     Source:
///     https://github.com/ton-blockchain/ton/blob/3fbab2c601380eba5ba68048f45d24a359bd2936/crypto/block/block.tlb#L250
///     storage_extra_none$000 = StorageExtraInfo;
///     storage_extra_info$001 dict_hash:uint256 = StorageExtraInfo;
/// </summary>
public record StorageExtraInfo(BigInteger DictHash)
{
    /// <summary>
    ///     Loads StorageExtraInfo from a Slice.
    /// </summary>
    public static StorageExtraInfo? Load(Slice slice)
    {
        int header = (int)slice.LoadUint(3);

        return header switch
        {
            0 => null,
            1 => new StorageExtraInfo(slice.LoadUintBig(256)),
            _ => throw new InvalidOperationException($"Invalid storage extra info header: {header}")
        };
    }

    /// <summary>
    ///     Stores StorageExtraInfo into a Builder.
    /// </summary>
    public static void Store(Builder builder, StorageExtraInfo? info)
    {
        if (info == null)
        {
            builder.StoreUint(0, 3);
        }
        else
        {
            builder.StoreUint(1, 3);
            builder.StoreUint(info.DictHash, 256);
        }
    }
}