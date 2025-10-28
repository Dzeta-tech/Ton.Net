using System.Numerics;
using System.Security.Cryptography;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math.EC.Rfc8032;
using Ton.Crypto.Primitives;

namespace Ton.Adnl.Crypto;

/// <summary>
///     Manages ADNL key exchange using Ed25519/X25519.
///     Generates ephemeral keys and computes shared secrets for secure ADNL communication.
/// </summary>
public sealed class AdnlKeys
{
    byte[]? sharedSecret;

    /// <summary>
    ///     Creates a new instance with generated ephemeral keys.
    /// </summary>
    /// <param name="peerPublicKey">The peer's Ed25519 public key (32 bytes).</param>
    public AdnlKeys(byte[] peerPublicKey)
    {
        ArgumentNullException.ThrowIfNull(peerPublicKey);
        if (peerPublicKey.Length != 32)
            throw new ArgumentException("Peer public key must be 32 bytes", nameof(peerPublicKey));

        PeerPublicKey = peerPublicKey;

        // Generate ephemeral Ed25519 key pair
        byte[] seed = GenerateRandomBytes(32);
        byte[] publicKey = new byte[32];
        Ed25519.GeneratePublicKey(seed, 0, publicKey, 0);

        PrivateKey = new byte[64];
        Array.Copy(seed, 0, PrivateKey, 0, 32);
        Array.Copy(publicKey, 0, PrivateKey, 32, 32);
        PublicKey = publicKey;
    }

    /// <summary>
    ///     Gets the public key of this key pair (32 bytes).
    /// </summary>
    public byte[] PublicKey { get; }

    /// <summary>
    ///     Gets the private key of this key pair (64 bytes: 32-byte seed + 32-byte public key).
    /// </summary>
    public byte[] PrivateKey { get; }

    /// <summary>
    ///     Gets the peer's public key.
    /// </summary>
    public byte[] PeerPublicKey { get; }

    /// <summary>
    ///     Gets the shared secret computed via X25519 key exchange (32 bytes).
    ///     Computed lazily on first access.
    /// </summary>
    public byte[] SharedSecret
    {
        get
        {
            if (sharedSecret == null)
            {
                // Convert Ed25519 keys to X25519 (Curve25519) and compute shared secret
                byte[] privateX25519 = ConvertEd25519PrivateToX25519(PrivateKey);
                byte[] publicX25519 = ConvertEd25519PublicToX25519(PeerPublicKey);

                X25519PrivateKeyParameters x25519Private = new(privateX25519, 0);
                X25519PublicKeyParameters x25519Public = new(publicX25519, 0);

                sharedSecret = new byte[32];
                x25519Private.GenerateSecret(x25519Public, sharedSecret, 0);
            }

            return sharedSecret;
        }
    }

    /// <summary>
    ///     Generates cryptographically secure random bytes.
    /// </summary>
    /// <param name="count">Number of bytes to generate.</param>
    /// <returns>Random bytes.</returns>
    public static byte[] GenerateRandomBytes(int count)
    {
        if (count <= 0)
            throw new ArgumentOutOfRangeException(nameof(count), "Count must be positive");

        return RandomNumberGenerator.GetBytes(count);
    }

    /// <summary>
    ///     Converts an Ed25519 private key to an X25519 (Curve25519) private key.
    ///     Uses SHA-512 hash of the seed with clamping as per RFC 7748.
    /// </summary>
    static byte[] ConvertEd25519PrivateToX25519(byte[] ed25519PrivateKey)
    {
        if (ed25519PrivateKey.Length != 64)
            throw new ArgumentException("Ed25519 private key must be 64 bytes", nameof(ed25519PrivateKey));

        // Extract the 32-byte seed from the Ed25519 private key
        byte[] seed = new byte[32];
        Array.Copy(ed25519PrivateKey, 0, seed, 0, 32);

        // Hash the seed with SHA-512
        byte[] hash = Sha512.Hash(seed);

        // Clamp the hash to create a valid X25519 scalar
        hash[0] &= 248; // Clear lowest 3 bits
        hash[31] &= 127; // Clear highest bit
        hash[31] |= 64; // Set second highest bit

        // Return first 32 bytes as X25519 private key
        byte[] x25519Private = new byte[32];
        Array.Copy(hash, 0, x25519Private, 0, 32);
        return x25519Private;
    }

    /// <summary>
    ///     Converts an Ed25519 public key to an X25519 (Curve25519) public key.
    ///     Uses birationally equivalent Edwards to Montgomery curve transformation.
    /// </summary>
    static byte[] ConvertEd25519PublicToX25519(byte[] ed25519PublicKey)
    {
        if (ed25519PublicKey.Length != 32)
            throw new ArgumentException("Ed25519 public key must be 32 bytes", nameof(ed25519PublicKey));

        // Ed25519 prime: 2^255 - 19
        BigInteger p =
            BigInteger.Parse("57896044618658097711785492504343953926634992332820282019728792003956564819949");

        // Copy and clear sign bit
        byte[] yBytes = new byte[32];
        Array.Copy(ed25519PublicKey, yBytes, 32);
        yBytes[31] &= 0x7F; // Clear sign bit

        BigInteger y = new(yBytes, true);

        // Montgomery u = (1 + y) / (1 - y) mod p
        BigInteger numerator = (BigInteger.One + y) % p;
        BigInteger denominator = (BigInteger.One - y) % p;

        // Compute modular inverse: denominator^(-1) mod p
        // Using Fermat's little theorem: a^(p-2) ≡ a^(-1) (mod p) for prime p
        BigInteger denominatorInv = BigInteger.ModPow(denominator, p - 2, p);
        BigInteger u = numerator * denominatorInv % p;

        // Ensure positive
        if (u < 0)
            u += p;

        // Convert to 32-byte little-endian representation
        byte[] uBytes = u.ToByteArray(true);
        byte[] result = new byte[32];

        // Pad or trim to 32 bytes
        int copyLength = Math.Min(uBytes.Length, 32);
        Array.Copy(uBytes, 0, result, 0, copyLength);

        return result;
    }
}