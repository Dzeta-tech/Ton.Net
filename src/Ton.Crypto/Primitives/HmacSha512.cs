using System.Security.Cryptography;
using System.Text;

namespace Ton.Crypto.Primitives;

/// <summary>
///     HMAC-SHA512 functions for message authentication.
/// </summary>
public static class HmacSha512
{
    /// <summary>
    ///     Computes HMAC-SHA512 of the data with the given key.
    /// </summary>
    /// <param name="key">HMAC key.</param>
    /// <param name="data">Data to authenticate.</param>
    /// <returns>64-byte HMAC-SHA512.</returns>
    public static byte[] Hash(byte[] key, byte[] data)
    {
        return HMACSHA512.HashData(key, data);
    }

    /// <summary>
    ///     Computes HMAC-SHA512 with string inputs (UTF-8 encoded).
    /// </summary>
    /// <param name="key">HMAC key as string.</param>
    /// <param name="data">Data as string.</param>
    /// <returns>64-byte HMAC-SHA512.</returns>
    public static byte[] Hash(string key, string data)
    {
        return HMACSHA512.HashData(Encoding.UTF8.GetBytes(key), Encoding.UTF8.GetBytes(data));
    }

    /// <summary>
    ///     Computes HMAC-SHA512 with mixed inputs.
    /// </summary>
    /// <param name="key">HMAC key.</param>
    /// <param name="data">Data as string.</param>
    /// <returns>64-byte HMAC-SHA512.</returns>
    public static byte[] Hash(byte[] key, string data)
    {
        return HMACSHA512.HashData(key, Encoding.UTF8.GetBytes(data));
    }

    /// <summary>
    ///     Computes HMAC-SHA512 with mixed inputs.
    /// </summary>
    /// <param name="key">HMAC key as string.</param>
    /// <param name="data">Data.</param>
    /// <returns>64-byte HMAC-SHA512.</returns>
    public static byte[] Hash(string key, byte[] data)
    {
        return HMACSHA512.HashData(Encoding.UTF8.GetBytes(key), data);
    }
}