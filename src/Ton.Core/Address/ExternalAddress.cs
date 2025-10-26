using System.Numerics;

namespace Ton.Core.Addresses;

/// <summary>
///     Represents an external address in TON blockchain.
///     External addresses are used for messages from/to external systems.
/// </summary>
public record ExternalAddress
{
    /// <summary>
    ///     Creates a new ExternalAddress.
    /// </summary>
    public ExternalAddress(BigInteger value, int bits)
    {
        Value = value;
        Bits = bits;
    }

    /// <summary>
    ///     The address value.
    /// </summary>
    public BigInteger Value { get; init; }

    /// <summary>
    ///     Number of bits in the address.
    /// </summary>
    public int Bits { get; init; }

    /// <summary>
    ///     Returns a string representation of the external address.
    /// </summary>
    public override string ToString()
    {
        return $"External<{Bits}:{Value}>";
    }
}