using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Macs;
using Org.BouncyCastle.Crypto.Parameters;
using System.Security.Cryptography;

namespace Ton.Crypto.Ed25519;

/// <summary>
/// Secret-key authenticated encryption using XSalsa20-Poly1305 (NaCl secretbox).
/// </summary>
public static class SecretBox
{
    private const int KeySize = 32;
    private const int NonceSize = 24;
    private const int TagSize = 16;

    /// <summary>
    /// Encrypts and authenticates data using XSalsa20-Poly1305.
    /// </summary>
    /// <param name="data">Data to encrypt.</param>
    /// <param name="nonce">24-byte nonce.</param>
    /// <param name="key">32-byte secret key.</param>
    /// <returns>Encrypted data with 16-byte authentication tag prepended.</returns>
    public static byte[] Seal(byte[] data, byte[] nonce, byte[] key)
    {
        if (nonce == null || nonce.Length != NonceSize)
            throw new ArgumentException($"Nonce must be {NonceSize} bytes", nameof(nonce));
        if (key == null || key.Length != KeySize)
            throw new ArgumentException($"Key must be {KeySize} bytes", nameof(key));

        // XSalsa20-Poly1305: First use HSalsa20 to derive subkey, then use Salsa20-Poly1305
        byte[] subkey = new byte[32];
        HSalsa20(subkey, nonce, key);

        // Use the last 8 bytes of nonce for Salsa20
        byte[] salsa20Nonce = new byte[8];
        Array.Copy(nonce, 16, salsa20Nonce, 0, 8);

        // Encrypt with Salsa20
        var salsa = new Salsa20Engine();
        salsa.Init(true, new ParametersWithIV(new KeyParameter(subkey), salsa20Nonce));

        byte[] ciphertext = new byte[data.Length];
        salsa.ProcessBytes(data, 0, data.Length, ciphertext, 0);

        // Compute Poly1305 MAC
        var poly = new Poly1305();
        poly.Init(new KeyParameter(subkey));
        poly.BlockUpdate(ciphertext, 0, ciphertext.Length);

        byte[] tag = new byte[TagSize];
        poly.DoFinal(tag, 0);

        // Return tag + ciphertext
        byte[] result = new byte[TagSize + ciphertext.Length];
        Array.Copy(tag, 0, result, 0, TagSize);
        Array.Copy(ciphertext, 0, result, TagSize, ciphertext.Length);

        return result;
    }

    /// <summary>
    /// Decrypts and verifies data encrypted with XSalsa20-Poly1305.
    /// </summary>
    /// <param name="data">Encrypted data with 16-byte authentication tag prepended.</param>
    /// <param name="nonce">24-byte nonce.</param>
    /// <param name="key">32-byte secret key.</param>
    /// <returns>Decrypted data, or null if authentication fails.</returns>
    public static byte[]? Open(byte[] data, byte[] nonce, byte[] key)
    {
        if (nonce == null || nonce.Length != NonceSize)
            throw new ArgumentException($"Nonce must be {NonceSize} bytes", nameof(nonce));
        if (key == null || key.Length != KeySize)
            throw new ArgumentException($"Key must be {KeySize} bytes", nameof(key));
        if (data == null || data.Length < TagSize)
            return null;

        try
        {
            // Extract tag and ciphertext
            byte[] tag = new byte[TagSize];
            byte[] ciphertext = new byte[data.Length - TagSize];
            Array.Copy(data, 0, tag, 0, TagSize);
            Array.Copy(data, TagSize, ciphertext, 0, ciphertext.Length);

            // XSalsa20-Poly1305: First use HSalsa20 to derive subkey
            byte[] subkey = new byte[32];
            HSalsa20(subkey, nonce, key);

            // Verify Poly1305 MAC
            var poly = new Poly1305();
            poly.Init(new KeyParameter(subkey));
            poly.BlockUpdate(ciphertext, 0, ciphertext.Length);

            byte[] computedTag = new byte[TagSize];
            poly.DoFinal(computedTag, 0);

            // Constant-time comparison
            if (!CryptographicOperations.FixedTimeEquals(tag, computedTag))
                return null;

            // Use the last 8 bytes of nonce for Salsa20
            byte[] salsa20Nonce = new byte[8];
            Array.Copy(nonce, 16, salsa20Nonce, 0, 8);

            // Decrypt with Salsa20
            var salsa = new Salsa20Engine();
            salsa.Init(false, new ParametersWithIV(new KeyParameter(subkey), salsa20Nonce));

            byte[] plaintext = new byte[ciphertext.Length];
            salsa.ProcessBytes(ciphertext, 0, ciphertext.Length, plaintext, 0);

            return plaintext;
        }
        catch
        {
            return null;
        }
    }

    private static void HSalsa20(byte[] output, byte[] nonce, byte[] key)
    {
        // HSalsa20 core function - used to derive subkey from nonce and key
        uint[] x = new uint[16];

        // Constants "expand 32-byte k"
        x[0] = 0x61707865;
        x[5] = 0x3320646e;
        x[10] = 0x79622d32;
        x[15] = 0x6b206574;

        // Key
        x[1] = Load32(key, 0);
        x[2] = Load32(key, 4);
        x[3] = Load32(key, 8);
        x[4] = Load32(key, 12);
        x[11] = Load32(key, 16);
        x[12] = Load32(key, 20);
        x[13] = Load32(key, 24);
        x[14] = Load32(key, 28);

        // Nonce
        x[6] = Load32(nonce, 0);
        x[7] = Load32(nonce, 4);
        x[8] = Load32(nonce, 8);
        x[9] = Load32(nonce, 12);

        // 20 rounds
        for (int i = 0; i < 10; i++)
        {
            // Column round
            x[4] ^= Rotate(x[0] + x[12], 7);
            x[8] ^= Rotate(x[4] + x[0], 9);
            x[12] ^= Rotate(x[8] + x[4], 13);
            x[0] ^= Rotate(x[12] + x[8], 18);

            x[9] ^= Rotate(x[5] + x[1], 7);
            x[13] ^= Rotate(x[9] + x[5], 9);
            x[1] ^= Rotate(x[13] + x[9], 13);
            x[5] ^= Rotate(x[1] + x[13], 18);

            x[14] ^= Rotate(x[10] + x[6], 7);
            x[2] ^= Rotate(x[14] + x[10], 9);
            x[6] ^= Rotate(x[2] + x[14], 13);
            x[10] ^= Rotate(x[6] + x[2], 18);

            x[3] ^= Rotate(x[15] + x[11], 7);
            x[7] ^= Rotate(x[3] + x[15], 9);
            x[11] ^= Rotate(x[7] + x[3], 13);
            x[15] ^= Rotate(x[11] + x[7], 18);

            // Diagonal round
            x[1] ^= Rotate(x[0] + x[3], 7);
            x[2] ^= Rotate(x[1] + x[0], 9);
            x[3] ^= Rotate(x[2] + x[1], 13);
            x[0] ^= Rotate(x[3] + x[2], 18);

            x[6] ^= Rotate(x[5] + x[4], 7);
            x[7] ^= Rotate(x[6] + x[5], 9);
            x[4] ^= Rotate(x[7] + x[6], 13);
            x[5] ^= Rotate(x[4] + x[7], 18);

            x[11] ^= Rotate(x[10] + x[9], 7);
            x[8] ^= Rotate(x[11] + x[10], 9);
            x[9] ^= Rotate(x[8] + x[11], 13);
            x[10] ^= Rotate(x[9] + x[8], 18);

            x[12] ^= Rotate(x[15] + x[14], 7);
            x[13] ^= Rotate(x[12] + x[15], 9);
            x[14] ^= Rotate(x[13] + x[12], 13);
            x[15] ^= Rotate(x[14] + x[13], 18);
        }

        // Output
        Store32(output, 0, x[0]);
        Store32(output, 4, x[5]);
        Store32(output, 8, x[10]);
        Store32(output, 12, x[15]);
        Store32(output, 16, x[6]);
        Store32(output, 20, x[7]);
        Store32(output, 24, x[8]);
        Store32(output, 28, x[9]);
    }

    private static uint Load32(byte[] buffer, int offset)
    {
        return (uint)(buffer[offset] | (buffer[offset + 1] << 8) | (buffer[offset + 2] << 16) | (buffer[offset + 3] << 24));
    }

    private static void Store32(byte[] buffer, int offset, uint value)
    {
        buffer[offset] = (byte)value;
        buffer[offset + 1] = (byte)(value >> 8);
        buffer[offset + 2] = (byte)(value >> 16);
        buffer[offset + 3] = (byte)(value >> 24);
    }

    private static uint Rotate(uint v, int c)
    {
        return (v << c) | (v >> (32 - c));
    }
}


