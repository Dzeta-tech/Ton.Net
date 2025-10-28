using Ton.Adnl.Crypto;
using Xunit;

namespace Ton.Adnl.Tests.Crypto;

public class AdnlAesParamsTests
{
    [Fact]
    public void Constructor_ShouldGenerateRandomBytes()
    {
        var params1 = new AdnlAesParams();
        var params2 = new AdnlAesParams();

        Assert.NotEqual(params1.Bytes, params2.Bytes);
    }

    [Fact]
    public void Bytes_ShouldBe160Bytes()
    {
        var aesParams = new AdnlAesParams();
        Assert.Equal(160, aesParams.Bytes.Length);
    }

    [Fact]
    public void Hash_ShouldBe32Bytes()
    {
        var aesParams = new AdnlAesParams();
        Assert.Equal(32, aesParams.Hash.Length);
    }

    [Fact]
    public void TxKey_ShouldBe32Bytes()
    {
        var aesParams = new AdnlAesParams();
        Assert.Equal(32, aesParams.TxKey.Length);
    }

    [Fact]
    public void TxKey_ShouldBeFirstBytesOfParams()
    {
        var aesParams = new AdnlAesParams();
        var expected = aesParams.Bytes[0..32];
        Assert.Equal(expected, aesParams.TxKey);
    }

    [Fact]
    public void TxNonce_ShouldBe16Bytes()
    {
        var aesParams = new AdnlAesParams();
        Assert.Equal(16, aesParams.TxNonce.Length);
    }

    [Fact]
    public void TxNonce_ShouldBeBytes32To48()
    {
        var aesParams = new AdnlAesParams();
        var expected = aesParams.Bytes[32..48];
        Assert.Equal(expected, aesParams.TxNonce);
    }

    [Fact]
    public void RxKey_ShouldBe32Bytes()
    {
        var aesParams = new AdnlAesParams();
        Assert.Equal(32, aesParams.RxKey.Length);
    }

    [Fact]
    public void RxKey_ShouldBeBytes64To96()
    {
        var aesParams = new AdnlAesParams();
        var expected = aesParams.Bytes[64..96];
        Assert.Equal(expected, aesParams.RxKey);
    }

    [Fact]
    public void RxNonce_ShouldBe16Bytes()
    {
        var aesParams = new AdnlAesParams();
        Assert.Equal(16, aesParams.RxNonce.Length);
    }

    [Fact]
    public void RxNonce_ShouldBeBytes96To112()
    {
        var aesParams = new AdnlAesParams();
        var expected = aesParams.Bytes[96..112];
        Assert.Equal(expected, aesParams.RxNonce);
    }

    [Fact]
    public void TxAndRxKeys_ShouldBeDifferent()
    {
        var aesParams = new AdnlAesParams();
        Assert.NotEqual(aesParams.TxKey, aesParams.RxKey);
    }

    [Fact]
    public void TxAndRxNonces_ShouldBeDifferent()
    {
        var aesParams = new AdnlAesParams();
        Assert.NotEqual(aesParams.TxNonce, aesParams.RxNonce);
    }

    [Fact]
    public void Hash_ShouldBeSha256OfBytes()
    {
        var aesParams = new AdnlAesParams();
        var expectedHash = Ton.Crypto.Primitives.Sha256.Hash(aesParams.Bytes);
        Assert.Equal(expectedHash, aesParams.Hash);
    }

    [Fact]
    public void MultipleInstances_ShouldHaveDifferentValues()
    {
        var params1 = new AdnlAesParams();
        var params2 = new AdnlAesParams();
        var params3 = new AdnlAesParams();

        Assert.NotEqual(params1.Bytes, params2.Bytes);
        Assert.NotEqual(params2.Bytes, params3.Bytes);
        Assert.NotEqual(params1.Bytes, params3.Bytes);
    }
}

