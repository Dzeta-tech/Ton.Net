using System.Security.Cryptography;

namespace Ton.Adnl.Crypto;

/// <summary>
/// AES-256-CTR cipher for ADNL encryption/decryption.
/// Thread-safe implementation of Counter (CTR) mode encryption.
/// </summary>
public sealed class AdnlCipher : IDisposable
{
    private readonly Aes _aes;
    private readonly byte[] _counter;
    private readonly object _lock = new();
    private byte[] _remainingKeyStream;
    private int _remainingKeyStreamIndex;
    private bool _disposed;

    /// <summary>
    /// Creates a new AES-256-CTR cipher.
    /// </summary>
    /// <param name="key">256-bit (32-byte) encryption key.</param>
    /// <param name="initialCounter">128-bit (16-byte) initial counter value (IV).</param>
    public AdnlCipher(byte[] key, byte[] initialCounter)
    {
        if (key == null)
            throw new ArgumentNullException(nameof(key));
        if (key.Length != 32)
            throw new ArgumentException("Key must be 256 bits (32 bytes)", nameof(key));
        if (initialCounter == null)
            throw new ArgumentNullException(nameof(initialCounter));
        if (initialCounter.Length != 16)
            throw new ArgumentException("Counter must be 128 bits (16 bytes)", nameof(initialCounter));

        _counter = (byte[])initialCounter.Clone();
        _remainingKeyStream = new byte[16];
        _remainingKeyStreamIndex = 16; // Force generation on first use

        _aes = Aes.Create();
        _aes.Key = key;
        _aes.Mode = CipherMode.ECB; // ECB for encrypting the counter
        _aes.Padding = PaddingMode.None;
    }

    /// <summary>
    /// Encrypts or decrypts data (CTR mode is symmetric).
    /// </summary>
    /// <param name="data">Data to encrypt/decrypt.</param>
    /// <returns>Encrypted/decrypted data.</returns>
    public byte[] Process(byte[] data)
    {
        if (data == null)
            throw new ArgumentNullException(nameof(data));

        ObjectDisposedException.ThrowIf(_disposed, this);

        // Thread-safe: lock to prevent concurrent modification of counter state
        lock (_lock)
        {
            byte[] result = new byte[data.Length];

            for (int i = 0; i < data.Length; i++)
            {
                // Generate new key stream block if needed
                if (_remainingKeyStreamIndex >= 16)
                {
                    GenerateKeyStreamBlock();
                    _remainingKeyStreamIndex = 0;
                }

                // XOR data with key stream
                result[i] = (byte)(data[i] ^ _remainingKeyStream[_remainingKeyStreamIndex++]);
            }

            return result;
        }
    }

    /// <summary>
    /// Encrypts or decrypts data in-place (CTR mode is symmetric).
    /// </summary>
    /// <param name="data">Data to encrypt/decrypt in-place.</param>
    public void ProcessInPlace(Span<byte> data)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        // Thread-safe: lock to prevent concurrent modification of counter state
        lock (_lock)
        {
            for (int i = 0; i < data.Length; i++)
            {
                // Generate new key stream block if needed
                if (_remainingKeyStreamIndex >= 16)
                {
                    GenerateKeyStreamBlock();
                    _remainingKeyStreamIndex = 0;
                }

                // XOR data with key stream
                data[i] ^= _remainingKeyStream[_remainingKeyStreamIndex++];
            }
        }
    }

    /// <summary>
    /// Generates a new key stream block by encrypting the current counter.
    /// Increments the counter after generation.
    /// </summary>
    private void GenerateKeyStreamBlock()
    {
        using var encryptor = _aes.CreateEncryptor();
        encryptor.TransformBlock(_counter, 0, 16, _remainingKeyStream, 0);

        // Increment counter (little-endian)
        IncrementCounter();
    }

    /// <summary>
    /// Increments the counter in little-endian format.
    /// </summary>
    private void IncrementCounter()
    {
        for (int i = 15; i >= 0; i--)
        {
            if (++_counter[i] != 0)
                break; // No carry, done
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _aes?.Dispose();
            Array.Clear(_counter);
            Array.Clear(_remainingKeyStream);
            _disposed = true;
        }
    }
}

/// <summary>
/// Factory for creating ADNL ciphers.
/// </summary>
public static class AdnlCipherFactory
{
    /// <summary>
    /// Creates a new cipher for encryption.
    /// </summary>
    public static AdnlCipher CreateCipher(byte[] key, byte[] iv) => new AdnlCipher(key, iv);

    /// <summary>
    /// Creates a new cipher for decryption (same as encryption in CTR mode).
    /// </summary>
    public static AdnlCipher CreateDecipher(byte[] key, byte[] iv) => new AdnlCipher(key, iv);
}

