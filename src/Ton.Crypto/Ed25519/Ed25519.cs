namespace Ton.Crypto.Ed25519;

/// <summary>
///     Ed25519 digital signature functions.
/// </summary>
public static class Ed25519
{
    /// <summary>
    ///     Generates a key pair from a 32-byte seed.
    /// </summary>
    /// <param name="seed">32-byte seed.</param>
    /// <returns>Key pair.</returns>
    public static KeyPair KeyPairFromSeed(byte[] seed)
    {
        if (seed.Length != 32)
            throw new ArgumentException("Seed must be 32 bytes", nameof(seed));

        byte[] publicKey = new byte[32];
        byte[] privateKey = new byte[64];

        // Generate the key pair using Bouncy Castle's Ed25519
        Org.BouncyCastle.Math.EC.Rfc8032.Ed25519.GeneratePublicKey(seed, 0, publicKey, 0);

        // Secret key is seed + public key (64 bytes total)
        Array.Copy(seed, 0, privateKey, 0, 32);
        Array.Copy(publicKey, 0, privateKey, 32, 32);

        return new KeyPair(publicKey, privateKey);
    }

    /// <summary>
    ///     Derives a key pair from a 64-byte secret key.
    /// </summary>
    /// <param name="secretKey">64-byte secret key (32-byte seed + 32-byte public key).</param>
    /// <returns>Key pair.</returns>
    public static KeyPair KeyPairFromSecretKey(byte[] secretKey)
    {
        if (secretKey.Length != 64)
            throw new ArgumentException("Secret key must be 64 bytes", nameof(secretKey));

        byte[] publicKey = new byte[32];
        Array.Copy(secretKey, 32, publicKey, 0, 32);

        return new KeyPair(publicKey, secretKey);
    }

    /// <summary>
    ///     Signs data with a secret key.
    /// </summary>
    /// <param name="data">Data to sign.</param>
    /// <param name="secretKey">64-byte secret key.</param>
    /// <returns>64-byte signature.</returns>
    public static byte[] Sign(byte[] data, byte[] secretKey)
    {
        if (secretKey.Length != 64)
            throw new ArgumentException("Secret key must be 64 bytes", nameof(secretKey));

        byte[] signature = new byte[64];

        // Extract the 32-byte seed from the secret key
        byte[] seed = new byte[32];
        Array.Copy(secretKey, 0, seed, 0, 32);

        Org.BouncyCastle.Math.EC.Rfc8032.Ed25519.Sign(seed, 0, data, 0, data.Length, signature, 0);

        return signature;
    }

    /// <summary>
    ///     Verifies a signature.
    /// </summary>
    /// <param name="data">Data that was signed.</param>
    /// <param name="signature">64-byte signature.</param>
    /// <param name="publicKey">32-byte public key.</param>
    /// <returns>True if signature is valid.</returns>
    public static bool SignVerify(byte[] data, byte[] signature, byte[] publicKey)
    {
        if (signature.Length != 64)
            throw new ArgumentException("Signature must be 64 bytes", nameof(signature));
        if (publicKey.Length != 32)
            throw new ArgumentException("Public key must be 32 bytes", nameof(publicKey));

        return Org.BouncyCastle.Math.EC.Rfc8032.Ed25519.Verify(signature, 0, publicKey, 0, data, 0, data.Length);
    }
}