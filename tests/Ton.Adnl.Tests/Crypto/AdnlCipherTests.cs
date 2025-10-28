using Ton.Adnl.Crypto;
using Xunit;

namespace Ton.Adnl.Tests.Crypto;

public class AdnlCipherTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldSucceed()
    {
        var key = AdnlKeys.GenerateRandomBytes(32);
        var counter = AdnlKeys.GenerateRandomBytes(16);

        using var cipher = new AdnlCipher(key, counter);
        Assert.NotNull(cipher);
    }

    [Fact]
    public void Constructor_WithNullKey_ShouldThrow()
    {
        var counter = AdnlKeys.GenerateRandomBytes(16);
        Assert.Throws<ArgumentNullException>(() => new AdnlCipher(null!, counter));
    }

    [Fact]
    public void Constructor_WithInvalidKeyLength_ShouldThrow()
    {
        var counter = AdnlKeys.GenerateRandomBytes(16);
        Assert.Throws<ArgumentException>(() => new AdnlCipher(new byte[16], counter));
        Assert.Throws<ArgumentException>(() => new AdnlCipher(new byte[64], counter));
    }

    [Fact]
    public void Constructor_WithNullCounter_ShouldThrow()
    {
        var key = AdnlKeys.GenerateRandomBytes(32);
        Assert.Throws<ArgumentNullException>(() => new AdnlCipher(key, null!));
    }

    [Fact]
    public void Constructor_WithInvalidCounterLength_ShouldThrow()
    {
        var key = AdnlKeys.GenerateRandomBytes(32);
        Assert.Throws<ArgumentException>(() => new AdnlCipher(key, new byte[8]));
        Assert.Throws<ArgumentException>(() => new AdnlCipher(key, new byte[32]));
    }

    [Fact]
    public void Process_WithValidData_ShouldEncrypt()
    {
        var key = AdnlKeys.GenerateRandomBytes(32);
        var counter = AdnlKeys.GenerateRandomBytes(16);
        var plaintext = "Hello, ADNL!"u8.ToArray();

        using var cipher = new AdnlCipher(key, counter);
        var ciphertext = cipher.Process(plaintext);

        Assert.Equal(plaintext.Length, ciphertext.Length);
        Assert.NotEqual(plaintext, ciphertext);
    }

    [Fact]
    public void Process_EncryptDecrypt_ShouldRoundTrip()
    {
        var key = AdnlKeys.GenerateRandomBytes(32);
        var counter = AdnlKeys.GenerateRandomBytes(16);
        var plaintext = "Hello, ADNL!"u8.ToArray();

        using var encryptCipher = new AdnlCipher(key, counter);
        var ciphertext = encryptCipher.Process(plaintext);

        using var decryptCipher = new AdnlCipher(key, counter);
        var decrypted = decryptCipher.Process(ciphertext);

        Assert.Equal(plaintext, decrypted);
    }

    [Fact]
    public void Process_WithEmptyData_ShouldSucceed()
    {
        var key = AdnlKeys.GenerateRandomBytes(32);
        var counter = AdnlKeys.GenerateRandomBytes(16);

        using var cipher = new AdnlCipher(key, counter);
        var result = cipher.Process(Array.Empty<byte>());

        Assert.Empty(result);
    }

    [Fact]
    public void Process_WithNullData_ShouldThrow()
    {
        var key = AdnlKeys.GenerateRandomBytes(32);
        var counter = AdnlKeys.GenerateRandomBytes(16);

        using var cipher = new AdnlCipher(key, counter);
        Assert.Throws<ArgumentNullException>(() => cipher.Process(null!));
    }

    [Fact]
    public void Process_WithLargeData_ShouldHandleMultipleBlocks()
    {
        var key = AdnlKeys.GenerateRandomBytes(32);
        var counter = AdnlKeys.GenerateRandomBytes(16);
        var plaintext = AdnlKeys.GenerateRandomBytes(1024); // 64 blocks

        using var encryptCipher = new AdnlCipher(key, counter);
        var ciphertext = encryptCipher.Process(plaintext);

        using var decryptCipher = new AdnlCipher(key, counter);
        var decrypted = decryptCipher.Process(ciphertext);

        Assert.Equal(plaintext, decrypted);
    }

    [Fact]
    public void ProcessInPlace_ShouldModifyDataInPlace()
    {
        var key = AdnlKeys.GenerateRandomBytes(32);
        var counter = AdnlKeys.GenerateRandomBytes(16);
        var data = "Hello, ADNL!"u8.ToArray();
        var original = (byte[])data.Clone();

        using var cipher = new AdnlCipher(key, counter);
        cipher.ProcessInPlace(data);

        // Data should be modified
        Assert.NotEqual(original, data);
    }

    [Fact]
    public void ProcessInPlace_EncryptDecrypt_ShouldRoundTrip()
    {
        var key = AdnlKeys.GenerateRandomBytes(32);
        var counter = AdnlKeys.GenerateRandomBytes(16);
        var data = "Hello, ADNL!"u8.ToArray();
        var original = (byte[])data.Clone();

        using var encryptCipher = new AdnlCipher(key, counter);
        encryptCipher.ProcessInPlace(data);

        using var decryptCipher = new AdnlCipher(key, counter);
        decryptCipher.ProcessInPlace(data);

        Assert.Equal(original, data);
    }

    [Fact]
    public void Process_WithDifferentKeys_ShouldProduceDifferentCiphertext()
    {
        var key1 = AdnlKeys.GenerateRandomBytes(32);
        var key2 = AdnlKeys.GenerateRandomBytes(32);
        var counter = AdnlKeys.GenerateRandomBytes(16);
        var plaintext = "Hello, ADNL!"u8.ToArray();

        using var cipher1 = new AdnlCipher(key1, counter);
        var ciphertext1 = cipher1.Process(plaintext);

        using var cipher2 = new AdnlCipher(key2, counter);
        var ciphertext2 = cipher2.Process(plaintext);

        Assert.NotEqual(ciphertext1, ciphertext2);
    }

    [Fact]
    public void Process_WithDifferentCounters_ShouldProduceDifferentCiphertext()
    {
        var key = AdnlKeys.GenerateRandomBytes(32);
        var counter1 = AdnlKeys.GenerateRandomBytes(16);
        var counter2 = AdnlKeys.GenerateRandomBytes(16);
        var plaintext = "Hello, ADNL!"u8.ToArray();

        using var cipher1 = new AdnlCipher(key, counter1);
        var ciphertext1 = cipher1.Process(plaintext);

        using var cipher2 = new AdnlCipher(key, counter2);
        var ciphertext2 = cipher2.Process(plaintext);

        Assert.NotEqual(ciphertext1, ciphertext2);
    }

    [Fact]
    public void Process_MultipleCallsSameCipher_ShouldContinueKeyStream()
    {
        var key = AdnlKeys.GenerateRandomBytes(32);
        var counter = AdnlKeys.GenerateRandomBytes(16);
        var plaintext1 = "Hello"u8.ToArray();
        var plaintext2 = ", ADNL!"u8.ToArray();

        using var cipher = new AdnlCipher(key, counter);
        var ciphertext1 = cipher.Process(plaintext1);
        var ciphertext2 = cipher.Process(plaintext2);

        // Decrypt separately
        using var decryptCipher = new AdnlCipher(key, counter);
        var decrypted1 = decryptCipher.Process(ciphertext1);
        var decrypted2 = decryptCipher.Process(ciphertext2);

        Assert.Equal(plaintext1, decrypted1);
        Assert.Equal(plaintext2, decrypted2);
    }

    [Fact]
    public void Dispose_ShouldPreventFurtherUse()
    {
        var key = AdnlKeys.GenerateRandomBytes(32);
        var counter = AdnlKeys.GenerateRandomBytes(16);
        var plaintext = "Hello, ADNL!"u8.ToArray();

        var cipher = new AdnlCipher(key, counter);
        cipher.Dispose();

        Assert.Throws<ObjectDisposedException>(() => cipher.Process(plaintext));
    }

    [Fact]
    public void CipherFactory_CreateCipher_ShouldCreateValidCipher()
    {
        var key = AdnlKeys.GenerateRandomBytes(32);
        var iv = AdnlKeys.GenerateRandomBytes(16);

        using var cipher = AdnlCipherFactory.CreateCipher(key, iv);
        Assert.NotNull(cipher);
    }

    [Fact]
    public void CipherFactory_CreateDecipher_ShouldCreateValidCipher()
    {
        var key = AdnlKeys.GenerateRandomBytes(32);
        var iv = AdnlKeys.GenerateRandomBytes(16);

        using var cipher = AdnlCipherFactory.CreateDecipher(key, iv);
        Assert.NotNull(cipher);
    }
}

