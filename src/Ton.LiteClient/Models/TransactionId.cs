using System.Numerics;

namespace Ton.LiteClient.Models;

/// <summary>
/// Represents a transaction identifier in TON blockchain
/// </summary>
public readonly record struct TransactionId
{
    /// <summary>
    /// Logical time of the transaction
    /// </summary>
    public BigInteger Lt { get; init; }

    /// <summary>
    /// Hash of the transaction
    /// </summary>
    public byte[] Hash { get; init; }

    public TransactionId(BigInteger lt, byte[] hash)
    {
        ArgumentNullException.ThrowIfNull(hash);

        if (hash.Length != 32)
            throw new ArgumentException("Transaction hash must be 32 bytes", nameof(hash));

        Lt = lt;
        Hash = hash;
    }

    /// <summary>
    /// Returns the hash as a hex string
    /// </summary>
    public string HashHex => Convert.ToHexString(Hash);

    public override string ToString() => $"Tx(lt:{Lt}, hash:{HashHex[..16]}...)";
}

