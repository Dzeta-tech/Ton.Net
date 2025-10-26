using System.Security.Cryptography;
using System.Text;

namespace Ton.Crypto.Primitives;

/// <summary>
///     SHA-512 hashing functions.
/// </summary>
public static class Sha512
{
    /// <summary>
    ///     Computes SHA-512 hash of the input.
    /// </summary>
    /// <param name="source">Input data as byte array.</param>
    /// <returns>64-byte SHA-512 hash.</returns>
    public static byte[] Hash(byte[] source)
    {
        return SHA512.HashData(source);
    }

    /// <summary>
    ///     Computes SHA-512 hash of the input string (UTF-8 encoded).
    /// </summary>
    /// <param name="source">Input string.</param>
    /// <returns>64-byte SHA-512 hash.</returns>
    public static byte[] Hash(string source)
    {
        return SHA512.HashData(Encoding.UTF8.GetBytes(source));
    }
}