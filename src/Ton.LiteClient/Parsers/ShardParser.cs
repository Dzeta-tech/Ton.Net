using Ton.Core.Boc;
using Ton.LiteClient.Models;

namespace Ton.LiteClient.Parsers;

/// <summary>
/// Parser for TON shard information from BOC data
/// </summary>
public static class ShardParser
{
    /// <summary>
    /// Parses shard information from BOC-encoded data
    /// </summary>
    /// <param name="data">BOC-encoded shard data</param>
    /// <returns>Collection of shard descriptors</returns>
    public static ShardCollection ParseShards(byte[] data)
    {
        ArgumentNullException.ThrowIfNull(data);

        var cells = Cell.FromBoc(data);
        if (cells.Length == 0)
            return new ShardCollection();

        var shards = new List<ShardDescriptor>();

        foreach (var cell in cells)
        {
            try
            {
                // Parse as hashmap with workchain keys
                var slice = cell.BeginParse();
                
                // Load dictionary (simplified - full implementation would use proper dict parsing)
                // For now, try to load binary tree directly
                if (slice.LoadBit()) // dict non-empty
                {
                    var dictRef = slice.LoadRef();
                    ParseBinaryTree(dictRef.BeginParse(), ref shards);
                }
            }
            catch
            {
                // If parsing fails, continue with next cell
                continue;
            }
        }

        return new ShardCollection(shards);
    }

    /// <summary>
    /// Recursively parses binary tree of shard descriptors
    /// </summary>
    static void ParseBinaryTree(Slice slice, ref List<ShardDescriptor> shards)
    {
        if (!slice.LoadBit()) // leaf node
        {
            // Parse shard descriptor
            var descriptor = ParseShardDescriptor(slice);
            if (descriptor.HasValue)
                shards.Add(descriptor.Value);
        }
        else // branch node
        {
            // Load left and right branches
            if (slice.RemainingRefs >= 2)
            {
                var left = slice.LoadRef();
                var right = slice.LoadRef();
                
                ParseBinaryTree(left.BeginParse(), ref shards);
                ParseBinaryTree(right.BeginParse(), ref shards);
            }
        }
    }

    /// <summary>
    /// Parses a single shard descriptor from a cell slice
    /// </summary>
    static ShardDescriptor? ParseShardDescriptor(Slice slice)
    {
        try
        {
            uint type = (uint)slice.LoadUint(4);
            
            // Type should be 0xa or 0xb for valid ShardDescr
            if (type != 0xa && type != 0xb)
                return null;
            
            int seqno = (int)slice.LoadUint(32);
            slice.LoadUint(32); // reg_mc_seqno
            slice.LoadUint(64); // start_lt
            slice.LoadUint(64); // end_lt
            
            // LoadBits returns BitString, we need to convert to bytes
            var rootHashBits = slice.LoadBits(256);
            var fileHashBits = slice.LoadBits(256);
            byte[] rootHash = rootHashBits.Subbuffer(0, 256) ?? throw new InvalidOperationException("Failed to get root hash buffer");
            byte[] fileHash = fileHashBits.Subbuffer(0, 256) ?? throw new InvalidOperationException("Failed to get file hash buffer");
            
            // Skip flags
            slice.LoadBit(); // before_split
            slice.LoadBit(); // before_merge
            slice.LoadBit(); // want_split
            slice.LoadBit(); // want_merge
            slice.LoadBit(); // nx_cc_updated
            slice.LoadUint(3); // flags
            
            slice.LoadUint(32); // next_catchain_seqno
            long shard = slice.LoadInt(64);
            
            // Workchain is typically 0 for basechain shards
            int workchain = 0;

            return new ShardDescriptor(workchain, shard, seqno);
        }
        catch
        {
            return null;
        }
    }
}

