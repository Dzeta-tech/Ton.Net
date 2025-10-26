using System.Text;
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

        KeyPair keyPair = Ed25519.Ed25519.KeyPairFromSeed(seed);

        Assert.Multiple(() =>
        {
            Assert.That(keyPair.PublicKey, Has.Length.EqualTo(32));
            Assert.That(keyPair.SecretKey, Has.Length.EqualTo(64));
        });

        // Secret key should contain seed + public key
        for (int i = 0; i < 32; i++)
            Assert.Multiple(() =>
            {
                Assert.That(keyPair.SecretKey[i], Is.EqualTo(seed[i]));
                Assert.That(keyPair.SecretKey[32 + i], Is.EqualTo(keyPair.PublicKey[i]));
            });
    }

    [Test]
    public void Test_KeyPairFromSecretKey()
    {
        // Generate a test seed and create keypair
        byte[] seed = new byte[32];
        for (int i = 0; i < 32; i++)
            seed[i] = (byte)i;

        KeyPair originalKeyPair = Ed25519.Ed25519.KeyPairFromSeed(seed);

        // Now recreate from secret key
        KeyPair recreatedKeyPair = Ed25519.Ed25519.KeyPairFromSecretKey(originalKeyPair.SecretKey);

        Assert.Multiple(() =>
        {
            Assert.That(recreatedKeyPair.PublicKey, Is.EqualTo(originalKeyPair.PublicKey));
            Assert.That(recreatedKeyPair.SecretKey, Is.EqualTo(originalKeyPair.SecretKey));
        });
    }

    [Test]
    public void Test_SignAndVerify()
    {
        // Generate keypair
        byte[] seed = new byte[32];
        for (int i = 0; i < 32; i++)
            seed[i] = (byte)i;

        KeyPair keyPair = Ed25519.Ed25519.KeyPairFromSeed(seed);

        // Sign some data
        byte[] data = "Hello, TON!"u8.ToArray();
        byte[] signature = Ed25519.Ed25519.Sign(data, keyPair.SecretKey);

        Assert.That(signature, Has.Length.EqualTo(64));

        // Verify signature
        bool isValid = Ed25519.Ed25519.SignVerify(data, signature, keyPair.PublicKey);
        Assert.That(isValid, Is.True);

        // Verify with wrong data should fail
        byte[] wrongData = "Wrong data"u8.ToArray();
        bool isInvalid = Ed25519.Ed25519.SignVerify(wrongData, signature, keyPair.PublicKey);
        Assert.That(isInvalid, Is.False);
    }

    [Test]
    public void Test_SignAndVerify_MultipleMessages()
    {
        // Generate keypair
        byte[] seed = Convert.FromHexString("0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef");
        KeyPair keyPair = Ed25519.Ed25519.KeyPairFromSeed(seed);

        string[] messages = ["Test 1", "Test 2", "Test 3", "A longer test message!"];

        foreach (string message in messages)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            byte[] signature = Ed25519.Ed25519.Sign(data, keyPair.SecretKey);

            bool isValid = Ed25519.Ed25519.SignVerify(data, signature, keyPair.PublicKey);
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

        KeyPair keyPair1 = Ed25519.Ed25519.KeyPairFromSeed(seed1);
        KeyPair keyPair2 = Ed25519.Ed25519.KeyPairFromSeed(seed2);

        byte[] data = "Test data"u8.ToArray();
        byte[] signature1 = Ed25519.Ed25519.Sign(data, keyPair1.SecretKey);
        byte[] signature2 = Ed25519.Ed25519.Sign(data, keyPair2.SecretKey);

        Assert.That(signature1, Is.Not.EqualTo(signature2));
    }

    [Test]
    public void Test_InvalidSeedLength()
    {
        byte[] invalidSeed = new byte[16]; // Too short
        Assert.Throws<ArgumentException>(() => Ed25519.Ed25519.KeyPairFromSeed(invalidSeed));
    }

    [Test]
    public void Test_InvalidSecretKeyLength()
    {
        byte[] invalidKey = new byte[32]; // Too short
        Assert.Throws<ArgumentException>(() => Ed25519.Ed25519.KeyPairFromSecretKey(invalidKey));
    }
}