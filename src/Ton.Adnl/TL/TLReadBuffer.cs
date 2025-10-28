using System.Buffers.Binary;
using System.Numerics;
using System.Text;

namespace Ton.Adnl.TL;

/// <summary>
///     TL (Type Language) deserialization buffer for reading.
///     Used to deserialize TL types according to TON protocol specifications.
/// </summary>
public sealed class TLReadBuffer
{
    readonly byte[] buffer;

    public TLReadBuffer(byte[] buffer)
    {
        this.buffer = buffer ?? throw new ArgumentNullException(nameof(buffer));
        Position = 0;
    }

    public TLReadBuffer(ReadOnlySpan<byte> buffer)
    {
        this.buffer = buffer.ToArray();
        Position = 0;
    }

    /// <summary>
    ///     Gets the current position in the buffer.
    /// </summary>
    public int Position { get; private set; }

    /// <summary>
    ///     Gets the total length of the buffer.
    /// </summary>
    public int Length => buffer.Length;

    /// <summary>
    ///     Gets the number of remaining unread bytes.
    /// </summary>
    public int Remaining => buffer.Length - Position;

    /// <summary>
    ///     Checks if there are at least the specified number of bytes available.
    /// </summary>
    void EnsureAvailable(int bytes)
    {
        if (Position + bytes > buffer.Length)
            throw new InvalidOperationException($"Not enough bytes: need {bytes}, have {Remaining}");
    }

    /// <summary>
    ///     Reads a 32-bit signed integer in little-endian format.
    /// </summary>
    public int ReadInt32()
    {
        EnsureAvailable(4);
        int value = BinaryPrimitives.ReadInt32LittleEndian(buffer.AsSpan(Position));
        Position += 4;
        return value;
    }

    /// <summary>
    ///     Reads a 32-bit unsigned integer in little-endian format.
    /// </summary>
    public uint ReadUInt32()
    {
        EnsureAvailable(4);
        uint value = BinaryPrimitives.ReadUInt32LittleEndian(buffer.AsSpan(Position));
        Position += 4;
        return value;
    }

    /// <summary>
    ///     Reads a 64-bit signed integer in little-endian format.
    /// </summary>
    public long ReadInt64()
    {
        EnsureAvailable(8);
        long value = BinaryPrimitives.ReadInt64LittleEndian(buffer.AsSpan(Position));
        Position += 8;
        return value;
    }

    /// <summary>
    ///     Reads a 64-bit unsigned integer in little-endian format.
    /// </summary>
    public ulong ReadUInt64()
    {
        EnsureAvailable(8);
        ulong value = BinaryPrimitives.ReadUInt64LittleEndian(buffer.AsSpan(Position));
        Position += 8;
        return value;
    }

    /// <summary>
    ///     Reads a single byte.
    /// </summary>
    public byte ReadUInt8()
    {
        EnsureAvailable(1);
        return buffer[Position++];
    }

    /// <summary>
    ///     Reads a 256-bit integer (32 bytes) in little-endian format.
    /// </summary>
    public byte[] ReadInt256()
    {
        EnsureAvailable(32);
        byte[] result = new byte[32];
        Array.Copy(buffer, Position, result, 0, 32);
        Position += 32;
        return result;
    }

    /// <summary>
    ///     Reads a 256-bit integer as a BigInteger.
    /// </summary>
    public BigInteger ReadInt256AsBigInteger()
    {
        EnsureAvailable(32);
        BigInteger value = new(buffer.AsSpan(Position, 32));
        Position += 32;
        return value;
    }

    /// <summary>
    ///     Reads a 128-bit integer (16 bytes) in little-endian format.
    /// </summary>
    public byte[] ReadInt128()
    {
        EnsureAvailable(16);
        byte[] result = new byte[16];
        Array.Copy(buffer, Position, result, 0, 16);
        Position += 16;
        return result;
    }

    /// <summary>
    ///     Reads a 128-bit integer as a BigInteger.
    /// </summary>
    public BigInteger ReadInt128AsBigInteger()
    {
        EnsureAvailable(16);
        BigInteger value = new(buffer.AsSpan(Position, 16));
        Position += 16;
        return value;
    }

    /// <summary>
    ///     Reads a fixed-size byte array (no length prefix).
    /// </summary>
    public byte[] ReadBytes(int size)
    {
        if (size < 0)
            throw new ArgumentOutOfRangeException(nameof(size));

        EnsureAvailable(size);
        byte[] result = new byte[size];
        Array.Copy(buffer, Position, result, 0, size);
        Position += size;
        return result;
    }

    /// <summary>
    ///     Reads a variable-length byte buffer with TL decoding:
    ///     - If first byte &lt;= 253: it's the length, followed by data and padding
    ///     - If first byte == 254: next 3 bytes are length (little-endian), followed by data and padding
    ///     Padding aligns total size (header + data) to 4-byte boundary.
    /// </summary>
    public byte[] ReadBuffer()
    {
        EnsureAvailable(1);
        int length;
        int headerSize;

        byte firstByte = buffer[Position];
        if (firstByte <= 253)
        {
            length = firstByte;
            headerSize = 1;
            Position += 1;
        }
        else // firstByte == 254
        {
            EnsureAvailable(4);
            length = buffer[Position + 1] |
                     (buffer[Position + 2] << 8) |
                     (buffer[Position + 3] << 16);
            headerSize = 4;
            Position += 4;
        }

        EnsureAvailable(length);
        byte[] result = new byte[length];
        Array.Copy(buffer, Position, result, 0, length);
        Position += length;

        // Skip padding to 4-byte alignment
        int totalSize = headerSize + length;
        int padding = (4 - totalSize % 4) % 4;
        Position += padding;

        return result;
    }

    /// <summary>
    ///     Reads a UTF-8 encoded string with TL buffer decoding.
    /// </summary>
    public string ReadString()
    {
        byte[] bytes = ReadBuffer();
        return Encoding.UTF8.GetString(bytes);
    }

    /// <summary>
    ///     Reads a boolean value using TL decoding:
    ///     - 0x997275b5: true
    ///     - 0xbc799737: false
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
    ///     Reads a vector (array) with TL decoding:
    ///     First reads the count as UInt32, then reads each element using the provided function.
    /// </summary>
    public T[] ReadVector<T>(Func<TLReadBuffer, T> readElement)
    {
        ArgumentNullException.ThrowIfNull(readElement);

        uint count = ReadUInt32();
        if (count > int.MaxValue)
            throw new InvalidOperationException($"Vector too large: {count}");

        T[] result = new T[count];
        for (int i = 0; i < count; i++)
            result[i] = readElement(this);

        return result;
    }

    /// <summary>
    ///     Reads all remaining bytes in the buffer.
    /// </summary>
    public byte[] ReadRemaining()
    {
        int remaining = Remaining;
        if (remaining == 0)
            return [];

        byte[] result = new byte[remaining];
        Array.Copy(buffer, Position, result, 0, remaining);
        Position += remaining;
        return result;
    }

    /// <summary>
    ///     Peeks at the next byte without advancing the position.
    /// </summary>
    public byte PeekByte()
    {
        EnsureAvailable(1);
        return buffer[Position];
    }

    /// <summary>
    ///     Peeks at the next 4 bytes as a UInt32 without advancing the position.
    /// </summary>
    public uint PeekUInt32()
    {
        EnsureAvailable(4);
        return BinaryPrimitives.ReadUInt32LittleEndian(buffer.AsSpan(Position));
    }

    /// <summary>
    ///     Skips the specified number of bytes.
    /// </summary>
    public void Skip(int bytes)
    {
        if (bytes < 0)
            throw new ArgumentOutOfRangeException(nameof(bytes));

        EnsureAvailable(bytes);
        Position += bytes;
    }

    /// <summary>
    ///     Resets the position to the beginning.
    /// </summary>
    public void Reset()
    {
        Position = 0;
    }
}