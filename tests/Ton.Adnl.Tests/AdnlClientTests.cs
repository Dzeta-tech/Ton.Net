using Ton.Adnl.Crypto;

namespace Ton.Adnl.Tests;

public class AdnlClientTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldSucceed()
    {
        byte[] publicKey = AdnlKeys.GenerateRandomBytes(32);
        AdnlClient client = new("127.0.0.1", 12345, publicKey);

        Assert.NotNull(client);
        Assert.Equal(AdnlClientState.Closed, client.State);
    }

    [Fact]
    public void Constructor_WithNullHost_ShouldThrow()
    {
        byte[] publicKey = AdnlKeys.GenerateRandomBytes(32);
        Assert.Throws<ArgumentException>(() => new AdnlClient(null!, 12345, publicKey));
    }

    [Fact]
    public void Constructor_WithEmptyHost_ShouldThrow()
    {
        byte[] publicKey = AdnlKeys.GenerateRandomBytes(32);
        Assert.Throws<ArgumentException>(() => new AdnlClient("", 12345, publicKey));
    }

    [Fact]
    public void Constructor_WithInvalidPort_ShouldThrow()
    {
        byte[] publicKey = AdnlKeys.GenerateRandomBytes(32);
        Assert.Throws<ArgumentOutOfRangeException>(() => new AdnlClient("127.0.0.1", 0, publicKey));
        Assert.Throws<ArgumentOutOfRangeException>(() => new AdnlClient("127.0.0.1", -1, publicKey));
        Assert.Throws<ArgumentOutOfRangeException>(() => new AdnlClient("127.0.0.1", 65536, publicKey));
    }

    [Fact]
    public void Constructor_WithNullPublicKey_ShouldThrow()
    {
        Assert.Throws<ArgumentNullException>(() => new AdnlClient("127.0.0.1", 12345, null!));
    }

    [Fact]
    public void Constructor_WithInvalidPublicKeyLength_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() => new AdnlClient("127.0.0.1", 12345, new byte[31]));
        Assert.Throws<ArgumentException>(() => new AdnlClient("127.0.0.1", 12345, new byte[33]));
    }

    [Fact]
    public void Constructor_WithNegativeReconnectTimeout_ShouldThrow()
    {
        byte[] publicKey = AdnlKeys.GenerateRandomBytes(32);
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new AdnlClient("127.0.0.1", 12345, publicKey, -1));
    }

    [Fact]
    public void State_InitialState_ShouldBeClosed()
    {
        byte[] publicKey = AdnlKeys.GenerateRandomBytes(32);
        AdnlClient client = new("127.0.0.1", 12345, publicKey);

        Assert.Equal(AdnlClientState.Closed, client.State);
    }

    [Fact]
    public async Task WriteAsync_WhenNotConnected_ShouldThrow()
    {
        byte[] publicKey = AdnlKeys.GenerateRandomBytes(32);
        AdnlClient client = new("127.0.0.1", 12345, publicKey);

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await client.WriteAsync([1, 2, 3]);
        });
    }

    [Fact]
    public async Task WriteAsync_WithNullData_ShouldThrow()
    {
        byte[] publicKey = AdnlKeys.GenerateRandomBytes(32);
        AdnlClient client = new("127.0.0.1", 12345, publicKey);

        await Assert.ThrowsAsync<ArgumentNullException>(async () => { await client.WriteAsync(null!); });
    }

    [Fact]
    public void Dispose_ShouldCloseConnection()
    {
        byte[] publicKey = AdnlKeys.GenerateRandomBytes(32);
        AdnlClient client = new("127.0.0.1", 12345, publicKey);

        client.Dispose();

        // Should not throw
        client.Dispose(); // Double dispose
    }

    [Fact]
    public async Task Dispose_ShouldPreventFurtherOperations()
    {
        byte[] publicKey = AdnlKeys.GenerateRandomBytes(32);
        AdnlClient client = new("127.0.0.1", 12345, publicKey);

        client.Dispose();

        await Assert.ThrowsAsync<ObjectDisposedException>(async () => { await client.ConnectAsync(); });
    }

    [Fact]
    public void Events_ShouldBeRaisable()
    {
        byte[] publicKey = AdnlKeys.GenerateRandomBytes(32);
        AdnlClient client = new("127.0.0.1", 12345, publicKey);

        bool connectedRaised = false;
        bool readyRaised = false;
        bool closedRaised = false;
        bool dataReceivedRaised = false;
        bool errorRaised = false;

        client.Connected += () => connectedRaised = true;
        client.Ready += () => readyRaised = true;
        client.Closed += () => closedRaised = true;
        client.DataReceived += _ => dataReceivedRaised = true;
        client.Error += _ => errorRaised = true;

        // Verify we can attach event handlers without exceptions
        Assert.False(connectedRaised);
        Assert.False(readyRaised);
        Assert.False(closedRaised);
        Assert.False(dataReceivedRaised);
        Assert.False(errorRaised);
    }
}