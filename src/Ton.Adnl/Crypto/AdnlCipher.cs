using System.Security.Cryptography;

namespace Ton.Adnl.Crypto;

/// <summary>
///     AES-256-CTR cipher for ADNL encryption/decryption.
///     Thread-safe implementation of Counter (CTR) mode encryption.
/// </summary>
public sealed class AdnlCipher : IDisposable
{
    readonly Aes aes;
    readonly byte[] counter;
    readonly object @lock = new();
    readonly byte[] remainingKeyStream;
    bool disposed;
    int remainingKeyStreamIndex;

    /// <summary>
    ///     Creates a new AES-256-CTR cipher.
    /// </summary>
    /// <param name="key">256-bit (32-byte) encryption key.</param>
    /// <param name="initialCounter">128-bit (16-byte) initial counter value (IV).</param>
    public AdnlCipher(byte[] key, byte[] initialCounter)
    {
        ArgumentNullException.ThrowIfNull(key);
        if (key.Length != 32)
            throw new ArgumentException("Key must be 256 bits (32 bytes)", nameof(key));
        ArgumentNullException.ThrowIfNull(initialCounter);
        if (initialCounter.Length != 16)
            throw new ArgumentException("Counter must be 128 bits (16 bytes)", nameof(initialCounter));

        counter = (byte[])initialCounter.Clone();
        remainingKeyStream = new byte[16];
        remainingKeyStreamIndex = 16; // Force generation on first use

        aes = Aes.Create();
        aes.Key = key;
        aes.Mode = CipherMode.ECB; // ECB for encrypting the counter
        aes.Padding = PaddingMode.None;
    }

    public void Dispose()
    {
        if (disposed) return;

        aes.Dispose();
        Array.Clear(counter);
        Array.Clear(remainingKeyStream);
        disposed = true;
    }

    /// <summary>
    ///     Encrypts or decrypts data (CTR mode is symmetric).
    /// </summary>
    /// <param name="data">Data to encrypt/decrypt.</param>
    /// <returns>Encrypted/decrypted data.</returns>
    public byte[] Process(byte[] data)
    {
        ArgumentNullException.ThrowIfNull(data);

        if (disposed) throw new ObjectDisposedException(nameof(AdnlCipher));

        // Thread-safe: lock to prevent concurrent modification of counter state
        lock (@lock)
        {
            byte[] result = new byte[data.Length];

            for (int i = 0; i < data.Length; i++)
            {
                // Generate new key stream block if needed
                if (remainingKeyStreamIndex >= 16)
                {
                    GenerateKeyStreamBlock();
                    remainingKeyStreamIndex = 0;
                }

                // XOR data with key stream
                result[i] = (byte)(data[i] ^ remainingKeyStream[remainingKeyStreamIndex++]);
            }

            return result;
        }
    }

    /// <summary>
    ///     Encrypts or decrypts data in-place (CTR mode is symmetric).
    /// </summary>
    /// <param name="data">Data to encrypt/decrypt in-place.</param>
    public void ProcessInPlace(Span<byte> data)
    {
        if (disposed) throw new ObjectDisposedException(nameof(AdnlCipher));

        // Thread-safe: lock to prevent concurrent modification of counter state
        lock (@lock)
        {
            for (int i = 0; i < data.Length; i++)
            {
                // Generate new key stream block if needed
                if (remainingKeyStreamIndex >= 16)
                {
                    GenerateKeyStreamBlock();
                    remainingKeyStreamIndex = 0;
                }

                // XOR data with key stream
                data[i] ^= remainingKeyStream[remainingKeyStreamIndex++];
            }
        }
    }

    /// <summary>
    ///     Generates a new key stream block by encrypting the current counter.
    ///     Increments the counter after generation.
    /// </summary>
    void GenerateKeyStreamBlock()
    {
        using ICryptoTransform encryptor = aes.CreateEncryptor();
        encryptor.TransformBlock(counter, 0, 16, remainingKeyStream, 0);

        // Increment counter (little-endian)
        IncrementCounter();
    }

    /// <summary>
    ///     Increments the counter in little-endian format.
    /// </summary>
    void IncrementCounter()
    {
        for (int i = 15; i >= 0; i--)
            if (++counter[i] != 0)
                break; // No carry, done
    }
}

/// <summary>
///     Factory for creating ADNL ciphers.
/// </summary>
public static class AdnlCipherFactory
{
    /// <summary>
    ///     Creates a new cipher for encryption.
    /// </summary>
    public static AdnlCipher CreateCipher(byte[] key, byte[] iv)
    {
        return new AdnlCipher(key, iv);
    }

    /// <summary>
    ///     Creates a new cipher for decryption (same as encryption in CTR mode).
    /// </summary>
    public static AdnlCipher CreateDecipher(byte[] key, byte[] iv)
    {
        return new AdnlCipher(key, iv);
    }
}