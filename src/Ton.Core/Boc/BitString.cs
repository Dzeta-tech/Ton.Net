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