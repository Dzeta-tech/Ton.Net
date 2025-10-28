using Ton.Adnl.Crypto;
using Ton.Crypto.Primitives;

namespace Ton.Adnl.Tests.Crypto;

public class AdnlAesParamsTests
{
    [Fact]
    public void Constructor_ShouldGenerateRandomBytes()
    {
        AdnlAesParams params1 = new();
        AdnlAesParams params2 = new();

        Assert.NotEqual(params1.Bytes, params2.Bytes);
    }

    [Fact]
    public void Bytes_ShouldBe160Bytes()
    {
        AdnlAesParams aesParams = new();
        Assert.Equal(160, aesParams.Bytes.Length);
    }

    [Fact]
    public void Hash_ShouldBe32Bytes()
    {
        AdnlAesParams aesParams = new();
        Assert.Equal(32, aesParams.Hash.Length);
    }

    [Fact]
    public void TxKey_ShouldBe32Bytes()
    {
        AdnlAesParams aesParams = new();
        Assert.Equal(32, aesParams.TxKey.Length);
    }

    [Fact]
    public void TxKey_ShouldBeFirstBytesOfParams()
    {
        AdnlAesParams aesParams = new();
        // TxKey is now at bytes 32-64
        byte[] expected = aesParams.Bytes[32..64];
        Assert.Equal(expected, aesParams.TxKey);
    }

    [Fact]
    public void TxNonce_ShouldBe16Bytes()
    {
        AdnlAesParams aesParams = new();
        Assert.Equal(16, aesParams.TxNonce.Length);
    }

    [Fact]
    public void TxNonce_ShouldBeBytes32To48()
    {
        AdnlAesParams aesParams = new();
        // TxNonce is now at bytes 80-96
        byte[] expected = aesParams.Bytes[80..96];
        Assert.Equal(expected, aesParams.TxNonce);
    }

    [Fact]
    public void RxKey_ShouldBe32Bytes()
    {
        AdnlAesParams aesParams = new();
        Assert.Equal(32, aesParams.RxKey.Length);
    }

    [Fact]
    public void RxKey_ShouldBeBytes64To96()
    {
        AdnlAesParams aesParams = new();
        // RxKey is now at bytes 0-32
        byte[] expected = aesParams.Bytes[0..32];
        Assert.Equal(expected, aesParams.RxKey);
    }

    [Fact]
    public void RxNonce_ShouldBe16Bytes()
    {
        AdnlAesParams aesParams = new();
        Assert.Equal(16, aesParams.RxNonce.Length);
    }

    [Fact]
    public void RxNonce_ShouldBeBytes96To112()
    {
        AdnlAesParams aesParams = new();
        // RxNonce is now at bytes 64-80
        byte[] expected = aesParams.Bytes[64..80];
        Assert.Equal(expected, aesParams.RxNonce);
    }

    [Fact]
    public void TxAndRxKeys_ShouldBeDifferent()
    {
        AdnlAesParams aesParams = new();
        Assert.NotEqual(aesParams.TxKey, aesParams.RxKey);
    }

    [Fact]
    public void TxAndRxNonces_ShouldBeDifferent()
    {
        AdnlAesParams aesParams = new();
        Assert.NotEqual(aesParams.TxNonce, aesParams.RxNonce);
    }

    [Fact]
    public void Hash_ShouldBeSha256OfBytes()
    {
        AdnlAesParams aesParams = new();
        byte[] expectedHash = Sha256.Hash(aesParams.Bytes);
        Assert.Equal(expectedHash, aesParams.Hash);
    }

    [Fact]
    public void MultipleInstances_ShouldHaveDifferentValues()
    {
        AdnlAesParams params1 = new();
        AdnlAesParams params2 = new();
        AdnlAesParams params3 = new();

        Assert.NotEqual(params1.Bytes, params2.Bytes);
        Assert.NotEqual(params2.Bytes, params3.Bytes);
        Assert.NotEqual(params1.Bytes, params3.Bytes);
    }
}