using Ton.Core.Boc;

namespace Ton.Core.Types;

/// <summary>
///     Future split/merge schedule for a shard.
///     Source:
///     https://github.com/ton-blockchain/ton/blob/24dc184a2ea67f9c47042b4104bbb4d82289fac1/crypto/block/block.tlb#L515
///     fsm_none$0 = FutureSplitMerge;
///     fsm_split$10 split_utime:uint32 interval:uint32 = FutureSplitMerge;
///     fsm_merge$11 merge_utime:uint32 interval:uint32 = FutureSplitMerge;
/// </summary>
public sealed record FutureSplitMerge
{
    /// <summary>
    ///     Kind of scheduled action.
    /// </summary>
    public FutureSplitMergeKind Kind { get; init; }

    /// <summary>
    ///     Scheduled unix timestamp for split/merge operation (if any).
    /// </summary>
    public uint? Time { get; init; }

    /// <summary>
    ///     Time interval in seconds (if any).
    /// </summary>
    public uint? Interval { get; init; }

    private FutureSplitMerge(FutureSplitMergeKind kind, uint? time, uint? interval)
    {
        Kind = kind;
        Time = time;
        Interval = interval;
    }

    /// <summary>
    ///     Creates a value indicating no scheduled split/merge.
    /// </summary>
    public static FutureSplitMerge None { get; } = new(FutureSplitMergeKind.None, null, null);

    /// <summary>
    ///     Loads FutureSplitMerge from a <see cref="Slice" />.
    /// </summary>
    public static FutureSplitMerge Load(Slice slice)
    {
        // Encoding:
        //  - fsm_none:  0
        //  - fsm_split: 10
        //  - fsm_merge: 11
        bool first = slice.LoadBit();
        if (!first)
            return None;

        bool second = slice.LoadBit();

        uint time = (uint)slice.LoadUint(32);
        uint interval = (uint)slice.LoadUint(32);

        return second
            ? new FutureSplitMerge(FutureSplitMergeKind.Merge, time, interval)
            : new FutureSplitMerge(FutureSplitMergeKind.Split, time, interval);
    }

    /// <summary>
    ///     Stores FutureSplitMerge into a <see cref="Builder" />.
    /// </summary>
    public void Store(Builder builder)
    {
        switch (Kind)
        {
            case FutureSplitMergeKind.None:
                builder.StoreBit(false);
                break;
            case FutureSplitMergeKind.Split:
                builder.StoreBit(true);
                builder.StoreBit(false);
                builder.StoreUint(Time.GetValueOrDefault(), 32);
                builder.StoreUint(Interval.GetValueOrDefault(), 32);
                break;
            case FutureSplitMergeKind.Merge:
                builder.StoreBit(true);
                builder.StoreBit(true);
                builder.StoreUint(Time.GetValueOrDefault(), 32);
                builder.StoreUint(Interval.GetValueOrDefault(), 32);
                break;
            default:
                throw new InvalidOperationException($"Unknown {nameof(FutureSplitMergeKind)}: {Kind}");
        }
    }
}

/// <summary>
///     Kind of future split/merge action.
/// </summary>
public enum FutureSplitMergeKind : byte
{
    None = 0,
    Split = 1,
    Merge = 2
}


