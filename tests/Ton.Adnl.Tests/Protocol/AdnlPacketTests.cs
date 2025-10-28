using Ton.Adnl.Crypto;
using Ton.Adnl.Protocol;
using Xunit;

namespace Ton.Adnl.Tests.Protocol;

public class AdnlPacketTests
{
    [Fact]
    public void Constructor_WithValidPayload_ShouldSucceed()
    {
        var payload = "Hello, ADNL!"u8.ToArray();
        var packet = new AdnlPacket(payload);

        Assert.NotNull(packet);
        Assert.Equal(payload, packet.Payload);
        Assert.Equal(32, packet.Nonce.Length);
    }

    [Fact]
    public void Constructor_WithNullPayload_ShouldThrow()
    {
        Assert.Throws<ArgumentNullException>(() => new AdnlPacket(null!));
    }

    [Fact]
    public void Constructor_WithEmptyPayload_ShouldSucceed()
    {
        var packet = new AdnlPacket(Array.Empty<byte>());
        Assert.Empty(packet.Payload);
    }

    [Fact]
    public void Constructor_WithCustomNonce_ShouldUseProvidedNonce()
    {
        var payload = "Hello"u8.ToArray();
        var nonce = AdnlKeys.GenerateRandomBytes(32);
        var packet = new AdnlPacket(payload, nonce);

        Assert.Equal(nonce, packet.Nonce);
    }

    [Fact]
    public void Constructor_WithInvalidNonceLength_ShouldThrow()
    {
        var payload = "Hello"u8.ToArray();
        Assert.Throws<ArgumentException>(() => new AdnlPacket(payload, new byte[31]));
        Assert.Throws<ArgumentException>(() => new AdnlPacket(payload, new byte[33]));
    }

    [Fact]
    public void Length_ShouldBeCorrect()
    {
        var payload = "Hello"u8.ToArray();
        var packet = new AdnlPacket(payload);

        // Length = 4 (size) + 32 (nonce) + payload.Length + 32 (hash)
        int expectedLength = 4 + 32 + payload.Length + 32;
        Assert.Equal(expectedLength, packet.Length);
    }

    [Fact]
    public void ToBytes_ShouldSerializeCorrectly()
    {
        var payload = "Hello"u8.ToArray();
        var packet = new AdnlPacket(payload);
        var bytes = packet.ToBytes();

        // Check length
        Assert.Equal(packet.Length, bytes.Length);

        // Check size field (first 4 bytes, little-endian)
        uint size = BitConverter.ToUInt32(bytes, 0);
        Assert.Equal((uint)(32 + payload.Length + 32), size);
    }

    [Fact]
    public void ToBytes_ShouldCacheResult()
    {
        var payload = "Hello"u8.ToArray();
        var packet = new AdnlPacket(payload);

        var bytes1 = packet.ToBytes();
        var bytes2 = packet.ToBytes();

        // Should return the same cached instance
        Assert.Same(bytes1, bytes2);
    }

    [Fact]
    public void TryParse_WithValidPacket_ShouldSucceed()
    {
        var payload = "Hello, ADNL!"u8.ToArray();
        var originalPacket = new AdnlPacket(payload);
        var bytes = originalPacket.ToBytes();

        var parsedPacket = AdnlPacket.TryParse(bytes);

        Assert.NotNull(parsedPacket);
        Assert.Equal(payload, parsedPacket.Payload);
        Assert.Equal(originalPacket.Nonce, parsedPacket.Nonce);
    }

    [Fact]
    public void TryParse_WithInsufficientData_ShouldReturnNull()
    {
        var data = new byte[3]; // Less than 4 bytes
        var packet = AdnlPacket.TryParse(data);

        Assert.Null(packet);
    }

    [Fact]
    public void TryParse_WithIncompletePacket_ShouldReturnNull()
    {
        var payload = "Hello"u8.ToArray();
        var packet = new AdnlPacket(payload);
        var bytes = packet.ToBytes();

        // Take only first half
        var incompleteData = bytes[..(bytes.Length / 2)];
        var parsedPacket = AdnlPacket.TryParse(incompleteData);

        Assert.Null(parsedPacket);
    }

    [Fact]
    public void TryParse_WithCorruptedHash_ShouldThrow()
    {
        var payload = "Hello"u8.ToArray();
        var packet = new AdnlPacket(payload);
        var bytes = packet.ToBytes();

        // Corrupt the hash (last 32 bytes)
        bytes[^1] ^= 0xFF;

        Assert.Throws<InvalidOperationException>(() => AdnlPacket.TryParse(bytes));
    }

    [Fact]
    public void TryParse_WithNullData_ShouldThrow()
    {
        Assert.Throws<ArgumentNullException>(() => AdnlPacket.TryParse(null!));
    }

    [Fact]
    public void Parse_WithValidPacket_ShouldSucceed()
    {
        var payload = "Hello"u8.ToArray();
        var originalPacket = new AdnlPacket(payload);
        var bytes = originalPacket.ToBytes();

        var parsedPacket = AdnlPacket.Parse(bytes);

        Assert.NotNull(parsedPacket);
        Assert.Equal(payload, parsedPacket.Payload);
    }

    [Fact]
    public void Parse_WithInsufficientData_ShouldThrow()
    {
        var data = new byte[3];
        Assert.Throws<InvalidOperationException>(() => AdnlPacket.Parse(data));
    }

    [Fact]
    public void SerializeDeserialize_ShouldRoundTrip()
    {
        var payload = "Test payload with special chars: 你好, мир!"u8.ToArray();
        var nonce = AdnlKeys.GenerateRandomBytes(32);
        var originalPacket = new AdnlPacket(payload, nonce);

        var bytes = originalPacket.ToBytes();
        var parsedPacket = AdnlPacket.Parse(bytes);

        Assert.Equal(originalPacket.Payload, parsedPacket.Payload);
        Assert.Equal(originalPacket.Nonce, parsedPacket.Nonce);
        Assert.Equal(originalPacket.Length, parsedPacket.Length);
    }

    [Fact]
    public void IsComplete_WithCompletePacket_ShouldReturnTrue()
    {
        var payload = "Hello"u8.ToArray();
        var packet = new AdnlPacket(payload);
        var bytes = packet.ToBytes();

        Assert.True(AdnlPacket.IsComplete(bytes));
    }

    [Fact]
    public void IsComplete_WithIncompletePacket_ShouldReturnFalse()
    {
        var payload = "Hello"u8.ToArray();
        var packet = new AdnlPacket(payload);
        var bytes = packet.ToBytes();

        // Take only half
        var incompleteData = bytes[..(bytes.Length / 2)];
        Assert.False(AdnlPacket.IsComplete(incompleteData));
    }

    [Fact]
    public void IsComplete_WithInsufficientData_ShouldReturnFalse()
    {
        var data = new byte[3];
        Assert.False(AdnlPacket.IsComplete(data));
    }

    [Fact]
    public void IsComplete_WithNullData_ShouldReturnFalse()
    {
        Assert.False(AdnlPacket.IsComplete(null!));
    }

    [Fact]
    public void IsComplete_WithEmptyData_ShouldReturnFalse()
    {
        Assert.False(AdnlPacket.IsComplete(Array.Empty<byte>()));
    }

    [Fact]
    public void GetPacketLength_ShouldReturnCorrectLength()
    {
        var payload = "Hello"u8.ToArray();
        var packet = new AdnlPacket(payload);
        var bytes = packet.ToBytes();

        int length = AdnlPacket.GetPacketLength(bytes);
        Assert.Equal(packet.Length, length);
        Assert.Equal(bytes.Length, length);
    }

    [Fact]
    public void GetPacketLength_WithInsufficientData_ShouldThrow()
    {
        var data = new byte[3];
        Assert.Throws<ArgumentException>(() => AdnlPacket.GetPacketLength(data));
    }

    [Fact]
    public void GetPacketLength_WithNullData_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() => AdnlPacket.GetPacketLength(null!));
    }

    [Fact]
    public void MinimumSize_ShouldBe68()
    {
        Assert.Equal(68, AdnlPacket.MinimumSize);
    }

    [Fact]
    public void EmptyPayloadPacket_ShouldHaveMinimumSize()
    {
        var packet = new AdnlPacket(Array.Empty<byte>());
        Assert.Equal(AdnlPacket.MinimumSize, packet.Length);
    }

    [Fact]
    public void LargePayload_ShouldSerializeCorrectly()
    {
        var payload = AdnlKeys.GenerateRandomBytes(10000);
        var packet = new AdnlPacket(payload);
        var bytes = packet.ToBytes();

        var parsedPacket = AdnlPacket.Parse(bytes);
        Assert.Equal(payload, parsedPacket.Payload);
    }

    [Fact]
    public void DifferentPayloads_ShouldProduceDifferentPackets()
    {
        var payload1 = "Hello"u8.ToArray();
        var payload2 = "World"u8.ToArray();

        var packet1 = new AdnlPacket(payload1);
        var packet2 = new AdnlPacket(payload2);

        var bytes1 = packet1.ToBytes();
        var bytes2 = packet2.ToBytes();

        Assert.NotEqual(bytes1, bytes2);
    }

    [Fact]
    public void SamePayloadDifferentNonce_ShouldProduceDifferentPackets()
    {
        var payload = "Hello"u8.ToArray();
        var nonce1 = AdnlKeys.GenerateRandomBytes(32);
        var nonce2 = AdnlKeys.GenerateRandomBytes(32);

        var packet1 = new AdnlPacket(payload, nonce1);
        var packet2 = new AdnlPacket(payload, nonce2);

        var bytes1 = packet1.ToBytes();
        var bytes2 = packet2.ToBytes();

        Assert.NotEqual(bytes1, bytes2);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    [InlineData(1000)]
    public void VariousPayloadSizes_ShouldRoundTrip(int size)
    {
        var payload = size > 0 ? AdnlKeys.GenerateRandomBytes(size) : Array.Empty<byte>();
        var originalPacket = new AdnlPacket(payload);
        var bytes = originalPacket.ToBytes();

        var parsedPacket = AdnlPacket.Parse(bytes);
        Assert.Equal(payload, parsedPacket.Payload);
    }
}

