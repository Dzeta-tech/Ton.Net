using Ton.Core.Boc;

namespace Ton.Core.Types;

/// <summary>
///     Special contract tick-tock flag.
///     Source:
///     https://github.com/ton-blockchain/ton/blob/24dc184a2ea67f9c47042b4104bbb4d82289fac1/crypto/block/block.tlb#L139
///     TL-B: tick_tock$_ tick:Bool tock:Bool = TickTock;
/// </summary>
public record TickTock
{
    /// <summary>
    ///     Creates a new TickTock instance.
    /// </summary>
    public TickTock(bool tick, bool tock)
    {
        Tick = tick;
        Tock = tock;
    }

    /// <summary>
    ///     Tick flag - contract is invoked on tick.
    /// </summary>
    public bool Tick { get; init; }

    /// <summary>
    ///     Tock flag - contract is invoked on tock.
    /// </summary>
    public bool Tock { get; init; }

    /// <summary>
    ///     Loads TickTock from a slice.
    /// </summary>
    /// <param name="slice">Slice to load from.</param>
    /// <returns>Loaded TickTock.</returns>
    public static TickTock Load(Slice slice)
    {
        bool tick = slice.LoadBit();
        bool tock = slice.LoadBit();
        return new TickTock(tick, tock);
    }

    /// <summary>
    ///     Stores TickTock to a builder.
    /// </summary>
    /// <param name="builder">Builder to store to.</param>
    public void Store(Builder builder)
    {
        builder.StoreBit(Tick);
        builder.StoreBit(Tock);
    }
}