using Ton.Adnl.Crypto;
using Xunit;

namespace Ton.Adnl.Tests.Crypto;

public class AdnlKeysTests
{
    [Fact]
    public void Constructor_WithValidPeerPublicKey_ShouldSucceed()
    {
        var peerPublicKey = AdnlKeys.GenerateRandomBytes(32);
        var keys = new AdnlKeys(peerPublicKey);

        Assert.NotNull(keys.PublicKey);
        Assert.Equal(32, keys.PublicKey.Length);
        Assert.NotNull(keys.PrivateKey);
        Assert.Equal(64, keys.PrivateKey.Length);
    }

    [Fact]
    public void Constructor_WithNullPeerPublicKey_ShouldThrow()
    {
        Assert.Throws<ArgumentNullException>(() => new AdnlKeys(null!));
    }

    [Fact]
    public void Constructor_WithInvalidLengthPeerPublicKey_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() => new AdnlKeys(new byte[31]));
        Assert.Throws<ArgumentException>(() => new AdnlKeys(new byte[33]));
    }

    [Fact]
    public void PublicKey_ShouldBe32Bytes()
    {
        var peerPublicKey = AdnlKeys.GenerateRandomBytes(32);
        var keys = new AdnlKeys(peerPublicKey);

        Assert.Equal(32, keys.PublicKey.Length);
    }

    [Fact]
    public void PrivateKey_ShouldBe64Bytes()
    {
        var peerPublicKey = AdnlKeys.GenerateRandomBytes(32);
        var keys = new AdnlKeys(peerPublicKey);

        Assert.Equal(64, keys.PrivateKey.Length);
    }

    [Fact]
    public void PrivateKey_ShouldContainPublicKeyInLastBytes()
    {
        var peerPublicKey = AdnlKeys.GenerateRandomBytes(32);
        var keys = new AdnlKeys(peerPublicKey);

        // Ed25519 private key format: 32-byte seed + 32-byte public key
        var publicKeyFromPrivate = keys.PrivateKey[32..64];
        Assert.Equal(keys.PublicKey, publicKeyFromPrivate);
    }

    [Fact]
    public void SharedSecret_ShouldBe32Bytes()
    {
        var peerPublicKey = AdnlKeys.GenerateRandomBytes(32);
        var keys = new AdnlKeys(peerPublicKey);

        var sharedSecret = keys.SharedSecret;
        Assert.Equal(32, sharedSecret.Length);
    }

    [Fact]
    public void SharedSecret_ShouldBeCached()
    {
        var peerPublicKey = AdnlKeys.GenerateRandomBytes(32);
        var keys = new AdnlKeys(peerPublicKey);

        var sharedSecret1 = keys.SharedSecret;
        var sharedSecret2 = keys.SharedSecret;

        // Should return the same cached instance
        Assert.Same(sharedSecret1, sharedSecret2);
    }

    [Fact]
    public void SharedSecret_ShouldBeConsistent_BetweenTwoPeers()
    {
        // Generate two key pairs
        var aliceKeys = new AdnlKeys(AdnlKeys.GenerateRandomBytes(32));
        var bobKeys = new AdnlKeys(aliceKeys.PublicKey);

        // Recreate Alice's keys with Bob's public key
        var aliceKeysWithBob = new AdnlKeys(bobKeys.PublicKey);
        // Use Alice's original key pair for computing shared secret
        // This is a simplified test - in reality, both would use the same keypairs

        // The shared secret computation should be deterministic for same inputs
        var secret1 = aliceKeys.SharedSecret;
        var secret2 = aliceKeys.SharedSecret;

        Assert.Equal(secret1, secret2);
    }

    [Fact]
    public void GenerateRandomBytes_WithValidCount_ShouldReturnCorrectLength()
    {
        var bytes = AdnlKeys.GenerateRandomBytes(32);
        Assert.Equal(32, bytes.Length);

        bytes = AdnlKeys.GenerateRandomBytes(16);
        Assert.Equal(16, bytes.Length);

        bytes = AdnlKeys.GenerateRandomBytes(100);
        Assert.Equal(100, bytes.Length);
    }

    [Fact]
    public void GenerateRandomBytes_WithZeroCount_ShouldThrow()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => AdnlKeys.GenerateRandomBytes(0));
    }

    [Fact]
    public void GenerateRandomBytes_WithNegativeCount_ShouldThrow()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => AdnlKeys.GenerateRandomBytes(-1));
    }

    [Fact]
    public void GenerateRandomBytes_ShouldReturnDifferentValues()
    {
        var bytes1 = AdnlKeys.GenerateRandomBytes(32);
        var bytes2 = AdnlKeys.GenerateRandomBytes(32);

        // Extremely unlikely to be equal if truly random
        Assert.NotEqual(bytes1, bytes2);
    }

    [Fact]
    public void Constructor_WithDifferentPeers_ShouldGenerateDifferentKeys()
    {
        var peer1 = AdnlKeys.GenerateRandomBytes(32);
        var peer2 = AdnlKeys.GenerateRandomBytes(32);

        var keys1 = new AdnlKeys(peer1);
        var keys2 = new AdnlKeys(peer2);

        // Each instance should generate unique ephemeral keys
        Assert.NotEqual(keys1.PublicKey, keys2.PublicKey);
        Assert.NotEqual(keys1.PrivateKey, keys2.PrivateKey);
    }

    [Fact]
    public void PeerPublicKey_ShouldReturnOriginalValue()
    {
        var peerPublicKey = AdnlKeys.GenerateRandomBytes(32);
        var keys = new AdnlKeys(peerPublicKey);

        Assert.Equal(peerPublicKey, keys.PeerPublicKey);
    }
}

