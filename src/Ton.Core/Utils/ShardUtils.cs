using System.Numerics;
using Ton.Core.Types;

namespace Ton.Core.Utils;

/// <summary>
///     Utility functions for shard calculations.
/// </summary>
public static class ShardUtils
{
    /// <summary>
    ///     Converts ShardIdent to workchain and shard ID.
    ///     Based on Go implementation: ConvertShardIdentToShard
    /// </summary>
    public static (int workchain, ulong shard) ConvertShardIdentToShard(ShardIdent shardIdent)
    {
        ulong shard = (ulong)shardIdent.ShardPrefix;
        ulong pow2 = 1UL << (63 - shardIdent.ShardPrefixBits);
        shard |= pow2;
        return (shardIdent.WorkchainId, shard);
    }

    /// <summary>
    ///     Gets the parent shard ID.
    ///     Based on Go implementation: ShardParent
    /// </summary>
    public static ulong ShardParent(ulong shard)
    {
        ulong x = LowerBit64(shard);
        return (shard - x) | (x << 1);
    }

    /// <summary>
    ///     Gets a child shard ID.
    ///     Based on Go implementation: ShardChild
    /// </summary>
    /// <param name="shard">Parent shard ID.</param>
    /// <param name="left">True for left child, false for right child.</param>
    /// <returns>Child shard ID.</returns>
    public static ulong ShardChild(ulong shard, bool left)
    {
        ulong x = LowerBit64(shard) >> 1;
        if (left)
            return shard - x;
        return shard + x;
    }

    /// <summary>
    ///     Gets the lowest set bit in a 64-bit value.
    ///     Based on Go implementation: lowerBit64
    /// </summary>
    private static ulong LowerBit64(ulong x)
    {
        return x & BitsNegate64(x);
    }

    /// <summary>
    ///     Two's complement negation helper.
    ///     Based on Go implementation: bitsNegate64
    /// </summary>
    private static ulong BitsNegate64(ulong x)
    {
        return ~x + 1;
    }
}

