namespace Ton.Core.Boc;

/// <summary>
///     Immutable bit string representation with efficient bit-level operations.
///     Used as the foundation for TON's Bag of Cells (BOC) serialization.
/// </summary>
public class BitString : IEquatable<BitString>
{
    /// <summary>
    ///     Empty bit string constant.
    /// </summary>
    public static readonly BitString Empty = new([], 0, 0);

    readonly byte[] data;
    readonly int offset;

    /// <summary>
    ///     Creates a new BitString from a buffer with specified offset and length in bits.
    /// </summary>
    /// <param name="data">The underlying byte buffer (should not be modified after construction).</param>
    /// <param name="offset">Bit offset from the start of the buffer.</param>
    /// <param name="length">Length of the bit string in bits.</param>
    /// <exception cref="ArgumentException">Thrown when length is negative.</exception>
    public BitString(byte[] data, int offset, int length)
    {
        if (length < 0)
            throw new ArgumentException($"Length {length} is out of bounds", nameof(length));

        this.data = data;
        this.offset = offset;
        Length = length;
    }

    /// <summary>
    ///     Gets the length of the bit string in bits.
    /// </summary>
    public int Length { get; }

    /// <summary>
    ///     Checks if this bit string equals another.
    /// </summary>
    /// <param name="other">The other bit string to compare.</param>
    /// <returns>True if equal, false otherwise.</returns>
    public bool Equals(BitString? other)
    {
        if (other == null)
            return false;
        if (Length != other.Length)
            return false;

        for (int i = 0; i < Length; i++)
            if (At(i) != other.At(i))
                return false;

        return true;
    }

    /// <summary>
    ///     Gets the bit at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the bit.</param>
    /// <returns>True if the bit is set, false otherwise.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when index is out of bounds.</exception>
    public bool At(int index)
    {
        if (index >= Length)
            throw new ArgumentOutOfRangeException(nameof(index), $"Index {index} > {Length} is out of bounds");
        if (index < 0)
            throw new ArgumentOutOfRangeException(nameof(index), $"Index {index} < 0 is out of bounds");

        // Calculate offsets (big-endian bit order)
        int byteIndex = (offset + index) >> 3;
        int bitIndex = 7 - (offset + index) % 8;

        return (data[byteIndex] & (1 << bitIndex)) != 0;
    }

    /// <summary>
    ///     Gets a substring of the bit string.
    /// </summary>
    /// <param name="offset">Bit offset from the start of this bit string.</param>
    /// <param name="length">Length of the substring in bits.</param>
    /// <returns>A new BitString representing the substring.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when offset or length is out of bounds.</exception>
    public BitString Substring(int offset, int length)
    {
        if (offset > Length)
            throw new ArgumentOutOfRangeException(nameof(offset), $"Offset({offset}) > {Length} is out of bounds");
        if (offset < 0)
            throw new ArgumentOutOfRangeException(nameof(offset), $"Offset({offset}) < 0 is out of bounds");

        // Empty string corner case
        if (length == 0)
            return Empty;

        if (offset + length > Length)
            throw new ArgumentOutOfRangeException(nameof(length),
                $"Offset {offset} + Length {length} > {Length} is out of bounds");

        return new BitString(data, this.offset + offset, length);
    }

    /// <summary>
    ///     Tries to get a byte buffer view of the bit string without allocations.
    ///     Only succeeds if the bit string is byte-aligned.
    /// </summary>
    /// <param name="offset">Bit offset from the start of this bit string.</param>
    /// <param name="length">Length in bits (must be multiple of 8).</param>
    /// <returns>A byte array view if aligned, null otherwise.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when offset or length is out of bounds.</exception>
    public byte[]? Subbuffer(int offset, int length)
    {
        if (offset > Length || offset < 0)
            throw new ArgumentOutOfRangeException(nameof(offset), $"Offset {offset} is out of bounds");
        if (offset + length > Length)
            throw new ArgumentOutOfRangeException(nameof(length),
                $"Offset + Length = {offset + length} is out of bounds");

        // Check alignment
        if (length % 8 != 0)
            return null;
        if ((this.offset + offset) % 8 != 0)
            return null;

        // Create view
        int start = (this.offset + offset) >> 3;
        int end = start + (length >> 3);
        return data[start..end];
    }

    /// <summary>
    ///     Converts the bit string to a byte array.
    ///     Works for any bit string, regardless of alignment.
    ///     If the length is not a multiple of 8, remaining bits in the last byte are set to 0.
    /// </summary>
    /// <returns>A byte array containing the bit string data.</returns>
    public byte[] ToBytes()
    {
        if (Length == 0)
            return [];

        int numBytes = (Length + 7) / 8;
        byte[] result = new byte[numBytes];

        for (int i = 0; i < Length; i++)
            if (At(i))
                result[i >> 3] |= (byte)(1 << (7 - i % 8));

        return result;
    }

    /// <summary>
    ///     Formats the bit string as a hexadecimal string with padding indicators.
    /// </summary>
    /// <returns>Hex string representation.</returns>
    public override string ToString()
    {
        byte[] padded = BitsToPaddedBuffer();

        if (Length % 4 == 0)
        {
            string s = Convert.ToHexString(padded[..(int)Math.Ceiling(Length / 8.0)]).ToUpper();
            if (Length % 8 == 0)
                return s;
            return s[..^1];
        }

        string hex = Convert.ToHexString(padded).ToUpper();
        if (Length % 8 <= 4)
            return hex[..^1] + "_";
        return hex + "_";
    }

    public override bool Equals(object? obj)
    {
        return obj is BitString other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Length);
    }

    /// <summary>
    ///     Parse a bit string from its canonical hex representation.
    /// </summary>
    /// <param name="str">Hex string (may end with "_" if length is not a multiple of 4).</param>
    /// <returns>Parsed BitString.</returns>
    public static BitString Parse(string str)
    {
        // Handle underscore suffix
        bool hasUnderscore = str.EndsWith('_');
        string hex = hasUnderscore ? str[..^1] : str;

        // Handle odd-length hex strings by padding with '0' (matching JS SDK behavior)
        if (hex.Length % 2 != 0) hex += '0';

        // Convert hex to bytes
        byte[] bytes = Convert.FromHexString(hex);

        // Calculate bit length
        int bitLength;
        if (hasUnderscore || (hex.Length > 0 && hex.Length % 2 != 0))
        {
            // Find the padding bit and calculate actual bit length (matching JS paddedBufferToBits)
            bitLength = 0;
            for (int i = bytes.Length - 1; i >= 0; i--)
                if (bytes[i] != 0)
                {
                    int testByte = bytes[i];
                    // Find rightmost set bit (padding bit)
                    int bitPos = testByte & -testByte;
                    if ((bitPos & 1) == 0)
                        // It's a power of 2 (only one bit set)
                        bitPos = (int)Math.Log2(bitPos) + 1;
                    else
                        bitPos = 0;

                    if (i > 0)
                        // Number of full bytes * 8
                        bitLength = i * 8;
                    bitLength += 8 - bitPos;
                    break;
                }
        }
        else
        {
            // No padding, all bits are data
            bitLength = hex.Length * 4;
        }

        return new BitString(bytes, 0, bitLength);
    }

    /// <summary>
    ///     Converts bits to a padded buffer for serialization.
    /// </summary>
    byte[] BitsToPaddedBuffer()
    {
        int paddedBits = (Length + 7) & -8; // Round up to nearest byte
        int bytes = paddedBits / 8;
        byte[] buffer = new byte[bytes];

        for (int i = 0; i < Length; i++)
            if (At(i))
                buffer[i >> 3] |= (byte)(1 << (7 - i % 8));

        // Add padding bit
        if (Length % 8 != 0) buffer[Length >> 3] |= (byte)(1 << (7 - Length % 8));

        return buffer;
    }
}