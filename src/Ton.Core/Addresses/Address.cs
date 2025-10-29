using System.Text.RegularExpressions;
using Ton.Core.Utils;

namespace Ton.Core.Addresses;

/// <summary>
///     Represents a TON blockchain address.
///     Addresses are immutable and can be parsed from both friendly (base64-encoded) and raw (workchain:hash) formats.
/// </summary>
public class Address : IEquatable<Address>
{
    const byte BounceableTag = 0x11;
    const byte NonBounceableTag = 0x51;
    const byte TestFlag = 0x80;

    static readonly Regex Base64Regex = new("^[A-Za-z0-9+/_-]+$", RegexOptions.Compiled);
    static readonly Regex HexRegex = new("^[a-f0-9]+$", RegexOptions.Compiled);

    /// <summary>
    ///     Creates a new TON address with the specified workchain and hash.
    /// </summary>
    /// <param name="workchain">The workchain ID.</param>
    /// <param name="hash">The 32-byte address hash.</param>
    /// <exception cref="ArgumentException">Thrown when hash length is not 32 bytes.</exception>
    public Address(int workchain, byte[] hash)
    {
        if (hash.Length != 32)
            throw new ArgumentException($"Invalid address hash length: {hash.Length}", nameof(hash));

        Workchain = workchain;
        Hash = hash;
    }

    /// <summary>
    ///     The workchain ID. Typically 0 for basechain or -1 for masterchain.
    /// </summary>
    public int Workchain { get; }

    /// <summary>
    ///     The 32-byte address hash.
    /// </summary>
    public byte[] Hash { get; }

    // Equality

    /// <summary>
    ///     Determines whether this address is equal to another address.
    /// </summary>
    /// <param name="other">The address to compare with.</param>
    /// <returns>True if the addresses are equal (same workchain and hash), false otherwise.</returns>
    public bool Equals(Address? other)
    {
        if (other == null)
            return false;

        if (Workchain != other.Workchain)
            return false;

        return Hash.SequenceEqual(other.Hash);
    }

    // Static factory methods

    /// <summary>
    ///     Parses an address from either friendly (base64-encoded) or raw (workchain:hash) format.
    /// </summary>
    /// <param name="source">The address string to parse.</param>
    /// <returns>The parsed address.</returns>
    /// <exception cref="ArgumentException">Thrown when the address format is invalid.</exception>
    public static Address Parse(string source)
    {
        if (IsFriendly(source)) return ParseFriendly(source).Address;

        if (IsRaw(source)) return ParseRaw(source);

        throw new ArgumentException($"Unknown address type: {source}", nameof(source));
    }

    /// <summary>
    ///     Parses an address from raw format (workchain:hash).
    /// </summary>
    /// <param name="source">The raw address string (e.g., "0:e4d954ef...").</param>
    /// <returns>The parsed address.</returns>
    /// <exception cref="ArgumentException">Thrown when the hash length is invalid or hex string is malformed.</exception>
    public static Address ParseRaw(string source)
    {
        string[] parts = source.Split(':');
        int workChain = int.Parse(parts[0]);

        string hexString = parts[1];

        // Validate hex string length (must be even and result in 32 bytes)
        if (hexString.Length % 2 != 0 || hexString.Length / 2 != 32)
        {
            int actualLength = hexString.Length / 2;
            throw new ArgumentException($"Invalid address hash length: {actualLength}", nameof(source));
        }

        try
        {
            byte[] hash = Convert.FromHexString(hexString);
            return new Address(workChain, hash);
        }
        catch (FormatException ex)
        {
            throw new ArgumentException("Invalid hex string in address hash", nameof(source), ex);
        }
    }

    /// <summary>
    ///     Parses an address from friendly (base64-encoded) format with metadata.
    /// </summary>
    /// <param name="source">The friendly address string (e.g., "EQAs9VlT...").</param>
    /// <returns>A tuple containing IsBounceable flag, IsTestOnly flag, and the parsed Address.</returns>
    /// <exception cref="ArgumentException">Thrown when the address format is invalid or checksum fails.</exception>
    public static (bool IsBounceable, bool IsTestOnly, Address Address) ParseFriendly(string source)
    {
        // Convert from URL-friendly to true base64
        string addr = source.Replace('-', '+').Replace('_', '/');

        try
        {
            byte[] data = Convert.FromBase64String(addr);
            return ParseFriendlyBytes(data);
        }
        catch (FormatException)
        {
            throw new ArgumentException("Unknown address type: byte length is not equal to 36", nameof(source));
        }
    }

    /// <summary>
    ///     Parses an address from friendly (base64-encoded) bytes with metadata.
    /// </summary>
    /// <param name="source">The friendly address bytes.</param>
    /// <returns>A tuple containing IsBounceable flag, IsTestOnly flag, and the parsed Address.</returns>
    /// <exception cref="ArgumentException">Thrown when the address format is invalid or checksum fails.</exception>
    public static (bool IsBounceable, bool IsTestOnly, Address Address) ParseFriendly(byte[] source)
    {
        return ParseFriendlyBytes(source);
    }

    static (bool IsBounceable, bool IsTestOnly, Address Address) ParseFriendlyBytes(byte[] data)
    {
        // 1byte tag + 1byte workchain + 32 bytes hash + 2 byte crc
        if (data.Length != 36)
            throw new ArgumentException("Unknown address type: byte length is not equal to 36");

        // Prepare data
        byte[] addr = data[..34];
        byte[] crc = data[34..36];
        byte[] calculatedCrc = Crc16.Compute(addr);

        if (calculatedCrc[0] != crc[0] || calculatedCrc[1] != crc[1])
            throw new ArgumentException("Invalid checksum");

        // Parse tag
        byte tag = addr[0];
        bool isTestOnly = false;

        if ((tag & TestFlag) != 0)
        {
            isTestOnly = true;
            tag ^= TestFlag;
        }

        if (tag != BounceableTag && tag != NonBounceableTag)
            throw new ArgumentException("Unknown address tag");

        bool isBounceable = tag == BounceableTag;

        int workChain = addr[1] == 0xff ? -1 : addr[1];
        byte[] hashPart = addr[2..34];

        return (isBounceable, isTestOnly, new Address(workChain, hashPart));
    }

    // Static validation methods

    /// <summary>
    ///     Checks if the given string is a valid friendly (base64-encoded) address format.
    /// </summary>
    /// <param name="source">The string to validate.</param>
    /// <returns>True if the string is a valid friendly address format, false otherwise.</returns>
    public static bool IsFriendly(string source)
    {
        // Check length
        if (source.Length != 48)
            return false;

        // Check if address is valid base64
        if (!Base64Regex.IsMatch(source))
            return false;

        return true;
    }

    /// <summary>
    ///     Checks if the given string is a valid raw (workchain:hash) address format.
    /// </summary>
    /// <param name="source">The string to validate.</param>
    /// <returns>True if the string is a valid raw address format, false otherwise.</returns>
    public static bool IsRaw(string source)
    {
        // Check if has delimiter
        if (!source.Contains(':'))
            return false;

        string[] parts = source.Split(':');
        if (parts.Length != 2)
            return false;

        string wc = parts[0];
        string hash = parts[1];

        // wc is not valid
        if (!int.TryParse(wc, out _))
            return false;

        // hash is not valid hex
        if (!HexRegex.IsMatch(hash.ToLower()))
            return false;

        // hash is not correct length
        if (hash.Length != 64)
            return false;

        return true;
    }

    /// <summary>
    ///     Normalizes an address string to its standard friendly format.
    /// </summary>
    /// <param name="source">The address string to normalize.</param>
    /// <returns>The normalized address in friendly format.</returns>
    public static string Normalize(string source)
    {
        return Parse(source).ToString();
    }

    /// <summary>
    ///     Normalizes an address to its standard friendly format.
    /// </summary>
    /// <param name="source">The address to normalize.</param>
    /// <returns>The normalized address in friendly format.</returns>
    public static string Normalize(Address source)
    {
        return source.ToString();
    }

    // Instance methods

    /// <summary>
    ///     Converts the address to raw format (workchain:hash).
    /// </summary>
    /// <returns>The address in raw string format (e.g., "0:e4d954ef...").</returns>
    public string ToRawString()
    {
        return $"{Workchain}:{Convert.ToHexString(Hash).ToLower()}";
    }

    /// <summary>
    ///     Converts the address to raw bytes format with workchain repeated.
    /// </summary>
    /// <returns>36-byte array: 32 bytes hash + 4 bytes workchain.</returns>
    public byte[] ToRaw()
    {
        byte[] addressWithChecksum = new byte[36];
        Array.Copy(Hash, addressWithChecksum, 32);
        addressWithChecksum[32] = (byte)Workchain;
        addressWithChecksum[33] = (byte)Workchain;
        addressWithChecksum[34] = (byte)Workchain;
        addressWithChecksum[35] = (byte)Workchain;
        return addressWithChecksum;
    }

    /// <summary>
    ///     Converts the address to friendly format bytes with checksum.
    /// </summary>
    /// <param name="bounceable">Whether the address should be bounceable (default: true).</param>
    /// <param name="testOnly">Whether this is a test-only address (default: false).</param>
    /// <returns>36-byte array: tag + workchain + 32-byte hash + 2-byte checksum.</returns>
    public byte[] ToStringBuffer(bool bounceable = true, bool testOnly = false)
    {
        byte tag = bounceable ? BounceableTag : NonBounceableTag;
        if (testOnly)
            tag |= TestFlag;

        byte[] addr = new byte[34];
        addr[0] = tag;
        addr[1] = (byte)Workchain;
        Array.Copy(Hash, 0, addr, 2, 32);

        byte[] addressWithChecksum = new byte[36];
        Array.Copy(addr, addressWithChecksum, 34);
        byte[] crc = Crc16.Compute(addr);
        addressWithChecksum[34] = crc[0];
        addressWithChecksum[35] = crc[1];

        return addressWithChecksum;
    }

    /// <summary>
    ///     Converts the address to friendly (base64-encoded) string format.
    /// </summary>
    /// <param name="urlSafe">Whether to use URL-safe base64 encoding (default: true).</param>
    /// <param name="bounceable">Whether the address should be bounceable (default: true).</param>
    /// <param name="testOnly">Whether this is a test-only address (default: false).</param>
    /// <returns>The address in friendly format (e.g., "EQAs9VlT...").</returns>
    public string ToString(bool urlSafe = true, bool bounceable = true, bool testOnly = false)
    {
        byte[] buffer = ToStringBuffer(bounceable, testOnly);
        string base64 = Convert.ToBase64String(buffer);

        if (urlSafe)
            return base64.Replace('+', '-').Replace('/', '_');
        return base64;
    }

    /// <summary>
    ///     Converts the address to its default string representation (URL-safe, bounceable, production).
    /// </summary>
    /// <returns>The address in friendly format.</returns>
    public override string ToString()
    {
        return ToString();
    }

    public override bool Equals(object? obj)
    {
        return obj is Address address && Equals(address);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Workchain, Hash.Length > 0 ? Hash[0] : 0);
    }

    public static bool operator ==(Address? left, Address? right)
    {
        if (ReferenceEquals(left, right))
            return true;
        if (left is null || right is null)
            return false;
        return left.Equals(right);
    }

    public static bool operator !=(Address? left, Address? right)
    {
        return !(left == right);
    }
}

/// <summary>
///     Extension methods for Address-related operations.
/// </summary>
public static class AddressExtensions
{
    /// <summary>
    ///     Parses a string as a TON address.
    ///     Convenience extension method matching JavaScript API style.
    /// </summary>
    /// <param name="source">The address string to parse.</param>
    /// <returns>The parsed address.</returns>
    /// <exception cref="ArgumentException">Thrown when the address format is invalid.</exception>
    public static Address ParseAddress(this string source)
    {
        return Address.Parse(source);
    }
}