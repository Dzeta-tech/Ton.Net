using Ton.Adnl.Crypto;
using Ton.Crypto.Primitives;

namespace Ton.Adnl.Tests.Crypto;

public class AdnlAddressTests
{
    [Fact]
    public void Constructor_WithValidPublicKey_ShouldSucceed()
    {
        byte[] publicKey = AdnlKeys.GenerateRandomBytes(32);
        AdnlAddress address = new(publicKey);

        Assert.NotNull(address);
        Assert.Equal(32, address.Hash.Length);
    }

    [Fact]
    public void Constructor_WithNullPublicKey_ShouldThrow()
    {
        Assert.Throws<ArgumentNullException>(() => new AdnlAddress((byte[])null!));
    }

    [Fact]
    public void Constructor_WithInvalidLengthPublicKey_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() => new AdnlAddress(new byte[31]));
        Assert.Throws<ArgumentException>(() => new AdnlAddress(new byte[33]));
    }

    [Fact]
    public void Constructor_WithBase64PublicKey_ShouldSucceed()
    {
        byte[] publicKey = AdnlKeys.GenerateRandomBytes(32);
        string base64 = Convert.ToBase64String(publicKey);
        AdnlAddress address = new(base64);

        Assert.NotNull(address);
        Assert.Equal(32, address.Hash.Length);
    }

    [Fact]
    public void Constructor_WithInvalidBase64_ShouldThrow()
    {
        Assert.Throws<FormatException>(() => new AdnlAddress("invalid-base64!!!"));
    }

    [Fact]
    public void Constructor_WithNullOrEmptyBase64_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() => new AdnlAddress((string)null!));
        Assert.Throws<ArgumentException>(() => new AdnlAddress(""));
    }

    [Fact]
    public void Hash_ShouldBeSha256OfPublicKey()
    {
        byte[] publicKey = AdnlKeys.GenerateRandomBytes(32);
        AdnlAddress address = new(publicKey);
        
        // Hash should be SHA256(pub.ed25519_constructor + public_key)
        // pub.ed25519 constructor = 0xC6B41348
        byte[] data = new byte[36];
        data[0] = 0xC6;
        data[1] = 0xB4;
        data[2] = 0x13;
        data[3] = 0x48;
        Array.Copy(publicKey, 0, data, 4, 32);
        byte[] expectedHash = Sha256.Hash(data);

        Assert.Equal(expectedHash, address.Hash);
    }

    [Fact]
    public void Hash_ShouldReturnCopy()
    {
        byte[] publicKey = AdnlKeys.GenerateRandomBytes(32);
        AdnlAddress address = new(publicKey);

        byte[] hash1 = address.Hash;
        byte[] hash2 = address.Hash;

        // Should return different instances (copies)
        Assert.NotSame(hash1, hash2);
        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void ToHex_ShouldReturnLowercaseHexString()
    {
        byte[] publicKey = AdnlKeys.GenerateRandomBytes(32);
        AdnlAddress address = new(publicKey);
        string hex = address.ToHex();

        Assert.Equal(64, hex.Length); // 32 bytes = 64 hex chars
        Assert.All(hex, c => Assert.True(char.IsDigit(c) || c is >= 'a' and <= 'f'));
    }

    [Fact]
    public void ToBase64_ShouldReturnValidBase64String()
    {
        byte[] publicKey = AdnlKeys.GenerateRandomBytes(32);
        AdnlAddress address = new(publicKey);
        string base64 = address.ToBase64();

        // Should be able to decode it
        byte[] decoded = Convert.FromBase64String(base64);
        Assert.Equal(32, decoded.Length);
        Assert.Equal(address.Hash, decoded);
    }

    [Fact]
    public void ToString_ShouldReturnHexString()
    {
        byte[] publicKey = AdnlKeys.GenerateRandomBytes(32);
        AdnlAddress address = new(publicKey);

        Assert.Equal(address.ToHex(), address.ToString());
    }

    [Fact]
    public void Equals_WithSamePublicKey_ShouldReturnTrue()
    {
        byte[] publicKey = AdnlKeys.GenerateRandomBytes(32);
        AdnlAddress address1 = new(publicKey);
        AdnlAddress address2 = new(publicKey);

        Assert.True(address1.Equals(address2));
        Assert.True(address1 == address2);
        Assert.False(address1 != address2);
    }

    [Fact]
    public void Equals_WithDifferentPublicKeys_ShouldReturnFalse()
    {
        byte[] publicKey1 = AdnlKeys.GenerateRandomBytes(32);
        byte[] publicKey2 = AdnlKeys.GenerateRandomBytes(32);
        AdnlAddress address1 = new(publicKey1);
        AdnlAddress address2 = new(publicKey2);

        Assert.False(address1.Equals(address2));
        Assert.False(address1 == address2);
        Assert.True(address1 != address2);
    }

    [Fact]
    public void Equals_WithNull_ShouldReturnFalse()
    {
        byte[] publicKey = AdnlKeys.GenerateRandomBytes(32);
        AdnlAddress? address = new(publicKey);

        Assert.False(address.Equals(null));
        Assert.False(address == null);
        Assert.True(address != null);
    }

    [Fact]
    public void Equals_WithSameInstance_ShouldReturnTrue()
    {
        byte[] publicKey = AdnlKeys.GenerateRandomBytes(32);
        AdnlAddress address = new(publicKey);

        Assert.True(address.Equals(address));
        Assert.True(address == address);
    }

    [Fact]
    public void GetHashCode_WithSamePublicKey_ShouldBeSame()
    {
        byte[] publicKey = AdnlKeys.GenerateRandomBytes(32);
        AdnlAddress address1 = new(publicKey);
        AdnlAddress address2 = new(publicKey);

        Assert.Equal(address1.GetHashCode(), address2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_WithDifferentPublicKeys_ShouldBeDifferent()
    {
        byte[] publicKey1 = AdnlKeys.GenerateRandomBytes(32);
        byte[] publicKey2 = AdnlKeys.GenerateRandomBytes(32);
        AdnlAddress address1 = new(publicKey1);
        AdnlAddress address2 = new(publicKey2);

        // Hash codes might collide, but it's extremely unlikely with random keys
        Assert.NotEqual(address1.GetHashCode(), address2.GetHashCode());
    }

    [Fact]
    public void Constructor_WithBase64AndBinary_ShouldProduceSameAddress()
    {
        byte[] publicKey = AdnlKeys.GenerateRandomBytes(32);
        string base64 = Convert.ToBase64String(publicKey);

        AdnlAddress address1 = new(publicKey);
        AdnlAddress address2 = new(base64);

        Assert.Equal(address1, address2);
    }
}