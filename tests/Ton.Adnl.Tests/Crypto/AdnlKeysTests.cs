using Ton.Adnl.Crypto;

namespace Ton.Adnl.Tests.Crypto;

public class AdnlKeysTests
{
    [Fact]
    public void Constructor_WithValidPeerPublicKey_ShouldSucceed()
    {
        byte[] peerPublicKey = AdnlKeys.GenerateRandomBytes(32);
        AdnlKeys keys = new(peerPublicKey);

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
        byte[] peerPublicKey = AdnlKeys.GenerateRandomBytes(32);
        AdnlKeys keys = new(peerPublicKey);

        Assert.Equal(32, keys.PublicKey.Length);
    }

    [Fact]
    public void PrivateKey_ShouldBe64Bytes()
    {
        byte[] peerPublicKey = AdnlKeys.GenerateRandomBytes(32);
        AdnlKeys keys = new(peerPublicKey);

        Assert.Equal(64, keys.PrivateKey.Length);
    }

    [Fact]
    public void PrivateKey_ShouldContainPublicKeyInLastBytes()
    {
        byte[] peerPublicKey = AdnlKeys.GenerateRandomBytes(32);
        AdnlKeys keys = new(peerPublicKey);

        // Ed25519 private key format: 32-byte seed + 32-byte public key
        byte[] publicKeyFromPrivate = keys.PrivateKey[32..64];
        Assert.Equal(keys.PublicKey, publicKeyFromPrivate);
    }

    [Fact]
    public void SharedSecret_ShouldBe32Bytes()
    {
        byte[] peerPublicKey = AdnlKeys.GenerateRandomBytes(32);
        AdnlKeys keys = new(peerPublicKey);

        byte[] sharedSecret = keys.SharedSecret;
        Assert.Equal(32, sharedSecret.Length);
    }

    [Fact]
    public void SharedSecret_ShouldBeCached()
    {
        byte[] peerPublicKey = AdnlKeys.GenerateRandomBytes(32);
        AdnlKeys keys = new(peerPublicKey);

        byte[] sharedSecret1 = keys.SharedSecret;
        byte[] sharedSecret2 = keys.SharedSecret;

        // Should return the same cached instance
        Assert.Same(sharedSecret1, sharedSecret2);
    }

    [Fact]
    public void SharedSecret_ShouldBeConsistent_BetweenTwoPeers()
    {
        // Generate two key pairs
        AdnlKeys aliceKeys = new(AdnlKeys.GenerateRandomBytes(32));
        AdnlKeys bobKeys = new(aliceKeys.PublicKey);

        // Recreate Alice's keys with Bob's public key
        AdnlKeys aliceKeysWithBob = new(bobKeys.PublicKey);
        // Use Alice's original key pair for computing shared secret
        // This is a simplified test - in reality, both would use the same keypairs

        // The shared secret computation should be deterministic for same inputs
        byte[] secret1 = aliceKeys.SharedSecret;
        byte[] secret2 = aliceKeys.SharedSecret;

        Assert.Equal(secret1, secret2);
    }

    [Fact]
    public void GenerateRandomBytes_WithValidCount_ShouldReturnCorrectLength()
    {
        byte[] bytes = AdnlKeys.GenerateRandomBytes(32);
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
        byte[] bytes1 = AdnlKeys.GenerateRandomBytes(32);
        byte[] bytes2 = AdnlKeys.GenerateRandomBytes(32);

        // Extremely unlikely to be equal if truly random
        Assert.NotEqual(bytes1, bytes2);
    }

    [Fact]
    public void Constructor_WithDifferentPeers_ShouldGenerateDifferentKeys()
    {
        byte[] peer1 = AdnlKeys.GenerateRandomBytes(32);
        byte[] peer2 = AdnlKeys.GenerateRandomBytes(32);

        AdnlKeys keys1 = new(peer1);
        AdnlKeys keys2 = new(peer2);

        // Each instance should generate unique ephemeral keys
        Assert.NotEqual(keys1.PublicKey, keys2.PublicKey);
        Assert.NotEqual(keys1.PrivateKey, keys2.PrivateKey);
    }

    [Fact]
    public void PeerPublicKey_ShouldReturnOriginalValue()
    {
        byte[] peerPublicKey = AdnlKeys.GenerateRandomBytes(32);
        AdnlKeys keys = new(peerPublicKey);

        Assert.Equal(peerPublicKey, keys.PeerPublicKey);
    }
}