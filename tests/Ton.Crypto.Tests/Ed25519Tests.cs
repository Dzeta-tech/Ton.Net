using Ton.Crypto.Ed25519;

namespace Ton.Crypto.Tests;

[TestFixture]
public class Ed25519Tests
{
    [Test]
    public void Test_KeyPairFromSeed()
    {
        // Generate a test seed
        byte[] seed = new byte[32];
        for (int i = 0; i < 32; i++)
            seed[i] = (byte)i;

        var keyPair = Ton.Crypto.Ed25519.Ed25519.KeyPairFromSeed(seed);
        
        Assert.That(keyPair.PublicKey.Length, Is.EqualTo(32));
        Assert.That(keyPair.SecretKey.Length, Is.EqualTo(64));
        
        // Secret key should contain seed + public key
        for (int i = 0; i < 32; i++)
        {
            Assert.That(keyPair.SecretKey[i], Is.EqualTo(seed[i]));
            Assert.That(keyPair.SecretKey[32 + i], Is.EqualTo(keyPair.PublicKey[i]));
        }
    }

    [Test]
    public void Test_KeyPairFromSecretKey()
    {
        // Generate a test seed and create keypair
        byte[] seed = new byte[32];
        for (int i = 0; i < 32; i++)
            seed[i] = (byte)i;

        var originalKeyPair = Ton.Crypto.Ed25519.Ed25519.KeyPairFromSeed(seed);
        
        // Now recreate from secret key
        var recreatedKeyPair = Ton.Crypto.Ed25519.Ed25519.KeyPairFromSecretKey(originalKeyPair.SecretKey);
        
        Assert.That(recreatedKeyPair.PublicKey, Is.EqualTo(originalKeyPair.PublicKey));
        Assert.That(recreatedKeyPair.SecretKey, Is.EqualTo(originalKeyPair.SecretKey));
    }

    [Test]
    public void Test_SignAndVerify()
    {
        // Generate keypair
        byte[] seed = new byte[32];
        for (int i = 0; i < 32; i++)
            seed[i] = (byte)i;

        var keyPair = Ton.Crypto.Ed25519.Ed25519.KeyPairFromSeed(seed);
        
        // Sign some data
        byte[] data = System.Text.Encoding.UTF8.GetBytes("Hello, TON!");
        byte[] signature = Ton.Crypto.Ed25519.Ed25519.Sign(data, keyPair.SecretKey);
        
        Assert.That(signature.Length, Is.EqualTo(64));
        
        // Verify signature
        bool isValid = Ton.Crypto.Ed25519.Ed25519.SignVerify(data, signature, keyPair.PublicKey);
        Assert.That(isValid, Is.True);
        
        // Verify with wrong data should fail
        byte[] wrongData = System.Text.Encoding.UTF8.GetBytes("Wrong data");
        bool isInvalid = Ton.Crypto.Ed25519.Ed25519.SignVerify(wrongData, signature, keyPair.PublicKey);
        Assert.That(isInvalid, Is.False);
    }

    [Test]
    public void Test_SignAndVerify_MultipleMessages()
    {
        // Generate keypair
        byte[] seed = Convert.FromHexString("0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef");
        var keyPair = Ton.Crypto.Ed25519.Ed25519.KeyPairFromSeed(seed);
        
        string[] messages = { "Test 1", "Test 2", "Test 3", "A longer test message!" };
        
        foreach (var message in messages)
        {
            byte[] data = System.Text.Encoding.UTF8.GetBytes(message);
            byte[] signature = Ton.Crypto.Ed25519.Ed25519.Sign(data, keyPair.SecretKey);
            
            bool isValid = Ton.Crypto.Ed25519.Ed25519.SignVerify(data, signature, keyPair.PublicKey);
            Assert.That(isValid, Is.True, $"Signature verification failed for message: {message}");
        }
    }

    [Test]
    public void Test_SignaturesDiffer()
    {
        // Two different keypairs should produce different signatures
        byte[] seed1 = new byte[32];
        byte[] seed2 = new byte[32];
        
        for (int i = 0; i < 32; i++)
        {
            seed1[i] = (byte)i;
            seed2[i] = (byte)(i + 1);
        }

        var keyPair1 = Ton.Crypto.Ed25519.Ed25519.KeyPairFromSeed(seed1);
        var keyPair2 = Ton.Crypto.Ed25519.Ed25519.KeyPairFromSeed(seed2);
        
        byte[] data = System.Text.Encoding.UTF8.GetBytes("Test data");
        byte[] signature1 = Ton.Crypto.Ed25519.Ed25519.Sign(data, keyPair1.SecretKey);
        byte[] signature2 = Ton.Crypto.Ed25519.Ed25519.Sign(data, keyPair2.SecretKey);
        
        Assert.That(signature1, Is.Not.EqualTo(signature2));
    }

    [Test]
    public void Test_InvalidSeedLength()
    {
        byte[] invalidSeed = new byte[16]; // Too short
        Assert.Throws<ArgumentException>(() => Ton.Crypto.Ed25519.Ed25519.KeyPairFromSeed(invalidSeed));
    }

    [Test]
    public void Test_InvalidSecretKeyLength()
    {
        byte[] invalidKey = new byte[32]; // Too short
        Assert.Throws<ArgumentException>(() => Ton.Crypto.Ed25519.Ed25519.KeyPairFromSecretKey(invalidKey));
    }
}

