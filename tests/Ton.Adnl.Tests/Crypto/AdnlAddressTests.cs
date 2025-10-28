using Ton.Adnl.Crypto;
using Xunit;

namespace Ton.Adnl.Tests.Crypto;

public class AdnlAddressTests
{
    [Fact]
    public void Constructor_WithValidPublicKey_ShouldSucceed()
    {
        var publicKey = AdnlKeys.GenerateRandomBytes(32);
        var address = new AdnlAddress(publicKey);

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
        var publicKey = AdnlKeys.GenerateRandomBytes(32);
        var base64 = Convert.ToBase64String(publicKey);
        var address = new AdnlAddress(base64);

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
        var publicKey = AdnlKeys.GenerateRandomBytes(32);
        var address = new AdnlAddress(publicKey);
        var expectedHash = Ton.Crypto.Primitives.Sha256.Hash(publicKey);

        Assert.Equal(expectedHash, address.Hash);
    }

    [Fact]
    public void Hash_ShouldReturnCopy()
    {
        var publicKey = AdnlKeys.GenerateRandomBytes(32);
        var address = new AdnlAddress(publicKey);

        var hash1 = address.Hash;
        var hash2 = address.Hash;

        // Should return different instances (copies)
        Assert.NotSame(hash1, hash2);
        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void ToHex_ShouldReturnLowercaseHexString()
    {
        var publicKey = AdnlKeys.GenerateRandomBytes(32);
        var address = new AdnlAddress(publicKey);
        var hex = address.ToHex();

        Assert.Equal(64, hex.Length); // 32 bytes = 64 hex chars
        Assert.All(hex, c => Assert.True(char.IsDigit(c) || (c >= 'a' && c <= 'f')));
    }

    [Fact]
    public void ToBase64_ShouldReturnValidBase64String()
    {
        var publicKey = AdnlKeys.GenerateRandomBytes(32);
        var address = new AdnlAddress(publicKey);
        var base64 = address.ToBase64();

        // Should be able to decode it
        var decoded = Convert.FromBase64String(base64);
        Assert.Equal(32, decoded.Length);
        Assert.Equal(address.Hash, decoded);
    }

    [Fact]
    public void ToString_ShouldReturnHexString()
    {
        var publicKey = AdnlKeys.GenerateRandomBytes(32);
        var address = new AdnlAddress(publicKey);

        Assert.Equal(address.ToHex(), address.ToString());
    }

    [Fact]
    public void Equals_WithSamePublicKey_ShouldReturnTrue()
    {
        var publicKey = AdnlKeys.GenerateRandomBytes(32);
        var address1 = new AdnlAddress(publicKey);
        var address2 = new AdnlAddress(publicKey);

        Assert.True(address1.Equals(address2));
        Assert.True(address1 == address2);
        Assert.False(address1 != address2);
    }

    [Fact]
    public void Equals_WithDifferentPublicKeys_ShouldReturnFalse()
    {
        var publicKey1 = AdnlKeys.GenerateRandomBytes(32);
        var publicKey2 = AdnlKeys.GenerateRandomBytes(32);
        var address1 = new AdnlAddress(publicKey1);
        var address2 = new AdnlAddress(publicKey2);

        Assert.False(address1.Equals(address2));
        Assert.False(address1 == address2);
        Assert.True(address1 != address2);
    }

    [Fact]
    public void Equals_WithNull_ShouldReturnFalse()
    {
        var publicKey = AdnlKeys.GenerateRandomBytes(32);
        var address = new AdnlAddress(publicKey);

        Assert.False(address.Equals(null));
        Assert.False(address == null);
        Assert.True(address != null);
    }

    [Fact]
    public void Equals_WithSameInstance_ShouldReturnTrue()
    {
        var publicKey = AdnlKeys.GenerateRandomBytes(32);
        var address = new AdnlAddress(publicKey);

        Assert.True(address.Equals(address));
        Assert.True(address == address);
    }

    [Fact]
    public void GetHashCode_WithSamePublicKey_ShouldBeSame()
    {
        var publicKey = AdnlKeys.GenerateRandomBytes(32);
        var address1 = new AdnlAddress(publicKey);
        var address2 = new AdnlAddress(publicKey);

        Assert.Equal(address1.GetHashCode(), address2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_WithDifferentPublicKeys_ShouldBeDifferent()
    {
        var publicKey1 = AdnlKeys.GenerateRandomBytes(32);
        var publicKey2 = AdnlKeys.GenerateRandomBytes(32);
        var address1 = new AdnlAddress(publicKey1);
        var address2 = new AdnlAddress(publicKey2);

        // Hash codes might collide, but it's extremely unlikely with random keys
        Assert.NotEqual(address1.GetHashCode(), address2.GetHashCode());
    }

    [Fact]
    public void Constructor_WithBase64AndBinary_ShouldProduceSameAddress()
    {
        var publicKey = AdnlKeys.GenerateRandomBytes(32);
        var base64 = Convert.ToBase64String(publicKey);

        var address1 = new AdnlAddress(publicKey);
        var address2 = new AdnlAddress(base64);

        Assert.Equal(address1, address2);
    }
}

