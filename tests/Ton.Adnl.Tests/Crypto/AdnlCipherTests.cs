using Ton.Adnl.Crypto;

namespace Ton.Adnl.Tests.Crypto;

public class AdnlCipherTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldSucceed()
    {
        byte[] key = AdnlKeys.GenerateRandomBytes(32);
        byte[] counter = AdnlKeys.GenerateRandomBytes(16);

        using AdnlCipher cipher = new(key, counter);
        Assert.NotNull(cipher);
    }

    [Fact]
    public void Constructor_WithNullKey_ShouldThrow()
    {
        byte[] counter = AdnlKeys.GenerateRandomBytes(16);
        Assert.Throws<ArgumentNullException>(() => new AdnlCipher(null!, counter));
    }

    [Fact]
    public void Constructor_WithInvalidKeyLength_ShouldThrow()
    {
        byte[] counter = AdnlKeys.GenerateRandomBytes(16);
        Assert.Throws<ArgumentException>(() => new AdnlCipher(new byte[16], counter));
        Assert.Throws<ArgumentException>(() => new AdnlCipher(new byte[64], counter));
    }

    [Fact]
    public void Constructor_WithNullCounter_ShouldThrow()
    {
        byte[] key = AdnlKeys.GenerateRandomBytes(32);
        Assert.Throws<ArgumentNullException>(() => new AdnlCipher(key, null!));
    }

    [Fact]
    public void Constructor_WithInvalidCounterLength_ShouldThrow()
    {
        byte[] key = AdnlKeys.GenerateRandomBytes(32);
        Assert.Throws<ArgumentException>(() => new AdnlCipher(key, new byte[8]));
        Assert.Throws<ArgumentException>(() => new AdnlCipher(key, new byte[32]));
    }

    [Fact]
    public void Process_WithValidData_ShouldEncrypt()
    {
        byte[] key = AdnlKeys.GenerateRandomBytes(32);
        byte[] counter = AdnlKeys.GenerateRandomBytes(16);
        byte[] plaintext = "Hello, ADNL!"u8.ToArray();

        using AdnlCipher cipher = new(key, counter);
        byte[] ciphertext = cipher.Process(plaintext);

        Assert.Equal(plaintext.Length, ciphertext.Length);
        Assert.NotEqual(plaintext, ciphertext);
    }

    [Fact]
    public void Process_EncryptDecrypt_ShouldRoundTrip()
    {
        byte[] key = AdnlKeys.GenerateRandomBytes(32);
        byte[] counter = AdnlKeys.GenerateRandomBytes(16);
        byte[] plaintext = "Hello, ADNL!"u8.ToArray();

        using AdnlCipher encryptCipher = new(key, counter);
        byte[] ciphertext = encryptCipher.Process(plaintext);

        using AdnlCipher decryptCipher = new(key, counter);
        byte[] decrypted = decryptCipher.Process(ciphertext);

        Assert.Equal(plaintext, decrypted);
    }

    [Fact]
    public void Process_WithEmptyData_ShouldSucceed()
    {
        byte[] key = AdnlKeys.GenerateRandomBytes(32);
        byte[] counter = AdnlKeys.GenerateRandomBytes(16);

        using AdnlCipher cipher = new(key, counter);
        byte[] result = cipher.Process([]);

        Assert.Empty(result);
    }

    [Fact]
    public void Process_WithNullData_ShouldThrow()
    {
        byte[] key = AdnlKeys.GenerateRandomBytes(32);
        byte[] counter = AdnlKeys.GenerateRandomBytes(16);

        using AdnlCipher cipher = new(key, counter);
        Assert.Throws<ArgumentNullException>(() => cipher.Process(null!));
    }

    [Fact]
    public void Process_WithLargeData_ShouldHandleMultipleBlocks()
    {
        byte[] key = AdnlKeys.GenerateRandomBytes(32);
        byte[] counter = AdnlKeys.GenerateRandomBytes(16);
        byte[] plaintext = AdnlKeys.GenerateRandomBytes(1024); // 64 blocks

        using AdnlCipher encryptCipher = new(key, counter);
        byte[] ciphertext = encryptCipher.Process(plaintext);

        using AdnlCipher decryptCipher = new(key, counter);
        byte[] decrypted = decryptCipher.Process(ciphertext);

        Assert.Equal(plaintext, decrypted);
    }

    [Fact]
    public void ProcessInPlace_ShouldModifyDataInPlace()
    {
        byte[] key = AdnlKeys.GenerateRandomBytes(32);
        byte[] counter = AdnlKeys.GenerateRandomBytes(16);
        byte[] data = "Hello, ADNL!"u8.ToArray();
        byte[] original = (byte[])data.Clone();

        using AdnlCipher cipher = new(key, counter);
        cipher.ProcessInPlace(data);

        // Data should be modified
        Assert.NotEqual(original, data);
    }

    [Fact]
    public void ProcessInPlace_EncryptDecrypt_ShouldRoundTrip()
    {
        byte[] key = AdnlKeys.GenerateRandomBytes(32);
        byte[] counter = AdnlKeys.GenerateRandomBytes(16);
        byte[] data = "Hello, ADNL!"u8.ToArray();
        byte[] original = (byte[])data.Clone();

        using AdnlCipher encryptCipher = new(key, counter);
        encryptCipher.ProcessInPlace(data);

        using AdnlCipher decryptCipher = new(key, counter);
        decryptCipher.ProcessInPlace(data);

        Assert.Equal(original, data);
    }

    [Fact]
    public void Process_WithDifferentKeys_ShouldProduceDifferentCiphertext()
    {
        byte[] key1 = AdnlKeys.GenerateRandomBytes(32);
        byte[] key2 = AdnlKeys.GenerateRandomBytes(32);
        byte[] counter = AdnlKeys.GenerateRandomBytes(16);
        byte[] plaintext = "Hello, ADNL!"u8.ToArray();

        using AdnlCipher cipher1 = new(key1, counter);
        byte[] ciphertext1 = cipher1.Process(plaintext);

        using AdnlCipher cipher2 = new(key2, counter);
        byte[] ciphertext2 = cipher2.Process(plaintext);

        Assert.NotEqual(ciphertext1, ciphertext2);
    }

    [Fact]
    public void Process_WithDifferentCounters_ShouldProduceDifferentCiphertext()
    {
        byte[] key = AdnlKeys.GenerateRandomBytes(32);
        byte[] counter1 = AdnlKeys.GenerateRandomBytes(16);
        byte[] counter2 = AdnlKeys.GenerateRandomBytes(16);
        byte[] plaintext = "Hello, ADNL!"u8.ToArray();

        using AdnlCipher cipher1 = new(key, counter1);
        byte[] ciphertext1 = cipher1.Process(plaintext);

        using AdnlCipher cipher2 = new(key, counter2);
        byte[] ciphertext2 = cipher2.Process(plaintext);

        Assert.NotEqual(ciphertext1, ciphertext2);
    }

    [Fact]
    public void Process_MultipleCallsSameCipher_ShouldContinueKeyStream()
    {
        byte[] key = AdnlKeys.GenerateRandomBytes(32);
        byte[] counter = AdnlKeys.GenerateRandomBytes(16);
        byte[] plaintext1 = "Hello"u8.ToArray();
        byte[] plaintext2 = ", ADNL!"u8.ToArray();

        using AdnlCipher cipher = new(key, counter);
        byte[] ciphertext1 = cipher.Process(plaintext1);
        byte[] ciphertext2 = cipher.Process(plaintext2);

        // Decrypt separately
        using AdnlCipher decryptCipher = new(key, counter);
        byte[] decrypted1 = decryptCipher.Process(ciphertext1);
        byte[] decrypted2 = decryptCipher.Process(ciphertext2);

        Assert.Equal(plaintext1, decrypted1);
        Assert.Equal(plaintext2, decrypted2);
    }

    [Fact]
    public void Dispose_ShouldPreventFurtherUse()
    {
        byte[] key = AdnlKeys.GenerateRandomBytes(32);
        byte[] counter = AdnlKeys.GenerateRandomBytes(16);
        byte[] plaintext = "Hello, ADNL!"u8.ToArray();

        AdnlCipher cipher = new(key, counter);
        cipher.Dispose();

        Assert.Throws<ObjectDisposedException>(() => cipher.Process(plaintext));
    }

    [Fact]
    public void CipherFactory_CreateCipher_ShouldCreateValidCipher()
    {
        byte[] key = AdnlKeys.GenerateRandomBytes(32);
        byte[] iv = AdnlKeys.GenerateRandomBytes(16);

        using AdnlCipher cipher = AdnlCipherFactory.CreateCipher(key, iv);
        Assert.NotNull(cipher);
    }

    [Fact]
    public void CipherFactory_CreateDecipher_ShouldCreateValidCipher()
    {
        byte[] key = AdnlKeys.GenerateRandomBytes(32);
        byte[] iv = AdnlKeys.GenerateRandomBytes(16);

        using AdnlCipher cipher = AdnlCipherFactory.CreateDecipher(key, iv);
        Assert.NotNull(cipher);
    }
}