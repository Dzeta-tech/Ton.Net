using System.Buffers.Binary;
using System.Numerics;
using System.Text;

namespace Ton.Adnl.TL;

/// <summary>
/// TL (Type Language) serialization buffer for writing.
/// Used to serialize TL types according to TON protocol specifications.
/// </summary>
public sealed class TLWriteBuffer
{
    private byte[] _buffer;
    private int _position;

    public TLWriteBuffer(int initialCapacity = 128)
    {
        if (initialCapacity <= 0)
            throw new ArgumentOutOfRangeException(nameof(initialCapacity), "Initial capacity must be positive");

        _buffer = new byte[initialCapacity];
        _position = 0;
    }

    /// <summary>
    /// Gets the current position in the buffer.
    /// </summary>
    public int Position => _position;

    /// <summary>
    /// Gets the current capacity of the buffer.
    /// </summary>
    public int Capacity => _buffer.Length;

    private void EnsureCapacity(int additionalBytes)
    {
        int required = _position + additionalBytes;
        if (required <= _buffer.Length)
            return;

        int newCapacity = Math.Max(_buffer.Length * 2, required);
        Array.Resize(ref _buffer, newCapacity);
    }

    /// <summary>
    /// Writes a 32-bit signed integer in little-endian format.
    /// </summary>
    public void WriteInt32(int value)
    {
        EnsureCapacity(4);
        BinaryPrimitives.WriteInt32LittleEndian(_buffer.AsSpan(_position), value);
        _position += 4;
    }

    /// <summary>
    /// Writes a 32-bit unsigned integer in little-endian format.
    /// </summary>
    public void WriteUInt32(uint value)
    {
        EnsureCapacity(4);
        BinaryPrimitives.WriteUInt32LittleEndian(_buffer.AsSpan(_position), value);
        _position += 4;
    }

    /// <summary>
    /// Writes a 64-bit signed integer in little-endian format.
    /// </summary>
    public void WriteInt64(long value)
    {
        EnsureCapacity(8);
        BinaryPrimitives.WriteInt64LittleEndian(_buffer.AsSpan(_position), value);
        _position += 8;
    }

    /// <summary>
    /// Writes a 64-bit unsigned integer in little-endian format.
    /// </summary>
    public void WriteUInt64(ulong value)
    {
        EnsureCapacity(8);
        BinaryPrimitives.WriteUInt64LittleEndian(_buffer.AsSpan(_position), value);
        _position += 8;
    }

    /// <summary>
    /// Writes a single byte.
    /// </summary>
    public void WriteUInt8(byte value)
    {
        EnsureCapacity(1);
        _buffer[_position++] = value;
    }

    /// <summary>
    /// Writes a 256-bit integer (32 bytes) in little-endian format.
    /// </summary>
    public void WriteInt256(BigInteger value)
    {
        EnsureCapacity(32);
        Span<byte> span = _buffer.AsSpan(_position, 32);
        span.Clear();

        if (value != 0)
        {
            if (!value.TryWriteBytes(span, out int bytesWritten, isUnsigned: false, isBigEndian: false))
                throw new InvalidOperationException("Failed to write Int256 value");

            // For negative numbers in two's complement, fill remaining bytes with 0xFF
            if (value < 0 && bytesWritten < 32)
                span[bytesWritten..].Fill(0xFF);
        }

        _position += 32;
    }

    /// <summary>
    /// Writes a 128-bit integer (16 bytes) in little-endian format.
    /// </summary>
    public void WriteInt128(BigInteger value)
    {
        EnsureCapacity(16);
        Span<byte> span = _buffer.AsSpan(_position, 16);
        span.Clear();

        if (value != 0)
        {
            if (!value.TryWriteBytes(span, out int bytesWritten, isUnsigned: false, isBigEndian: false))
                throw new InvalidOperationException("Failed to write Int128 value");

            // For negative numbers in two's complement, fill remaining bytes with 0xFF
            if (value < 0 && bytesWritten < 16)
                span[bytesWritten..].Fill(0xFF);
        }

        _position += 16;
    }

    /// <summary>
    /// Writes a fixed-size byte array (no length prefix).
    /// </summary>
    public void WriteBytes(ReadOnlySpan<byte> data, int expectedSize)
    {
        if (data.Length != expectedSize)
            throw new ArgumentException($"Expected {expectedSize} bytes, got {data.Length}");

        EnsureCapacity(expectedSize);
        data.CopyTo(_buffer.AsSpan(_position));
        _position += expectedSize;
    }

    /// <summary>
    /// Writes a variable-length byte buffer with TL encoding:
    /// - If length &lt;= 253: 1 byte length + data + padding to 4-byte alignment
    /// - If length &gt; 253: 0xFE + 3 bytes length (little-endian) + data + padding to 4-byte alignment
    /// </summary>
    public void WriteBuffer(ReadOnlySpan<byte> data)
    {
        int length = data.Length;
        int headerSize;
        int paddingSize;

        if (length <= 253)
        {
            headerSize = 1;
            paddingSize = (4 - ((1 + length) % 4)) % 4;
        }
        else
        {
            headerSize = 4;
            paddingSize = (4 - ((4 + length) % 4)) % 4;
        }

        EnsureCapacity(headerSize + length + paddingSize);

        // Write header
        if (length <= 253)
        {
            _buffer[_position++] = (byte)length;
        }
        else
        {
            _buffer[_position++] = 0xFE;
            _buffer[_position++] = (byte)(length & 0xFF);
            _buffer[_position++] = (byte)((length >> 8) & 0xFF);
            _buffer[_position++] = (byte)((length >> 16) & 0xFF);
        }

        // Write data
        data.CopyTo(_buffer.AsSpan(_position));
        _position += length;

        // Write padding (zeros)
        _buffer.AsSpan(_position, paddingSize).Clear();
        _position += paddingSize;
    }

    /// <summary>
    /// Writes a UTF-8 encoded string with TL buffer encoding.
    /// </summary>
    public void WriteString(string value)
    {
        if (value == null)
            throw new ArgumentNullException(nameof(value));

        int maxByteCount = Encoding.UTF8.GetMaxByteCount(value.Length);
        Span<byte> buffer = maxByteCount <= 256 ? stackalloc byte[maxByteCount] : new byte[maxByteCount];
        int actualBytes = Encoding.UTF8.GetBytes(value, buffer);
        WriteBuffer(buffer[..actualBytes]);
    }

    /// <summary>
    /// Writes a boolean value using TL encoding:
    /// - true: 0x997275b5
    /// - false: 0xbc799737
    /// </summary>
    public void WriteBool(bool value)
    {
        WriteUInt32(value ? 0x997275b5u : 0xbc799737u);
    }

    /// <summary>
    /// Writes a vector (array) with TL encoding:
    /// First writes the count as UInt32, then writes each element using the provided action.
    /// </summary>
    public void WriteVector<T>(T[] items, Action<TLWriteBuffer, T> writeElement)
    {
        if (items == null)
            throw new ArgumentNullException(nameof(items));
        if (writeElement == null)
            throw new ArgumentNullException(nameof(writeElement));

        WriteUInt32((uint)items.Length);
        foreach (var item in items)
            writeElement(this, item);
    }

    /// <summary>
    /// Builds and returns the serialized byte array.
    /// The buffer is trimmed to the actual written data.
    /// </summary>
    public byte[] Build()
    {
        byte[] result = new byte[_position];
        Array.Copy(_buffer, 0, result, 0, _position);
        return result;
    }

    /// <summary>
    /// Builds and returns the serialized data as a span.
    /// </summary>
    public ReadOnlySpan<byte> AsSpan() => _buffer.AsSpan(0, _position);

    /// <summary>
    /// Resets the buffer to reuse it.
    /// </summary>
    public void Reset()
    {
        _position = 0;
    }
}

