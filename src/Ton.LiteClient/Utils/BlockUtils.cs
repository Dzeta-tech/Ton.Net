using Ton.Core.Types;
using Ton.Core.Utils;
using Ton.LiteClient.Models;

namespace Ton.LiteClient.Utils;

/// <summary>
///     Utility functions for block operations.
/// </summary>
public static class BlockUtils
{
    /// <summary>
    ///     Gets parent block IDs from a BlockInfo.
    ///     Based on Go implementation: GetParentBlocks
    /// </summary>
    /// <param name="blockInfo">Block information to extract parents from.</param>
    /// <returns>Array of parent block IDs.</returns>
    public static BlockId[] GetParentBlocks(BlockInfo blockInfo)
    {
        (int workchain, ulong shard) = ShardUtils.ConvertShardIdentToShard(blockInfo.Shard);
        long shardLong = unchecked((long)shard);

        if (!blockInfo.AfterMerge && !blockInfo.AfterSplit)
        {
            // Single parent block
            return new[]
            {
                new BlockId(
                    workchain,
                    shardLong,
                    blockInfo.PrevRef.Prev1.SeqNo,
                    blockInfo.PrevRef.Prev1.RootHash,
                    blockInfo.PrevRef.Prev1.FileHash)
            };
        }

        if (!blockInfo.AfterMerge && blockInfo.AfterSplit)
        {
            // After split - parent shard is the parent of current shard
            ulong parentShard = ShardUtils.ShardParent(shard);
            return new[]
            {
                new BlockId(
                    workchain,
                    unchecked((long)parentShard),
                    blockInfo.PrevRef.Prev1.SeqNo,
                    blockInfo.PrevRef.Prev1.RootHash,
                    blockInfo.PrevRef.Prev1.FileHash)
            };
        }

        // After merge - two parent blocks
        if (blockInfo.PrevRef.Prev2 == null)
            throw new InvalidOperationException("Must be 2 parent blocks after merge");

        ulong leftChild = ShardUtils.ShardChild(shard, left: true);
        ulong rightChild = ShardUtils.ShardChild(shard, left: false);

        return new[]
        {
            new BlockId(
                workchain,
                unchecked((long)leftChild),
                blockInfo.PrevRef.Prev1.SeqNo,
                blockInfo.PrevRef.Prev1.RootHash,
                blockInfo.PrevRef.Prev1.FileHash),
            new BlockId(
                workchain,
                unchecked((long)rightChild),
                blockInfo.PrevRef.Prev2.SeqNo,
                blockInfo.PrevRef.Prev2.RootHash,
                blockInfo.PrevRef.Prev2.FileHash)
        };
    }
}

