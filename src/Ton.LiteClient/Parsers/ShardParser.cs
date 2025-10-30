using Ton.Core.Boc;
using Ton.Core.Dict;
using Ton.LiteClient.Models;

namespace Ton.LiteClient.Parsers;

/// <summary>
///     Parser for TON shard information from BOC data
/// </summary>
public static class ShardParser
{
    /// <summary>
    ///     Parses shard information from BOC-encoded data
    /// </summary>
    /// <param name="data">BOC-encoded shard data</param>
    /// <returns>Array of shard block IDs</returns>
    public static BlockId[] ParseShards(byte[] data)
    {
        ArgumentNullException.ThrowIfNull(data);

        Cell[] cells = Cell.FromBoc(data);
        if (cells.Length == 0)
            return [];

        Cell root = cells[0];
        Slice slice = root.BeginParse();

        // Check if dictionary is non-empty
        if (!slice.LoadBit())
            return [];

        // Load the dictionary reference and parse it
        Cell dictCell = slice.LoadRef();
        Slice dictSlice = dictCell.BeginParse();

        // Parse dictionary with 32-bit keys (workchain IDs)
        Core.Dict.Dictionary<DictKeyInt, Cell>? dict = dictSlice.LoadDictDirect(
            DictionaryKeys.Int(32),
            DictionaryValues.Cell());

        if (dict == null || dict.Count() == 0)
            return [];

        List<BlockId> shards = [];

        // Parse each workchain's shard tree
        foreach (KeyValuePair<DictKeyInt, Cell> kvp in dict)
        {
            int workchain = kvp.Key.Value;
            Cell shardTreeCell = kvp.Value;

            Slice treeSlice = shardTreeCell.BeginParse();
            bool isBranch = treeSlice.LoadBit();

            if (!isBranch)
            {
                // bt_leaf - single shard for this workchain (contains full ShardDescr)
                uint type = (uint)treeSlice.LoadUint(4);

                if (type == 0xa || type == 0xb)
                {
                    // Read full ShardDescr from the leaf
                    uint seqno = (uint)treeSlice.LoadUint(32);
                    treeSlice.Skip(32); // reg_mc_seqno
                    treeSlice.Skip(64); // start_lt
                    treeSlice.Skip(64); // end_lt

                    byte[] rootHash = treeSlice.LoadBits(256).ToBytes();
                    byte[] fileHash = treeSlice.LoadBits(256).ToBytes();

                    treeSlice.Skip(5); // flags
                    treeSlice.Skip(3);
                    treeSlice.Skip(32);
                    long shardId = treeSlice.LoadInt(64);

                    shards.Add(new BlockId(workchain, shardId, seqno, rootHash, fileHash));
                }
            }
            else
            {
                // bt_fork - multiple shards for this workchain (binary tree in references)
                // Parse the binary tree of ShardDescr nodes
                ParseShardTreeIterative(shardTreeCell, workchain, ref shards);
            }
        }

        return shards.ToArray();
    }

    /// <summary>
    ///     Parses binary tree of shard descriptors iteratively using a stack
    /// </summary>
    static void ParseShardTreeIterative(Cell rootCell, int workchain, ref List<BlockId> shards)
    {
        // Stack of (slice, shard ID) pairs to process
        Stack<(Slice slice, long shard)> stack = new();
        stack.Push((rootCell.BeginParse(), 1L << 63));

        while (stack.Count > 0)
        {
            (Slice slice, long shard) = stack.Pop();

            bool isBranch = slice.LoadBit();

            if (!isBranch) // Leaf node - contains full ShardDescr
            {
                uint type = (uint)slice.LoadUint(4);

                if (type == 0xa || type == 0xb)
                {
                    // Read full ShardDescr
                    uint seqno = (uint)slice.LoadUint(32);
                    slice.Skip(32); // reg_mc_seqno
                    slice.Skip(64); // start_lt
                    slice.Skip(64); // end_lt

                    byte[] rootHash = slice.LoadBits(256).ToBytes();
                    byte[] fileHash = slice.LoadBits(256).ToBytes();

                    slice.Skip(5); // flags
                    slice.Skip(3);
                    slice.Skip(32);
                    long shardId = slice.LoadInt(64);

                    shards.Add(new BlockId(workchain, shardId, seqno, rootHash, fileHash));
                }

                continue;
            }

            // Branch node - split into left and right
            // Calculate delta: (shard & (~shard + 1)) >> 1
            long delta = (shard & (~shard + 1)) >> 1;

            // Branch must have exactly 2 refs and no remaining bits
            if (delta == 0 || slice.RemainingRefs != 2 || slice.RemainingBits > 0)
                continue;

            // Push left branch (shard - delta)
            Cell left = slice.LoadRef();
            stack.Push((left.BeginParse(), shard - delta));

            // Push right branch (shard + delta)
            Cell right = slice.LoadRef();
            stack.Push((right.BeginParse(), shard + delta));
        }
    }
}