using System.Numerics;
using Ton.Core.Boc;

namespace Ton.Core.Types;

/// <summary>
///     Split/merge information for sharding.
///     Source:
///     https://github.com/ton-blockchain/ton/blob/24dc184a2ea67f9c47042b4104bbb4d82289fac1/crypto/block/block.tlb#L339
///     split_merge_info$_ cur_shard_pfx_len:(## 6)
///     acc_split_depth:(## 6) this_addr:bits256 sibling_addr:bits256
///     = SplitMergeInfo;
/// </summary>
public record SplitMergeInfo(
    byte CurrentShardPrefixLength,
    byte AccountSplitDepth,
    BigInteger ThisAddress,
    BigInteger SiblingAddress
)
{
    /// <summary>
    ///     Loads SplitMergeInfo from a Slice.
    /// </summary>
    public static SplitMergeInfo Load(Slice slice)
    {
        byte currentShardPrefixLength = (byte)slice.LoadUint(6);
        byte accountSplitDepth = (byte)slice.LoadUint(6);
        BigInteger thisAddress = slice.LoadUintBig(256);
        BigInteger siblingAddress = slice.LoadUintBig(256);

        return new SplitMergeInfo(
            currentShardPrefixLength,
            accountSplitDepth,
            thisAddress,
            siblingAddress
        );
    }

    /// <summary>
    ///     Stores SplitMergeInfo into a Builder.
    /// </summary>
    public void Store(Builder builder)
    {
        builder.StoreUint(CurrentShardPrefixLength, 6);
        builder.StoreUint(AccountSplitDepth, 6);
        builder.StoreUint(ThisAddress, 256);
        builder.StoreUint(SiblingAddress, 256);
    }
}