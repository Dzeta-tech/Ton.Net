using System.Security.Cryptography;
using System.Text;

namespace Ton.Crypto.Primitives;

/// <summary>
/// PBKDF2-SHA512 key derivation functions.
/// </summary>
public static class Pbkdf2Sha512
{
    /// <summary>
    /// Derives a key using PBKDF2-SHA512.
    /// </summary>
    /// <param name="password">Password.</param>
    /// <param name="salt">Salt.</param>
    /// <param name="iterations">Number of iterations.</param>
    /// <param name="keyLength">Desired key length in bytes.</param>
    /// <returns>Derived key.</returns>
    public static byte[] DeriveKey(byte[] password, byte[] salt, int iterations, int keyLength)
    {
        using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA512);
        return pbkdf2.GetBytes(keyLength);
    }

    /// <summary>
    /// Derives a key using PBKDF2-SHA512 with string inputs (UTF-8 encoded).
    /// </summary>
    /// <param name="password">Password as string.</param>
    /// <param name="salt">Salt as string.</param>
    /// <param name="iterations">Number of iterations.</param>
    /// <param name="keyLength">Desired key length in bytes.</param>
    /// <returns>Derived key.</returns>
    public static byte[] DeriveKey(string password, string salt, int iterations, int keyLength)
    {
        return DeriveKey(Encoding.UTF8.GetBytes(password), Encoding.UTF8.GetBytes(salt), iterations, keyLength);
    }

    /// <summary>
    /// Derives a key using PBKDF2-SHA512 with mixed inputs.
    /// </summary>
    /// <param name="password">Password.</param>
    /// <param name="salt">Salt as string.</param>
    /// <param name="iterations">Number of iterations.</param>
    /// <param name="keyLength">Desired key length in bytes.</param>
    /// <returns>Derived key.</returns>
    public static byte[] DeriveKey(byte[] password, string salt, int iterations, int keyLength)
    {
        return DeriveKey(password, Encoding.UTF8.GetBytes(salt), iterations, keyLength);
    }

    /// <summary>
    /// Derives a key using PBKDF2-SHA512 with mixed inputs.
    /// </summary>
    /// <param name="password">Password as string.</param>
    /// <param name="salt">Salt.</param>
    /// <param name="iterations">Number of iterations.</param>
    /// <param name="keyLength">Desired key length in bytes.</param>
    /// <returns>Derived key.</returns>
    public static byte[] DeriveKey(string password, byte[] salt, int iterations, int keyLength)
    {
        return DeriveKey(Encoding.UTF8.GetBytes(password), salt, iterations, keyLength);
    }
}

