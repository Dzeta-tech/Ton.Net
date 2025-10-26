using System.Numerics;
using Ton.Core.Boc;

namespace Ton.Core.Types;

/// <summary>
///     Shard identifier.
///     Source:
///     https://github.com/ton-blockchain/ton/blob/24dc184a2ea67f9c47042b4104bbb4d82289fac1/crypto/block/block.tlb#L384
///     shard_ident$00 shard_pfx_bits:(#&lt;= 60)
///     workchain_id:int32 shard_prefix:uint64 = ShardIdent;
/// </summary>
public record ShardIdent(
    byte ShardPrefixBits,
    int WorkchainId,
    BigInteger ShardPrefix
)
{
    /// <summary>
    ///     Loads ShardIdent from a Slice.
    /// </summary>
    public static ShardIdent Load(Slice slice)
    {
        if (slice.LoadUint(2) != 0)
            throw new InvalidOperationException("Invalid ShardIdent prefix");

        return new ShardIdent(
            (byte)slice.LoadUint(6),
            (int)slice.LoadInt(32),
            slice.LoadUintBig(64)
        );
    }

    /// <summary>
    ///     Stores ShardIdent into a Builder.
    /// </summary>
    public void Store(Builder builder)
    {
        builder.StoreUint(0, 2);
        builder.StoreUint(ShardPrefixBits, 6);
        builder.StoreInt(WorkchainId, 32);
        builder.StoreUint(ShardPrefix, 64);
    }
}