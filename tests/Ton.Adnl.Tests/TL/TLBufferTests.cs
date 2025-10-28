using System.Numerics;
using System.Text;
using Ton.Adnl.TL;
using Xunit;

namespace Ton.Adnl.Tests.TL;

public class TLBufferTests
{
    [Fact]
    public void WriteReadInt32_ShouldRoundTrip()
    {
        var writer = new TLWriteBuffer();
        writer.WriteInt32(42);
        writer.WriteInt32(-42);
        writer.WriteInt32(int.MaxValue);
        writer.WriteInt32(int.MinValue);

        var reader = new TLReadBuffer(writer.Build());
        Assert.Equal(42, reader.ReadInt32());
        Assert.Equal(-42, reader.ReadInt32());
        Assert.Equal(int.MaxValue, reader.ReadInt32());
        Assert.Equal(int.MinValue, reader.ReadInt32());
    }

    [Fact]
    public void WriteReadUInt32_ShouldRoundTrip()
    {
        var writer = new TLWriteBuffer();
        writer.WriteUInt32(0);
        writer.WriteUInt32(42u);
        writer.WriteUInt32(uint.MaxValue);
        writer.WriteUInt32(0xDEADBEEFu);

        var reader = new TLReadBuffer(writer.Build());
        Assert.Equal(0u, reader.ReadUInt32());
        Assert.Equal(42u, reader.ReadUInt32());
        Assert.Equal(uint.MaxValue, reader.ReadUInt32());
        Assert.Equal(0xDEADBEEFu, reader.ReadUInt32());
    }

    [Fact]
    public void WriteReadInt64_ShouldRoundTrip()
    {
        var writer = new TLWriteBuffer();
        writer.WriteInt64(42L);
        writer.WriteInt64(-42L);
        writer.WriteInt64(long.MaxValue);
        writer.WriteInt64(long.MinValue);

        var reader = new TLReadBuffer(writer.Build());
        Assert.Equal(42L, reader.ReadInt64());
        Assert.Equal(-42L, reader.ReadInt64());
        Assert.Equal(long.MaxValue, reader.ReadInt64());
        Assert.Equal(long.MinValue, reader.ReadInt64());
    }

    [Fact]
    public void WriteReadUInt8_ShouldRoundTrip()
    {
        var writer = new TLWriteBuffer();
        writer.WriteUInt8(0);
        writer.WriteUInt8(42);
        writer.WriteUInt8(255);

        var reader = new TLReadBuffer(writer.Build());
        Assert.Equal((byte)0, reader.ReadUInt8());
        Assert.Equal((byte)42, reader.ReadUInt8());
        Assert.Equal((byte)255, reader.ReadUInt8());
    }

    [Fact]
    public void WriteReadInt256_ShouldRoundTrip()
    {
        var writer = new TLWriteBuffer();
        var value1 = BigInteger.Parse("12345678901234567890123456789012345678901234567890");
        var value2 = BigInteger.Parse("-12345678901234567890123456789012345678901234567890");
        var value3 = BigInteger.Zero;

        writer.WriteInt256(value1);
        writer.WriteInt256(value2);
        writer.WriteInt256(value3);

        var reader = new TLReadBuffer(writer.Build());
        Assert.Equal(value1, reader.ReadInt256AsBigInteger());
        Assert.Equal(value2, reader.ReadInt256AsBigInteger());
        Assert.Equal(value3, reader.ReadInt256AsBigInteger());
    }

    [Fact]
    public void WriteReadBytes_ShouldRoundTrip()
    {
        var writer = new TLWriteBuffer();
        var data = new byte[] { 1, 2, 3, 4, 5 };
        writer.WriteBytes(data, 5);

        var reader = new TLReadBuffer(writer.Build());
        var result = reader.ReadBytes(5);
        Assert.Equal(data, result);
    }

    [Fact]
    public void WriteReadBuffer_ShortLength_ShouldRoundTrip()
    {
        // Length <= 253: uses 1-byte length prefix
        var writer = new TLWriteBuffer();
        var data = new byte[] { 1, 2, 3, 4, 5 };
        writer.WriteBuffer(data);

        var bytes = writer.Build();
        // Check: 1 byte length (5) + 5 bytes data + 2 bytes padding = 8 bytes total
        Assert.Equal(8, bytes.Length);
        Assert.Equal(5, bytes[0]); // length prefix

        var reader = new TLReadBuffer(bytes);
        var result = reader.ReadBuffer();
        Assert.Equal(data, result);
    }

    [Fact]
    public void WriteReadBuffer_LongLength_ShouldRoundTrip()
    {
        // Length > 253: uses 4-byte length prefix (0xFE + 3 bytes)
        var writer = new TLWriteBuffer();
        var data = new byte[300];
        for (int i = 0; i < data.Length; i++)
            data[i] = (byte)(i % 256);
        
        writer.WriteBuffer(data);

        var bytes = writer.Build();
        // Check: 4 bytes length prefix + 300 bytes data + 0 bytes padding = 304 bytes
        Assert.Equal(304, bytes.Length);
        Assert.Equal(0xFE, bytes[0]); // long length marker

        var reader = new TLReadBuffer(bytes);
        var result = reader.ReadBuffer();
        Assert.Equal(data, result);
    }

    [Fact]
    public void WriteReadBuffer_EmptyArray_ShouldRoundTrip()
    {
        var writer = new TLWriteBuffer();
        writer.WriteBuffer(Array.Empty<byte>());

        var bytes = writer.Build();
        // 1 byte length (0) + 0 bytes data + 3 bytes padding = 4 bytes
        Assert.Equal(4, bytes.Length);

        var reader = new TLReadBuffer(bytes);
        var result = reader.ReadBuffer();
        Assert.Empty(result);
    }

    [Fact]
    public void WriteReadString_ShouldRoundTrip()
    {
        var writer = new TLWriteBuffer();
        writer.WriteString("Hello, TON!");
        writer.WriteString("");
        writer.WriteString("Привет"); // Cyrillic

        var reader = new TLReadBuffer(writer.Build());
        Assert.Equal("Hello, TON!", reader.ReadString());
        Assert.Equal("", reader.ReadString());
        Assert.Equal("Привет", reader.ReadString());
    }

    [Fact]
    public void WriteReadBool_ShouldRoundTrip()
    {
        var writer = new TLWriteBuffer();
        writer.WriteBool(true);
        writer.WriteBool(false);

        var reader = new TLReadBuffer(writer.Build());
        Assert.True(reader.ReadBool());
        Assert.False(reader.ReadBool());
    }

    [Fact]
    public void WriteReadVector_ShouldRoundTrip()
    {
        var writer = new TLWriteBuffer();
        var data = new int[] { 1, 2, 3, 4, 5 };
        writer.WriteVector(data, (w, item) => w.WriteInt32(item));

        var reader = new TLReadBuffer(writer.Build());
        var result = reader.ReadVector(r => r.ReadInt32());
        Assert.Equal(data, result);
    }

    [Fact]
    public void WriteReadVector_Empty_ShouldRoundTrip()
    {
        var writer = new TLWriteBuffer();
        writer.WriteVector(Array.Empty<int>(), (w, item) => w.WriteInt32(item));

        var reader = new TLReadBuffer(writer.Build());
        var result = reader.ReadVector(r => r.ReadInt32());
        Assert.Empty(result);
    }

    [Fact]
    public void TLReadBuffer_Remaining_ShouldBeCorrect()
    {
        var data = new byte[] { 1, 2, 3, 4, 5 };
        var reader = new TLReadBuffer(data);
        
        Assert.Equal(5, reader.Remaining);
        reader.ReadUInt8();
        Assert.Equal(4, reader.Remaining);
        reader.ReadUInt8();
        reader.ReadUInt8();
        Assert.Equal(2, reader.Remaining);
        reader.ReadUInt8();
        reader.ReadUInt8();
        Assert.Equal(0, reader.Remaining);
    }

    [Fact]
    public void TLReadBuffer_PeekByte_ShouldNotAdvancePosition()
    {
        var data = new byte[] { 42, 43, 44 };
        var reader = new TLReadBuffer(data);
        
        Assert.Equal((byte)42, reader.PeekByte());
        Assert.Equal((byte)42, reader.PeekByte()); // Should still be 42
        Assert.Equal((byte)42, reader.ReadUInt8()); // Now advance
        Assert.Equal((byte)43, reader.PeekByte());
    }

    [Fact]
    public void TLReadBuffer_PeekUInt32_ShouldNotAdvancePosition()
    {
        var writer = new TLWriteBuffer();
        writer.WriteUInt32(0xDEADBEEF);
        var data = writer.Build();
        
        var reader = new TLReadBuffer(data);
        Assert.Equal(0xDEADBEEFu, reader.PeekUInt32());
        Assert.Equal(0xDEADBEEFu, reader.PeekUInt32());
        Assert.Equal(0xDEADBEEFu, reader.ReadUInt32());
        Assert.Equal(0, reader.Remaining);
    }

    [Fact]
    public void TLReadBuffer_Skip_ShouldAdvancePosition()
    {
        var data = new byte[] { 1, 2, 3, 4, 5 };
        var reader = new TLReadBuffer(data);
        
        reader.Skip(2);
        Assert.Equal((byte)3, reader.ReadUInt8());
        reader.Skip(1);
        Assert.Equal((byte)5, reader.ReadUInt8());
    }

    [Fact]
    public void TLReadBuffer_Reset_ShouldResetPosition()
    {
        var data = new byte[] { 1, 2, 3, 4, 5 };
        var reader = new TLReadBuffer(data);
        
        reader.ReadUInt8();
        reader.ReadUInt8();
        Assert.Equal(3, reader.Remaining);
        
        reader.Reset();
        Assert.Equal(5, reader.Remaining);
        Assert.Equal((byte)1, reader.ReadUInt8());
    }

    [Fact]
    public void TLWriteBuffer_Reset_ShouldResetPosition()
    {
        var writer = new TLWriteBuffer();
        writer.WriteInt32(42);
        writer.WriteInt32(43);
        Assert.Equal(8, writer.Position);
        
        writer.Reset();
        Assert.Equal(0, writer.Position);
        
        writer.WriteInt32(100);
        var data = writer.Build();
        Assert.Equal(4, data.Length);
        
        var reader = new TLReadBuffer(data);
        Assert.Equal(100, reader.ReadInt32());
    }

    [Fact]
    public void TLReadBuffer_ReadRemaining_ShouldReadAll()
    {
        var data = new byte[] { 1, 2, 3, 4, 5 };
        var reader = new TLReadBuffer(data);
        
        reader.ReadUInt8();
        reader.ReadUInt8();
        
        var remaining = reader.ReadRemaining();
        Assert.Equal(new byte[] { 3, 4, 5 }, remaining);
        Assert.Equal(0, reader.Remaining);
    }

    [Fact]
    public void TLReadBuffer_NotEnoughBytes_ShouldThrow()
    {
        var data = new byte[] { 1, 2, 3 };
        var reader = new TLReadBuffer(data);
        
        Assert.Throws<InvalidOperationException>(() => reader.ReadInt32());
    }

    [Fact]
    public void TLWriteBuffer_AutoExpands_WhenCapacityExceeded()
    {
        var writer = new TLWriteBuffer(initialCapacity: 4);
        
        // Write more than initial capacity
        writer.WriteInt32(1);
        writer.WriteInt32(2);
        writer.WriteInt32(3);
        
        var data = writer.Build();
        Assert.Equal(12, data.Length);
        
        var reader = new TLReadBuffer(data);
        Assert.Equal(1, reader.ReadInt32());
        Assert.Equal(2, reader.ReadInt32());
        Assert.Equal(3, reader.ReadInt32());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(253)]
    [InlineData(254)]
    [InlineData(255)]
    [InlineData(300)]
    [InlineData(1000)]
    public void WriteReadBuffer_VariousLengths_ShouldRoundTrip(int length)
    {
        var writer = new TLWriteBuffer();
        var data = new byte[length];
        for (int i = 0; i < length; i++)
            data[i] = (byte)(i % 256);
        
        writer.WriteBuffer(data);
        
        var reader = new TLReadBuffer(writer.Build());
        var result = reader.ReadBuffer();
        Assert.Equal(data, result);
    }
}

