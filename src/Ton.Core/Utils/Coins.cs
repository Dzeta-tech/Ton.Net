using System.Globalization;
using System.Numerics;
using System.Text.RegularExpressions;

namespace Ton.Core.Utils;

/// <summary>
///     Utility functions for converting between TON coins and nanotons.
///     1 TON = 1,000,000,000 nanotons
/// </summary>
public static partial class Coins
{
    const long NanoPower = 1_000_000_000L;

    /// <summary>
    ///     Converts TON coins to nanotons.
    /// </summary>
    /// <param name="value">The value in TON coins (can be string, long, or BigInteger).</param>
    /// <returns>The value in nanotons.</returns>
    /// <exception cref="ArgumentException">Thrown when the value is invalid or has insufficient precision.</exception>
    public static BigInteger ToNano(string value)
    {
        // Check sign
        bool neg = false;
        while (value.StartsWith('-'))
        {
            neg = !neg;
            value = value.Substring(1);
        }

        // Split string
        if (value == ".")
            throw new ArgumentException("Invalid number", nameof(value));

        string[] parts = value.Split('.');
        if (parts.Length > 2)
            throw new ArgumentException("Invalid number", nameof(value));

        // Prepare parts
        string whole = parts.Length > 0 ? parts[0] : "";
        string frac = parts.Length > 1 ? parts[1] : "";

        if (string.IsNullOrEmpty(whole))
            whole = "0";
        if (string.IsNullOrEmpty(frac))
            frac = "0";
        if (frac.Length > 9)
            throw new ArgumentException("Invalid number: too many decimal places", nameof(value));

        while (frac.Length < 9)
            frac += "0";

        // Convert
        BigInteger result = BigInteger.Parse(whole) * NanoPower + BigInteger.Parse(frac);
        if (neg)
            result = -result;

        return result;
    }

    /// <summary>
    ///     Converts TON coins to nanotons.
    /// </summary>
    /// <param name="value">The value in TON coins.</param>
    /// <returns>The value in nanotons.</returns>
    public static BigInteger ToNano(BigInteger value)
    {
        return value * NanoPower;
    }

    /// <summary>
    ///     Converts TON coins to nanotons.
    /// </summary>
    /// <param name="value">The value in TON coins.</param>
    /// <returns>The value in nanotons.</returns>
    /// <exception cref="ArgumentException">Thrown when the number doesn't have enough precision.</exception>
    public static BigInteger ToNano(double value)
    {
        if (!double.IsFinite(value))
            throw new ArgumentException("Invalid number", nameof(value));

        if (Math.Log10(value) <= 6)
        {
            string str = value.ToString("F9", CultureInfo.InvariantCulture);
            return ToNano(str);
        }

        if (value - Math.Truncate(value) == 0)
        {
            string str = value.ToString("F0", CultureInfo.InvariantCulture);
            return ToNano(str);
        }

        throw new ArgumentException("Not enough precision for a number value. Use string value instead", nameof(value));
    }

    /// <summary>
    ///     Converts nanotons to TON coins as a decimal string.
    /// </summary>
    /// <param name="value">The value in nanotons.</param>
    /// <returns>The value in TON coins as a string.</returns>
    public static string FromNano(BigInteger value)
    {
        bool neg = false;
        if (value < 0)
        {
            neg = true;
            value = -value;
        }

        // Convert fraction
        BigInteger frac = value % NanoPower;
        string fracStr = frac.ToString();
        while (fracStr.Length < 9)
            fracStr = "0" + fracStr;

        // Remove trailing zeros
        fracStr = MyRegex().Replace(fracStr, "$1");

        // Convert whole
        BigInteger whole = value / NanoPower;
        string wholeStr = whole.ToString();

        // Value
        string result = wholeStr + (fracStr == "0" ? "" : $".{fracStr}");
        if (neg)
            result = "-" + result;

        return result;
    }

    /// <summary>
    ///     Converts nanotons to TON coins as a decimal string.
    /// </summary>
    /// <param name="value">The value in nanotons.</param>
    /// <returns>The value in TON coins as a string.</returns>
    public static string FromNano(long value)
    {
        return FromNano(new BigInteger(value));
    }

    /// <summary>
    ///     Converts nanotons to TON coins as a decimal string.
    /// </summary>
    /// <param name="value">The value in nanotons as a string.</param>
    /// <returns>The value in TON coins as a string.</returns>
    public static string FromNano(string value)
    {
        return FromNano(BigInteger.Parse(value));
    }

    [GeneratedRegex("^([0-9]*[1-9]|0)(0*)$")]
    private static partial Regex MyRegex();
}