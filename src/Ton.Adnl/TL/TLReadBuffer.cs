using System.Buffers.Binary;
using System.Numerics;
using System.Text;

namespace Ton.Adnl.TL;

/// <summary>
/// TL (Type Language) deserialization buffer for reading.
/// Used to deserialize TL types according to TON protocol specifications.
/// </summary>
public sealed class TLReadBuffer
{
    private readonly byte[] _buffer;
    private int _position;

    public TLReadBuffer(byte[] buffer)
    {
        _buffer = buffer ?? throw new ArgumentNullException(nameof(buffer));
        _position = 0;
    }

    public TLReadBuffer(ReadOnlySpan<byte> buffer)
    {
        _buffer = buffer.ToArray();
        _position = 0;
    }

    /// <summary>
    /// Gets the current position in the buffer.
    /// </summary>
    public int Position => _position;

    /// <summary>
    /// Gets the total length of the buffer.
    /// </summary>
    public int Length => _buffer.Length;

    /// <summary>
    /// Gets the number of remaining unread bytes.
    /// </summary>
    public int Remaining => _buffer.Length - _position;

    /// <summary>
    /// Checks if there are at least the specified number of bytes available.
    /// </summary>
    private void EnsureAvailable(int bytes)
    {
        if (_position + bytes > _buffer.Length)
            throw new InvalidOperationException($"Not enough bytes: need {bytes}, have {Remaining}");
    }

    /// <summary>
    /// Reads a 32-bit signed integer in little-endian format.
    /// </summary>
    public int ReadInt32()
    {
        EnsureAvailable(4);
        int value = BinaryPrimitives.ReadInt32LittleEndian(_buffer.AsSpan(_position));
        _position += 4;
        return value;
    }

    /// <summary>
    /// Reads a 32-bit unsigned integer in little-endian format.
    /// </summary>
    public uint ReadUInt32()
    {
        EnsureAvailable(4);
        uint value = BinaryPrimitives.ReadUInt32LittleEndian(_buffer.AsSpan(_position));
        _position += 4;
        return value;
    }

    /// <summary>
    /// Reads a 64-bit signed integer in little-endian format.
    /// </summary>
    public long ReadInt64()
    {
        EnsureAvailable(8);
        long value = BinaryPrimitives.ReadInt64LittleEndian(_buffer.AsSpan(_position));
        _position += 8;
        return value;
    }

    /// <summary>
    /// Reads a 64-bit unsigned integer in little-endian format.
    /// </summary>
    public ulong ReadUInt64()
    {
        EnsureAvailable(8);
        ulong value = BinaryPrimitives.ReadUInt64LittleEndian(_buffer.AsSpan(_position));
        _position += 8;
        return value;
    }

    /// <summary>
    /// Reads a single byte.
    /// </summary>
    public byte ReadUInt8()
    {
        EnsureAvailable(1);
        return _buffer[_position++];
    }

    /// <summary>
    /// Reads a 256-bit integer (32 bytes) in little-endian format.
    /// </summary>
    public byte[] ReadInt256()
    {
        EnsureAvailable(32);
        byte[] result = new byte[32];
        Array.Copy(_buffer, _position, result, 0, 32);
        _position += 32;
        return result;
    }

    /// <summary>
    /// Reads a 256-bit integer as a BigInteger.
    /// </summary>
    public BigInteger ReadInt256AsBigInteger()
    {
        EnsureAvailable(32);
        var value = new BigInteger(_buffer.AsSpan(_position, 32), isUnsigned: false, isBigEndian: false);
        _position += 32;
        return value;
    }

    /// <summary>
    /// Reads a 128-bit integer (16 bytes) in little-endian format.
    /// </summary>
    public byte[] ReadInt128()
    {
        EnsureAvailable(16);
        byte[] result = new byte[16];
        Array.Copy(_buffer, _position, result, 0, 16);
        _position += 16;
        return result;
    }

    /// <summary>
    /// Reads a 128-bit integer as a BigInteger.
    /// </summary>
    public BigInteger ReadInt128AsBigInteger()
    {
        EnsureAvailable(16);
        var value = new BigInteger(_buffer.AsSpan(_position, 16), isUnsigned: false, isBigEndian: false);
        _position += 16;
        return value;
    }

    /// <summary>
    /// Reads a fixed-size byte array (no length prefix).
    /// </summary>
    public byte[] ReadBytes(int size)
    {
        if (size < 0)
            throw new ArgumentOutOfRangeException(nameof(size));

        EnsureAvailable(size);
        byte[] result = new byte[size];
        Array.Copy(_buffer, _position, result, 0, size);
        _position += size;
        return result;
    }

    /// <summary>
    /// Reads a variable-length byte buffer with TL decoding:
    /// - If first byte &lt;= 253: it's the length, followed by data and padding
    /// - If first byte == 254: next 3 bytes are length (little-endian), followed by data and padding
    /// Padding aligns total size (header + data) to 4-byte boundary.
    /// </summary>
    public byte[] ReadBuffer()
    {
        EnsureAvailable(1);
        int length;
        int headerSize;

        byte firstByte = _buffer[_position];
        if (firstByte <= 253)
        {
            length = firstByte;
            headerSize = 1;
            _position += 1;
        }
        else // firstByte == 254
        {
            EnsureAvailable(4);
            length = _buffer[_position + 1] |
                     (_buffer[_position + 2] << 8) |
                     (_buffer[_position + 3] << 16);
            headerSize = 4;
            _position += 4;
        }

        EnsureAvailable(length);
        byte[] result = new byte[length];
        Array.Copy(_buffer, _position, result, 0, length);
        _position += length;

        // Skip padding to 4-byte alignment
        int totalSize = headerSize + length;
        int padding = (4 - (totalSize % 4)) % 4;
        _position += padding;

        return result;
    }

    /// <summary>
    /// Reads a UTF-8 encoded string with TL buffer decoding.
    /// </summary>
    public string ReadString()
    {
        byte[] bytes = ReadBuffer();
        return Encoding.UTF8.GetString(bytes);
    }

    /// <summary>
    /// Reads a boolean value using TL decoding:
    /// - 0x997275b5: true
    /// - 0xbc799737: false
    /// </summary>
    public bool ReadBool()
    {
        EnsureAvailable(4);
        uint value = ReadUInt32();
        
        return value switch
        {
            0x997275b5u => true,
            0xbc799737u => false,
            _ => throw new InvalidOperationException($"Invalid boolean value: 0x{value:X8}")
        };
    }

    /// <summary>
    /// Reads a vector (array) with TL decoding:
    /// First reads the count as UInt32, then reads each element using the provided function.
    /// </summary>
    public T[] ReadVector<T>(Func<TLReadBuffer, T> readElement)
    {
        if (readElement == null)
            throw new ArgumentNullException(nameof(readElement));

        uint count = ReadUInt32();
        if (count > int.MaxValue)
            throw new InvalidOperationException($"Vector too large: {count}");

        var result = new T[count];
        for (int i = 0; i < count; i++)
            result[i] = readElement(this);

        return result;
    }

    /// <summary>
    /// Reads all remaining bytes in the buffer.
    /// </summary>
    public byte[] ReadRemaining()
    {
        int remaining = Remaining;
        if (remaining == 0)
            return Array.Empty<byte>();

        byte[] result = new byte[remaining];
        Array.Copy(_buffer, _position, result, 0, remaining);
        _position += remaining;
        return result;
    }

    /// <summary>
    /// Peeks at the next byte without advancing the position.
    /// </summary>
    public byte PeekByte()
    {
        EnsureAvailable(1);
        return _buffer[_position];
    }

    /// <summary>
    /// Peeks at the next 4 bytes as a UInt32 without advancing the position.
    /// </summary>
    public uint PeekUInt32()
    {
        EnsureAvailable(4);
        return BinaryPrimitives.ReadUInt32LittleEndian(_buffer.AsSpan(_position));
    }

    /// <summary>
    /// Skips the specified number of bytes.
    /// </summary>
    public void Skip(int bytes)
    {
        if (bytes < 0)
            throw new ArgumentOutOfRangeException(nameof(bytes));

        EnsureAvailable(bytes);
        _position += bytes;
    }

    /// <summary>
    /// Resets the position to the beginning.
    /// </summary>
    public void Reset()
    {
        _position = 0;
    }
}

