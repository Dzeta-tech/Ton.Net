using System.Security.Cryptography;

namespace Ton.Crypto.Primitives;

/// <summary>
///     Secure random number generation.
/// </summary>
public static class SecureRandom
{
    /// <summary>
    ///     Generates cryptographically secure random bytes.
    /// </summary>
    /// <param name="size">Number of bytes to generate.</param>
    /// <returns>Random bytes.</returns>
    public static byte[] GetBytes(int size)
    {
        byte[] bytes = new byte[size];
        RandomNumberGenerator.Fill(bytes);
        return bytes;
    }

    /// <summary>
    ///     Generates a cryptographically secure random number in the specified range.
    /// </summary>
    /// <param name="min">Minimum value (inclusive).</param>
    /// <param name="max">Maximum value (exclusive).</param>
    /// <returns>Random number in range [min, max).</returns>
    public static int GetNumber(int min, int max)
    {
        if (min >= max)
            throw new ArgumentException("min must be less than max");

        long range = (long)max - min;
        if (range > 1L << 53)
            throw new ArgumentException("Range is too large");

        int bitsNeeded = (int)Math.Ceiling(Math.Log2(range));
        int bytesNeeded = (int)Math.Ceiling(bitsNeeded / 8.0);
        int mask = (1 << bitsNeeded) - 1;

        while (true)
        {
            byte[] randomBytes = GetBytes(bytesNeeded);

            int numberValue = 0;
            int power = (bytesNeeded - 1) * 8;
            for (int i = 0; i < bytesNeeded; i++)
            {
                numberValue += randomBytes[i] * (1 << power);
                power -= 8;
            }

            numberValue &= mask; // Truncate

            if (numberValue >= range)
                continue;

            return min + numberValue;
        }
    }
}