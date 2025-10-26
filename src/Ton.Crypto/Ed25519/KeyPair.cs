namespace Ton.Crypto.Ed25519;

/// <summary>
/// Ed25519 key pair containing public and private keys.
/// </summary>
public class KeyPair
{
    /// <summary>
    /// Gets the 32-byte public key.
    /// </summary>
    public byte[] PublicKey { get; }

    /// <summary>
    /// Gets the 64-byte secret key (32-byte seed + 32-byte public key).
    /// </summary>
    public byte[] SecretKey { get; }

    /// <summary>
    /// Creates a new key pair.
    /// </summary>
    /// <param name="publicKey">32-byte public key.</param>
    /// <param name="secretKey">64-byte secret key.</param>
    public KeyPair(byte[] publicKey, byte[] secretKey)
    {
        if (publicKey.Length != 32)
            throw new ArgumentException("Public key must be 32 bytes", nameof(publicKey));
        if (secretKey.Length != 64)
            throw new ArgumentException("Secret key must be 64 bytes", nameof(secretKey));

        PublicKey = publicKey;
        SecretKey = secretKey;
    }
}

