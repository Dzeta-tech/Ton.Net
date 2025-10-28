using Ton.Crypto.Primitives;

namespace Ton.Adnl.Crypto;

/// <summary>
///     ADNL address - SHA-256 hash of a public key.
///     Used to identify ADNL peers in the network.
/// </summary>
public sealed class AdnlAddress : IEquatable<AdnlAddress>
{
    readonly byte[] hash;

    /// <summary>
    ///     Creates an ADNL address from a public key.
    /// </summary>
    /// <param name="publicKey">Ed25519 public key (32 bytes).</param>
    public AdnlAddress(byte[] publicKey)
    {
        ArgumentNullException.ThrowIfNull(publicKey);
        if (publicKey.Length != 32)
            throw new ArgumentException("Public key must be 32 bytes", nameof(publicKey));

        hash = Sha256.Hash(publicKey);
    }

    /// <summary>
    ///     Creates an ADNL address from a base64-encoded public key.
    /// </summary>
    /// <param name="publicKeyBase64">Base64-encoded Ed25519 public key.</param>
    public AdnlAddress(string publicKeyBase64)
    {
        if (string.IsNullOrEmpty(publicKeyBase64))
            throw new ArgumentException("Public key cannot be null or empty", nameof(publicKeyBase64));

        byte[] publicKey = Convert.FromBase64String(publicKeyBase64);
        if (publicKey.Length != 32)
            throw new ArgumentException("Public key must be 32 bytes", nameof(publicKeyBase64));

        hash = Sha256.Hash(publicKey);
    }

    /// <summary>
    ///     Gets the address hash (32 bytes).
    /// </summary>
    public byte[] Hash => (byte[])hash.Clone();

    public bool Equals(AdnlAddress? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return hash.AsSpan().SequenceEqual(other.hash);
    }

    /// <summary>
    ///     Gets the address as a hex string.
    /// </summary>
    public string ToHex()
    {
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    /// <summary>
    ///     Gets the address as a base64 string.
    /// </summary>
    public string ToBase64()
    {
        return Convert.ToBase64String(hash);
    }

    public override bool Equals(object? obj)
    {
        return obj is AdnlAddress other && Equals(other);
    }

    public override int GetHashCode()
    {
        // Use first 4 bytes of hash for GetHashCode
        return BitConverter.ToInt32(hash, 0);
    }

    public override string ToString()
    {
        return ToHex();
    }

    public static bool operator ==(AdnlAddress? left, AdnlAddress? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(AdnlAddress? left, AdnlAddress? right)
    {
        return !Equals(left, right);
    }
}