using Ton.Core.Boc;

namespace Ton.Core.Types;

/// <summary>
///     Compute skip reason enum.
///     Source:
///     https://github.com/ton-blockchain/ton/blob/24dc184a2ea67f9c47042b4104bbb4d82289fac1/crypto/block/block.tlb#L306
///     cskip_no_state$00 = ComputeSkipReason;
///     cskip_bad_state$01 = ComputeSkipReason;
///     cskip_no_gas$10 = ComputeSkipReason;
/// </summary>
public enum ComputeSkipReason
{
    NoState = 0x00,
    BadState = 0x01,
    NoGas = 0x02
}

/// <summary>
///     Extension methods for ComputeSkipReason.
/// </summary>
public static class ComputeSkipReasonExtensions
{
    /// <summary>
    ///     Loads ComputeSkipReason from a Slice.
    /// </summary>
    public static ComputeSkipReason LoadComputeSkipReason(this Slice slice)
    {
        int reason = (int)slice.LoadUint(2);
        return reason switch
        {
            0x00 => ComputeSkipReason.NoState,
            0x01 => ComputeSkipReason.BadState,
            0x02 => ComputeSkipReason.NoGas,
            _ => throw new InvalidOperationException($"Unknown ComputeSkipReason: {reason}")
        };
    }

    /// <summary>
    ///     Stores ComputeSkipReason into a Builder.
    /// </summary>
    public static Builder StoreComputeSkipReason(this Builder builder, ComputeSkipReason reason)
    {
        builder.StoreUint((ulong)reason, 2);
        return builder;
    }
}