using Ton.Crypto.Primitives;

namespace Ton.Adnl.Crypto;

/// <summary>
/// ADNL address - SHA-256 hash of a public key.
/// Used to identify ADNL peers in the network.
/// </summary>
public sealed class AdnlAddress : IEquatable<AdnlAddress>
{
    private readonly byte[] _hash;

    /// <summary>
    /// Creates an ADNL address from a public key.
    /// </summary>
    /// <param name="publicKey">Ed25519 public key (32 bytes).</param>
    public AdnlAddress(byte[] publicKey)
    {
        if (publicKey == null)
            throw new ArgumentNullException(nameof(publicKey));
        if (publicKey.Length != 32)
            throw new ArgumentException("Public key must be 32 bytes", nameof(publicKey));

        _hash = Sha256.Hash(publicKey);
    }

    /// <summary>
    /// Creates an ADNL address from a base64-encoded public key.
    /// </summary>
    /// <param name="publicKeyBase64">Base64-encoded Ed25519 public key.</param>
    public AdnlAddress(string publicKeyBase64)
    {
        if (string.IsNullOrEmpty(publicKeyBase64))
            throw new ArgumentException("Public key cannot be null or empty", nameof(publicKeyBase64));

        byte[] publicKey = Convert.FromBase64String(publicKeyBase64);
        if (publicKey.Length != 32)
            throw new ArgumentException("Public key must be 32 bytes", nameof(publicKeyBase64));

        _hash = Sha256.Hash(publicKey);
    }

    /// <summary>
    /// Gets the address hash (32 bytes).
    /// </summary>
    public byte[] Hash => (byte[])_hash.Clone();

    /// <summary>
    /// Gets the address as a hex string.
    /// </summary>
    public string ToHex() => Convert.ToHexString(_hash).ToLowerInvariant();

    /// <summary>
    /// Gets the address as a base64 string.
    /// </summary>
    public string ToBase64() => Convert.ToBase64String(_hash);

    public bool Equals(AdnlAddress? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return _hash.AsSpan().SequenceEqual(other._hash);
    }

    public override bool Equals(object? obj) => obj is AdnlAddress other && Equals(other);

    public override int GetHashCode()
    {
        // Use first 4 bytes of hash for GetHashCode
        return BitConverter.ToInt32(_hash, 0);
    }

    public override string ToString() => ToHex();

    public static bool operator ==(AdnlAddress? left, AdnlAddress? right) => Equals(left, right);
    public static bool operator !=(AdnlAddress? left, AdnlAddress? right) => !Equals(left, right);
}

