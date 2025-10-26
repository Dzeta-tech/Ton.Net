using Ton.Crypto.Ed25519;

namespace Ton.Crypto.Tests;

[TestFixture]
public class SecretBoxTests
{
    [Test]
    public void Test_SealAndOpen()
    {
        byte[] key = new byte[32];
        byte[] nonce = new byte[24];
        
        for (int i = 0; i < 32; i++)
            key[i] = (byte)i;
        for (int i = 0; i < 24; i++)
            nonce[i] = (byte)(i * 2);

        byte[] data = System.Text.Encoding.UTF8.GetBytes("Hello, secret world!");
        
        byte[] encrypted = SecretBox.Seal(data, nonce, key);
        Assert.That(encrypted, Is.Not.Null);
        Assert.That(encrypted.Length, Is.GreaterThan(data.Length));
        
        byte[] decrypted = SecretBox.Open(encrypted, nonce, key);
        Assert.That(decrypted, Is.Not.Null);
        Assert.That(decrypted, Is.EqualTo(data));
    }

    [Test]
    public void Test_OpenWithWrongKey()
    {
        byte[] key = new byte[32];
        byte[] wrongKey = new byte[32];
        byte[] nonce = new byte[24];
        
        for (int i = 0; i < 32; i++)
        {
            key[i] = (byte)i;
            wrongKey[i] = (byte)(i + 1);
        }

        byte[] data = System.Text.Encoding.UTF8.GetBytes("Secret message");
        byte[] encrypted = SecretBox.Seal(data, nonce, key);
        
        byte[] decrypted = SecretBox.Open(encrypted, nonce, wrongKey);
        Assert.That(decrypted, Is.Null);
    }

    [Test]
    public void Test_OpenWithWrongNonce()
    {
        byte[] key = new byte[32];
        byte[] nonce = new byte[24];
        byte[] wrongNonce = new byte[24];
        
        for (int i = 0; i < 24; i++)
        {
            nonce[i] = (byte)i;
            wrongNonce[i] = (byte)(i + 1);
        }

        byte[] data = System.Text.Encoding.UTF8.GetBytes("Secret message");
        byte[] encrypted = SecretBox.Seal(data, nonce, key);
        
        byte[] decrypted = SecretBox.Open(encrypted, wrongNonce, key);
        Assert.That(decrypted, Is.Null);
    }

    [Test]
    public void Test_TamperedDataFails()
    {
        byte[] key = new byte[32];
        byte[] nonce = new byte[24];

        byte[] data = System.Text.Encoding.UTF8.GetBytes("Secret message");
        byte[] encrypted = SecretBox.Seal(data, nonce, key);
        
        encrypted[5] ^= 1;
        
        byte[] decrypted = SecretBox.Open(encrypted, nonce, key);
        Assert.That(decrypted, Is.Null);
    }

    [Test]
    public void Test_InvalidNonceLength()
    {
        byte[] key = new byte[32];
        byte[] badNonce = new byte[16];
        byte[] data = new byte[] { 1, 2, 3 };
        
        Assert.Throws<ArgumentException>(() => SecretBox.Seal(data, badNonce, key));
    }

    [Test]
    public void Test_InvalidKeyLength()
    {
        byte[] badKey = new byte[16];
        byte[] nonce = new byte[24];
        byte[] data = new byte[] { 1, 2, 3 };
        
        Assert.Throws<ArgumentException>(() => SecretBox.Seal(data, nonce, badKey));
    }
}
