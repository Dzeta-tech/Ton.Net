using System.Numerics;
using Ton.Core.Boc;
using Ton.Core.Dict;
using Ton.Core.Types;
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
    /// <returns>Array of shard descriptions</returns>
    public static ShardDescr[] ParseShards(byte[] data)
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

        List<ShardDescr> shards = [];

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
                ShardDescr descr = LoadShardDescr(treeSlice, workchain, 1L << 63);
                shards.Add(descr);
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
    ///     Parses binary tree of shard descriptors iteratively using a stack.
    ///     Correctly computes shard identifiers from the binary tree position.
    /// </summary>
    static void ParseShardTreeIterative(Cell rootCell, int workchain, ref List<ShardDescr> shards)
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
                ShardDescr descr = LoadShardDescr(slice, workchain, shard);
                shards.Add(descr);
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

    /// <summary>
    ///     Loads a single ShardDescr from a leaf slice.
    /// </summary>
    static ShardDescr LoadShardDescr(Slice slice, int workchain, long shard)
    {
        uint magic = (uint)slice.LoadUint(4);
        if (magic != 0xa && magic != 0xb)
            throw new InvalidOperationException($"Not a ShardDescr (magic: 0x{magic:X})");

        uint seqno = (uint)slice.LoadUint(32);
        uint regMcSeqno = (uint)slice.LoadUint(32);
        BigInteger startLt = slice.LoadUintBig(64);
        BigInteger endLt = slice.LoadUintBig(64);

        byte[] rootHash = slice.LoadBits(256).ToBytes();
        byte[] fileHash = slice.LoadBits(256).ToBytes();

        bool beforeSplit = slice.LoadBit();
        bool beforeMerge = slice.LoadBit();
        bool wantSplit = slice.LoadBit();
        bool wantMerge = slice.LoadBit();
        bool nxCcUpdated = slice.LoadBit();
        byte flags = (byte)slice.LoadUint(3);
        uint nextCatchainSeqno = (uint)slice.LoadUint(32);
        BigInteger nextValidatorShard = slice.LoadUintBig(64);
        uint minRefMcSeqno = (uint)slice.LoadUint(32);
        uint genUtime = (uint)slice.LoadUint(32);

        FutureSplitMerge splitMergeAt = FutureSplitMerge.Load(slice);

        CurrencyCollection feesCollected;
        CurrencyCollection fundsCreated;

        if (magic == 0xb)
        {
            // Old layout: currencies inline
            feesCollected = CurrencyCollection.Load(slice);
            fundsCreated = CurrencyCollection.Load(slice);
        }
        else
        {
            // New layout: currencies are stored in a reference cell
            Cell refCell = slice.LoadRef();
            Slice refSlice = refCell.BeginParse();
            feesCollected = CurrencyCollection.Load(refSlice);
            fundsCreated = CurrencyCollection.Load(refSlice);
        }

        return new ShardDescr(
            workchain,
            shard,
            seqno,
            rootHash,
            fileHash,
            regMcSeqno,
            startLt,
            endLt,
            beforeSplit,
            beforeMerge,
            wantSplit,
            wantMerge,
            nxCcUpdated,
            flags,
            nextCatchainSeqno,
            nextValidatorShard,
            minRefMcSeqno,
            genUtime,
            splitMergeAt,
            feesCollected,
            fundsCreated
        );
    }
}