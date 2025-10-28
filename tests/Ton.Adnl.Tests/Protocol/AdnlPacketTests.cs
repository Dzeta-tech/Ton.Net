using Ton.Adnl.Crypto;
using Ton.Adnl.Protocol;

namespace Ton.Adnl.Tests.Protocol;

public class AdnlPacketTests
{
    [Fact]
    public void Constructor_WithValidPayload_ShouldSucceed()
    {
        byte[] payload = "Hello, ADNL!"u8.ToArray();
        AdnlPacket packet = new(payload);

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
        AdnlPacket packet = new([]);
        Assert.Empty(packet.Payload);
    }

    [Fact]
    public void Constructor_WithCustomNonce_ShouldUseProvidedNonce()
    {
        byte[] payload = "Hello"u8.ToArray();
        byte[] nonce = AdnlKeys.GenerateRandomBytes(32);
        AdnlPacket packet = new(payload, nonce);

        Assert.Equal(nonce, packet.Nonce);
    }

    [Fact]
    public void Constructor_WithInvalidNonceLength_ShouldThrow()
    {
        byte[] payload = "Hello"u8.ToArray();
        Assert.Throws<ArgumentException>(() => new AdnlPacket(payload, new byte[31]));
        Assert.Throws<ArgumentException>(() => new AdnlPacket(payload, new byte[33]));
    }

    [Fact]
    public void Length_ShouldBeCorrect()
    {
        byte[] payload = "Hello"u8.ToArray();
        AdnlPacket packet = new(payload);

        // Length = 4 (size) + 32 (nonce) + payload.Length + 32 (hash)
        int expectedLength = 4 + 32 + payload.Length + 32;
        Assert.Equal(expectedLength, packet.Length);
    }

    [Fact]
    public void ToBytes_ShouldSerializeCorrectly()
    {
        byte[] payload = "Hello"u8.ToArray();
        AdnlPacket packet = new(payload);
        byte[] bytes = packet.ToBytes();

        // Check length
        Assert.Equal(packet.Length, bytes.Length);

        // Check size field (first 4 bytes, little-endian)
        uint size = BitConverter.ToUInt32(bytes, 0);
        Assert.Equal((uint)(32 + payload.Length + 32), size);
    }

    [Fact]
    public void ToBytes_ShouldCacheResult()
    {
        byte[] payload = "Hello"u8.ToArray();
        AdnlPacket packet = new(payload);

        byte[] bytes1 = packet.ToBytes();
        byte[] bytes2 = packet.ToBytes();

        // Should return the same cached instance
        Assert.Same(bytes1, bytes2);
    }

    [Fact]
    public void TryParse_WithValidPacket_ShouldSucceed()
    {
        byte[] payload = "Hello, ADNL!"u8.ToArray();
        AdnlPacket originalPacket = new(payload);
        byte[] bytes = originalPacket.ToBytes();

        AdnlPacket? parsedPacket = AdnlPacket.TryParse(bytes);

        Assert.NotNull(parsedPacket);
        Assert.Equal(payload, parsedPacket.Payload);
        Assert.Equal(originalPacket.Nonce, parsedPacket.Nonce);
    }

    [Fact]
    public void TryParse_WithInsufficientData_ShouldReturnNull()
    {
        byte[] data = new byte[3]; // Less than 4 bytes
        AdnlPacket? packet = AdnlPacket.TryParse(data);

        Assert.Null(packet);
    }

    [Fact]
    public void TryParse_WithIncompletePacket_ShouldReturnNull()
    {
        byte[] payload = "Hello"u8.ToArray();
        AdnlPacket packet = new(payload);
        byte[] bytes = packet.ToBytes();

        // Take only first half
        byte[] incompleteData = bytes[..(bytes.Length / 2)];
        AdnlPacket? parsedPacket = AdnlPacket.TryParse(incompleteData);

        Assert.Null(parsedPacket);
    }

    [Fact]
    public void TryParse_WithCorruptedHash_ShouldThrow()
    {
        byte[] payload = "Hello"u8.ToArray();
        AdnlPacket packet = new(payload);
        byte[] bytes = packet.ToBytes();

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
        byte[] payload = "Hello"u8.ToArray();
        AdnlPacket originalPacket = new(payload);
        byte[] bytes = originalPacket.ToBytes();

        AdnlPacket parsedPacket = AdnlPacket.Parse(bytes);

        Assert.NotNull(parsedPacket);
        Assert.Equal(payload, parsedPacket.Payload);
    }

    [Fact]
    public void Parse_WithInsufficientData_ShouldThrow()
    {
        byte[] data = new byte[3];
        Assert.Throws<InvalidOperationException>(() => AdnlPacket.Parse(data));
    }

    [Fact]
    public void SerializeDeserialize_ShouldRoundTrip()
    {
        byte[] payload = "Test payload with special chars: 你好, мир!"u8.ToArray();
        byte[] nonce = AdnlKeys.GenerateRandomBytes(32);
        AdnlPacket originalPacket = new(payload, nonce);

        byte[] bytes = originalPacket.ToBytes();
        AdnlPacket parsedPacket = AdnlPacket.Parse(bytes);

        Assert.Equal(originalPacket.Payload, parsedPacket.Payload);
        Assert.Equal(originalPacket.Nonce, parsedPacket.Nonce);
        Assert.Equal(originalPacket.Length, parsedPacket.Length);
    }

    [Fact]
    public void IsComplete_WithCompletePacket_ShouldReturnTrue()
    {
        byte[] payload = "Hello"u8.ToArray();
        AdnlPacket packet = new(payload);
        byte[] bytes = packet.ToBytes();

        Assert.True(AdnlPacket.IsComplete(bytes));
    }

    [Fact]
    public void IsComplete_WithIncompletePacket_ShouldReturnFalse()
    {
        byte[] payload = "Hello"u8.ToArray();
        AdnlPacket packet = new(payload);
        byte[] bytes = packet.ToBytes();

        // Take only half
        byte[] incompleteData = bytes[..(bytes.Length / 2)];
        Assert.False(AdnlPacket.IsComplete(incompleteData));
    }

    [Fact]
    public void IsComplete_WithInsufficientData_ShouldReturnFalse()
    {
        byte[] data = new byte[3];
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
        Assert.False(AdnlPacket.IsComplete([]));
    }

    [Fact]
    public void GetPacketLength_ShouldReturnCorrectLength()
    {
        byte[] payload = "Hello"u8.ToArray();
        AdnlPacket packet = new(payload);
        byte[] bytes = packet.ToBytes();

        int length = AdnlPacket.GetPacketLength(bytes);
        Assert.Equal(packet.Length, length);
        Assert.Equal(bytes.Length, length);
    }

    [Fact]
    public void GetPacketLength_WithInsufficientData_ShouldThrow()
    {
        byte[] data = new byte[3];
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
        AdnlPacket packet = new([]);
        Assert.Equal(AdnlPacket.MinimumSize, packet.Length);
    }

    [Fact]
    public void LargePayload_ShouldSerializeCorrectly()
    {
        byte[] payload = AdnlKeys.GenerateRandomBytes(10000);
        AdnlPacket packet = new(payload);
        byte[] bytes = packet.ToBytes();

        AdnlPacket parsedPacket = AdnlPacket.Parse(bytes);
        Assert.Equal(payload, parsedPacket.Payload);
    }

    [Fact]
    public void DifferentPayloads_ShouldProduceDifferentPackets()
    {
        byte[] payload1 = "Hello"u8.ToArray();
        byte[] payload2 = "World"u8.ToArray();

        AdnlPacket packet1 = new(payload1);
        AdnlPacket packet2 = new(payload2);

        byte[] bytes1 = packet1.ToBytes();
        byte[] bytes2 = packet2.ToBytes();

        Assert.NotEqual(bytes1, bytes2);
    }

    [Fact]
    public void SamePayloadDifferentNonce_ShouldProduceDifferentPackets()
    {
        byte[] payload = "Hello"u8.ToArray();
        byte[] nonce1 = AdnlKeys.GenerateRandomBytes(32);
        byte[] nonce2 = AdnlKeys.GenerateRandomBytes(32);

        AdnlPacket packet1 = new(payload, nonce1);
        AdnlPacket packet2 = new(payload, nonce2);

        byte[] bytes1 = packet1.ToBytes();
        byte[] bytes2 = packet2.ToBytes();

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
        byte[] payload = size > 0 ? AdnlKeys.GenerateRandomBytes(size) : [];
        AdnlPacket originalPacket = new(payload);
        byte[] bytes = originalPacket.ToBytes();

        AdnlPacket parsedPacket = AdnlPacket.Parse(bytes);
        Assert.Equal(payload, parsedPacket.Payload);
    }
}