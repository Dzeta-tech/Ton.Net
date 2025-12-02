using Ton.Core.Boc;

namespace Ton.Core.Types;

/// <summary>
///     Block previous info (single or merged).
///     Source:
///     https://github.com/ton-blockchain/ton/blob/24dc184a2ea67f9c47042b4104bbb4d82289fac1/crypto/block/block.tlb#L469
///     prev_blk_info$_ prev:ExtBlkRef = BlkPrevInfo 0;
///     prev_blks_info$_ prev1:^ExtBlkRef prev2:^ExtBlkRef = BlkPrevInfo 1;
/// </summary>
public record BlkPrevInfo
{
    /// <summary>
    ///     Creates a new BlkPrevInfo with a single previous block.
    /// </summary>
    public BlkPrevInfo(ExtBlkRef prev1)
    {
        Prev1 = prev1;
        Prev2 = null;
    }

    /// <summary>
    ///     Creates a new BlkPrevInfo with two previous blocks (after merge).
    /// </summary>
    public BlkPrevInfo(ExtBlkRef prev1, ExtBlkRef prev2)
    {
        Prev1 = prev1;
        Prev2 = prev2;
    }

    /// <summary>
    ///     First previous block reference.
    /// </summary>
    public ExtBlkRef Prev1 { get; init; }

    /// <summary>
    ///     Second previous block reference (null if not merged).
    /// </summary>
    public ExtBlkRef? Prev2 { get; init; }

    /// <summary>
    ///     Returns true if this represents a merge (has two previous blocks).
    /// </summary>
    public bool IsMerge => Prev2 != null;

    /// <summary>
    ///     Loads BlkPrevInfo from a Slice.
    /// </summary>
    /// <param name="slice">The slice to load from.</param>
    /// <param name="afterMerge">Whether this is after a merge (determines structure).</param>
    /// <returns>A new BlkPrevInfo instance.</returns>
    public static BlkPrevInfo Load(Slice slice, bool afterMerge)
    {
        if (!afterMerge)
        {
            // Single previous block
            ExtBlkRef prev = ExtBlkRef.Load(slice);
            return new BlkPrevInfo(prev);
        }

        // Two previous blocks (after merge)
        Cell prev1Cell = slice.LoadRef();
        Cell prev2Cell = slice.LoadRef();

        ExtBlkRef prev1 = ExtBlkRef.Load(prev1Cell.BeginParse());
        ExtBlkRef prev2 = ExtBlkRef.Load(prev2Cell.BeginParse());

        return new BlkPrevInfo(prev1, prev2);
    }

    /// <summary>
    ///     Stores BlkPrevInfo into a Builder.
    /// </summary>
    public void Store(Builder builder)
    {
        if (Prev2 == null)
        {
            // Single previous block
            Prev1.Store(builder);
        }
        else
        {
            // Two previous blocks
            Builder prev1Builder = new();
            Prev1.Store(prev1Builder);
            builder.StoreRef(prev1Builder.EndCell());

            Builder prev2Builder = new();
            Prev2.Store(prev2Builder);
            builder.StoreRef(prev2Builder.EndCell());
        }
    }
}

