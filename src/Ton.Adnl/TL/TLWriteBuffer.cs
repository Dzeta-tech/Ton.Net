using System.Buffers.Binary;
using System.Numerics;
using System.Text;

namespace Ton.Adnl.TL;

/// <summary>
///     TL (Type Language) serialization buffer for writing.
///     Used to serialize TL types according to TON protocol specifications.
/// </summary>
public sealed class TLWriteBuffer
{
    byte[] buffer;

    public TLWriteBuffer(int initialCapacity = 128)
    {
        if (initialCapacity <= 0)
            throw new ArgumentOutOfRangeException(nameof(initialCapacity), "Initial capacity must be positive");

        buffer = new byte[initialCapacity];
        Position = 0;
    }

    /// <summary>
    ///     Gets the current position in the buffer.
    /// </summary>
    public int Position { get; private set; }

    /// <summary>
    ///     Gets the current capacity of the buffer.
    /// </summary>
    public int Capacity => buffer.Length;

    void EnsureCapacity(int additionalBytes)
    {
        int required = Position + additionalBytes;
        if (required <= buffer.Length)
            return;

        int newCapacity = Math.Max(buffer.Length * 2, required);
        Array.Resize(ref buffer, newCapacity);
    }

    /// <summary>
    ///     Writes a 32-bit signed integer in little-endian format.
    /// </summary>
    public void WriteInt32(int value)
    {
        EnsureCapacity(4);
        BinaryPrimitives.WriteInt32LittleEndian(buffer.AsSpan(Position), value);
        Position += 4;
    }

    /// <summary>
    ///     Writes a 32-bit unsigned integer in little-endian format.
    /// </summary>
    public void WriteUInt32(uint value)
    {
        EnsureCapacity(4);
        BinaryPrimitives.WriteUInt32LittleEndian(buffer.AsSpan(Position), value);
        Position += 4;
    }

    /// <summary>
    ///     Writes a 64-bit signed integer in little-endian format.
    /// </summary>
    public void WriteInt64(long value)
    {
        EnsureCapacity(8);
        BinaryPrimitives.WriteInt64LittleEndian(buffer.AsSpan(Position), value);
        Position += 8;
    }

    /// <summary>
    ///     Writes a 64-bit unsigned integer in little-endian format.
    /// </summary>
    public void WriteUInt64(ulong value)
    {
        EnsureCapacity(8);
        BinaryPrimitives.WriteUInt64LittleEndian(buffer.AsSpan(Position), value);
        Position += 8;
    }

    /// <summary>
    ///     Writes a single byte.
    /// </summary>
    public void WriteUInt8(byte value)
    {
        EnsureCapacity(1);
        buffer[Position++] = value;
    }

    /// <summary>
    ///     Writes a 256-bit integer (32 bytes) in little-endian format.
    /// </summary>
    public void WriteInt256(BigInteger value)
    {
        EnsureCapacity(32);
        Span<byte> span = buffer.AsSpan(Position, 32);
        span.Clear();

        if (value != 0)
        {
            if (!value.TryWriteBytes(span, out int bytesWritten))
                throw new InvalidOperationException("Failed to write Int256 value");

            // For negative numbers in two's complement, fill remaining bytes with 0xFF
            if (value < 0 && bytesWritten < 32)
                span[bytesWritten..].Fill(0xFF);
        }

        Position += 32;
    }

    /// <summary>
    ///     Writes a 128-bit integer (16 bytes) in little-endian format.
    /// </summary>
    public void WriteInt128(BigInteger value)
    {
        EnsureCapacity(16);
        Span<byte> span = buffer.AsSpan(Position, 16);
        span.Clear();

        if (value != 0)
        {
            if (!value.TryWriteBytes(span, out int bytesWritten))
                throw new InvalidOperationException("Failed to write Int128 value");

            // For negative numbers in two's complement, fill remaining bytes with 0xFF
            if (value < 0 && bytesWritten < 16)
                span[bytesWritten..].Fill(0xFF);
        }

        Position += 16;
    }

    /// <summary>
    ///     Writes a fixed-size byte array (no length prefix).
    /// </summary>
    public void WriteBytes(ReadOnlySpan<byte> data, int expectedSize)
    {
        if (data.Length != expectedSize)
            throw new ArgumentException($"Expected {expectedSize} bytes, got {data.Length}");

        EnsureCapacity(expectedSize);
        data.CopyTo(buffer.AsSpan(Position));
        Position += expectedSize;
    }

    /// <summary>
    ///     Writes a variable-length byte buffer with TL encoding:
    ///     - If length &lt;= 253: 1 byte length + data + padding to 4-byte alignment
    ///     - If length &gt; 253: 0xFE + 3 bytes length (little-endian) + data + padding to 4-byte alignment
    /// </summary>
    public void WriteBuffer(ReadOnlySpan<byte> data)
    {
        int length = data.Length;
        int headerSize;
        int paddingSize;

        if (length <= 253)
        {
            headerSize = 1;
            paddingSize = (4 - (1 + length) % 4) % 4;
        }
        else
        {
            headerSize = 4;
            paddingSize = (4 - (4 + length) % 4) % 4;
        }

        EnsureCapacity(headerSize + length + paddingSize);

        // Write header
        if (length <= 253)
        {
            buffer[Position++] = (byte)length;
        }
        else
        {
            buffer[Position++] = 0xFE;
            buffer[Position++] = (byte)(length & 0xFF);
            buffer[Position++] = (byte)((length >> 8) & 0xFF);
            buffer[Position++] = (byte)((length >> 16) & 0xFF);
        }

        // Write data
        data.CopyTo(buffer.AsSpan(Position));
        Position += length;

        // Write padding (zeros)
        buffer.AsSpan(Position, paddingSize).Clear();
        Position += paddingSize;
    }

    /// <summary>
    ///     Writes a UTF-8 encoded string with TL buffer encoding.
    /// </summary>
    public void WriteString(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        int maxByteCount = Encoding.UTF8.GetMaxByteCount(value.Length);
        Span<byte> buffer = maxByteCount <= 256 ? stackalloc byte[maxByteCount] : new byte[maxByteCount];
        int actualBytes = Encoding.UTF8.GetBytes(value, buffer);
        WriteBuffer(buffer[..actualBytes]);
    }

    /// <summary>
    ///     Writes a boolean value using TL encoding:
    ///     - true: 0x997275b5
    ///     - false: 0xbc799737
    /// </summary>
    public void WriteBool(bool value)
    {
        WriteUInt32(value ? 0x997275b5u : 0xbc799737u);
    }

    /// <summary>
    ///     Writes a vector (array) with TL encoding:
    ///     First writes the count as UInt32, then writes each element using the provided action.
    /// </summary>
    public void WriteVector<T>(T[] items, Action<TLWriteBuffer, T> writeElement)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(writeElement);

        WriteUInt32((uint)items.Length);
        foreach (T item in items)
            writeElement(this, item);
    }

    /// <summary>
    ///     Builds and returns the serialized byte array.
    ///     The buffer is trimmed to the actual written data.
    /// </summary>
    public byte[] Build()
    {
        byte[] result = new byte[Position];
        Array.Copy(buffer, 0, result, 0, Position);
        return result;
    }

    /// <summary>
    ///     Builds and returns the serialized data as a span.
    /// </summary>
    public ReadOnlySpan<byte> AsSpan()
    {
        return buffer.AsSpan(0, Position);
    }

    /// <summary>
    ///     Resets the buffer to reuse it.
    /// </summary>
    public void Reset()
    {
        Position = 0;
    }
}