namespace Ton.Core.Boc;

/// <summary>
/// Immutable bit string representation with efficient bit-level operations.
/// Used as the foundation for TON's Bag of Cells (BOC) serialization.
/// </summary>
public class BitString : IEquatable<BitString>
{
    /// <summary>
    /// Empty bit string constant.
    /// </summary>
    public static readonly BitString Empty = new(Array.Empty<byte>(), 0, 0);

    readonly byte[] _data;
    readonly int _offset;
    readonly int _length;

    /// <summary>
    /// Creates a new BitString from a buffer with specified offset and length in bits.
    /// </summary>
    /// <param name="data">The underlying byte buffer (should not be modified after construction).</param>
    /// <param name="offset">Bit offset from the start of the buffer.</param>
    /// <param name="length">Length of the bit string in bits.</param>
    /// <exception cref="ArgumentException">Thrown when length is negative.</exception>
    public BitString(byte[] data, int offset, int length)
    {
        if (length < 0)
            throw new ArgumentException($"Length {length} is out of bounds", nameof(length));

        _data = data;
        _offset = offset;
        _length = length;
    }

    /// <summary>
    /// Gets the length of the bit string in bits.
    /// </summary>
    public int Length => _length;

    /// <summary>
    /// Gets the bit at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the bit.</param>
    /// <returns>True if the bit is set, false otherwise.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when index is out of bounds.</exception>
    public bool At(int index)
    {
        if (index >= _length)
            throw new ArgumentOutOfRangeException(nameof(index), $"Index {index} > {_length} is out of bounds");
        if (index < 0)
            throw new ArgumentOutOfRangeException(nameof(index), $"Index {index} < 0 is out of bounds");

        // Calculate offsets (big-endian bit order)
        int byteIndex = (_offset + index) >> 3;
        int bitIndex = 7 - ((_offset + index) % 8);

        return (_data[byteIndex] & (1 << bitIndex)) != 0;
    }

    /// <summary>
    /// Gets a substring of the bit string.
    /// </summary>
    /// <param name="offset">Bit offset from the start of this bit string.</param>
    /// <param name="length">Length of the substring in bits.</param>
    /// <returns>A new BitString representing the substring.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when offset or length is out of bounds.</exception>
    public BitString Substring(int offset, int length)
    {
        if (offset > _length)
            throw new ArgumentOutOfRangeException(nameof(offset), $"Offset({offset}) > {_length} is out of bounds");
        if (offset < 0)
            throw new ArgumentOutOfRangeException(nameof(offset), $"Offset({offset}) < 0 is out of bounds");

        // Empty string corner case
        if (length == 0)
            return Empty;

        if (offset + length > _length)
            throw new ArgumentOutOfRangeException(nameof(length), $"Offset {offset} + Length {length} > {_length} is out of bounds");

        return new BitString(_data, _offset + offset, length);
    }

    /// <summary>
    /// Tries to get a byte buffer view of the bit string without allocations.
    /// Only succeeds if the bit string is byte-aligned.
    /// </summary>
    /// <param name="offset">Bit offset from the start of this bit string.</param>
    /// <param name="length">Length in bits (must be multiple of 8).</param>
    /// <returns>A byte array view if aligned, null otherwise.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when offset or length is out of bounds.</exception>
    public byte[]? Subbuffer(int offset, int length)
    {
        if (offset > _length)
            throw new ArgumentOutOfRangeException(nameof(offset), $"Offset {offset} is out of bounds");
        if (offset < 0)
            throw new ArgumentOutOfRangeException(nameof(offset), $"Offset {offset} is out of bounds");
        if (offset + length > _length)
            throw new ArgumentOutOfRangeException(nameof(length), $"Offset + Length = {offset + length} is out of bounds");

        // Check alignment
        if (length % 8 != 0)
            return null;
        if ((_offset + offset) % 8 != 0)
            return null;

        // Create view
        int start = (_offset + offset) >> 3;
        int end = start + (length >> 3);
        return _data[start..end];
    }

    /// <summary>
    /// Checks if this bit string equals another.
    /// </summary>
    /// <param name="other">The other bit string to compare.</param>
    /// <returns>True if equal, false otherwise.</returns>
    public bool Equals(BitString? other)
    {
        if (other == null)
            return false;
        if (_length != other._length)
            return false;

        for (int i = 0; i < _length; i++)
        {
            if (At(i) != other.At(i))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Formats the bit string as a hexadecimal string with padding indicators.
    /// </summary>
    /// <returns>Hex string representation.</returns>
    public override string ToString()
    {
        byte[] padded = BitsToPaddedBuffer();

        if (_length % 4 == 0)
        {
            string s = Convert.ToHexString(padded[..((int)Math.Ceiling(_length / 8.0))]).ToUpper();
            if (_length % 8 == 0)
                return s;
            else
                return s[..^1];
        }
        else
        {
            string hex = Convert.ToHexString(padded).ToUpper();
            if (_length % 8 <= 4)
                return hex[..^1] + "_";
            else
                return hex + "_";
        }
    }

    public override bool Equals(object? obj)
    {
        return obj is BitString other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_length);
    }

    /// <summary>
    /// Converts bits to a padded buffer for serialization.
    /// </summary>
    byte[] BitsToPaddedBuffer()
    {
        int paddedBits = (_length + 7) & (-8);  // Round up to nearest byte
        int bytes = paddedBits / 8;
        byte[] buffer = new byte[bytes];

        for (int i = 0; i < _length; i++)
        {
            if (At(i))
            {
                buffer[i >> 3] |= (byte)(1 << (7 - (i % 8)));
            }
        }

        // Add padding bit
        if (_length % 8 != 0)
        {
            buffer[_length >> 3] |= (byte)(1 << (7 - (_length % 8)));
        }

        return buffer;
    }
}

