using Ton.Core.Boc;
using Ton.LiteClient.Models;
using Xunit.Abstractions;

namespace Ton.LiteClient.Tests;

/// <summary>
///     Integration tests for LiteClient using public TON lite servers.
///     These tests require network access and may take longer to execute.
/// </summary>
[Trait("Category", "Integration")]
public class LiteClientIntegrationTests : IAsyncLifetime
{
    // TON mainnet global config URL
    const string MainnetConfigUrl = "https://ton.org/global-config.json";
    readonly ITestOutputHelper _output;
    LiteClient _client = null!;

    public LiteClientIntegrationTests(ITestOutputHelper output)
    {
        _output = output;
    }

    public async Task InitializeAsync()
    {
        // Create lite client from mainnet config - will use round-robin if multiple servers available
        // Connection happens automatically on first request
        _client = await LiteClientFactory.CreateFromUrlAsync(MainnetConfigUrl);
        _output.WriteLine("Created LiteClient from mainnet config");
    }

    public Task DisposeAsync()
    {
        _client?.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task GetTime_ShouldReturnCurrentTime()
    {
        // Act
        DateTimeOffset serverTime = await _client.GetTimeAsync();

        // Assert
        DateTimeOffset currentTime = DateTimeOffset.UtcNow;
        Assert.True(serverTime > DateTimeOffset.MinValue, "Server time should be valid");
        Assert.True(Math.Abs((serverTime - currentTime).TotalMinutes) < 5,
            "Server time should be within 5 minutes of current time");

        _output.WriteLine($"Server time: {serverTime:yyyy-MM-dd HH:mm:ss} UTC");
        _output.WriteLine($"Local time: {currentTime:yyyy-MM-dd HH:mm:ss} UTC");
        _output.WriteLine($"Difference: {(serverTime - currentTime).TotalSeconds:F1} seconds");
    }

    [Fact]
    public async Task GetVersion_ShouldReturnVersionInfo()
    {
        // Act
        (int Version, long Capabilities, int Now) version = await _client.GetVersionAsync();

        // Assert
        Assert.NotNull(version);
        Assert.True(version.Version > 0, "Version number should be positive");
        Assert.True(version.Capabilities >= 0, "Capabilities should be non-negative");

        _output.WriteLine($"LiteServer Version: {version.Version}");
        _output.WriteLine($"Capabilities: {version.Capabilities}");
        _output.WriteLine($"Now: {version.Now}");
    }

    [Fact]
    public async Task GetMasterchainInfo_ShouldReturnValidBlock()
    {
        // Act
        MasterchainInfo masterchainInfo = await _client.GetMasterchainInfoAsync();

        // Assert
        Assert.NotNull(masterchainInfo);
        Assert.NotNull(masterchainInfo.Last);
        Assert.Equal(-1, masterchainInfo.Last.Workchain);
        Assert.Equal(long.MinValue, masterchainInfo.Last.Shard);
        Assert.True(masterchainInfo.Last.Seqno > 0u, "Block seqno should be positive");
        Assert.NotNull(masterchainInfo.Last.RootHash);
        Assert.Equal(32, masterchainInfo.Last.RootHash.Length);
        Assert.NotNull(masterchainInfo.Last.FileHash);
        Assert.Equal(32, masterchainInfo.Last.FileHash.Length);

        Assert.NotNull(masterchainInfo.Init);
        Assert.Equal(-1, masterchainInfo.Init.Workchain);

        _output.WriteLine("Latest Masterchain Block:");
        _output.WriteLine($"  Seqno: {masterchainInfo.Last.Seqno}");
        _output.WriteLine($"  Workchain: {masterchainInfo.Last.Workchain}");
        _output.WriteLine($"  Shard: {masterchainInfo.Last.Shard:X16}");
        _output.WriteLine($"  Root Hash: {masterchainInfo.Last.RootHashHex}");
        _output.WriteLine($"  File Hash: {masterchainInfo.Last.FileHashHex}");
    }

    [Fact]
    public async Task GetAllShardsInfo_ShouldReturnWorkchainShards()
    {
        // Arrange
        MasterchainInfo masterchainInfo = await _client.GetMasterchainInfoAsync();

        // Act
        BlockId[] shards = await _client.GetAllShardsInfoAsync(masterchainInfo.Last);

        // Assert
        Assert.NotNull(shards);
        Assert.True(shards.Length > 0, "Should have at least one shard");

        // Validate that we have workchain 0 shards
        BlockId[] workchain0Shards = shards.Where(s => s.Workchain == 0).ToArray();
        Assert.True(workchain0Shards.Length > 0, "Should have at least one shard for workchain 0");

        // Validate each shard has proper data
        foreach (BlockId shard in shards)
        {
            Assert.True(shard.Seqno > 0u, $"Shard seqno should be positive for shard {shard.Shard:X16}");
            Assert.NotNull(shard.RootHash);
            Assert.Equal(32, shard.RootHash.Length);
            Assert.NotNull(shard.FileHash);
            Assert.Equal(32, shard.FileHash.Length);

            _output.WriteLine($"Shard: wc={shard.Workchain}, shard={shard.Shard:X16}, " +
                              $"seqno={shard.Seqno}, " +
                              $"rootHash={shard.RootHashHex.Substring(0, 16)}..., " +
                              $"fileHash={shard.FileHashHex.Substring(0, 16)}...");
        }

        _output.WriteLine($"Total shards: {shards.Length}");
        _output.WriteLine($"Workchain 0 shards: {workchain0Shards.Length}");
    }

    [Fact]
    public async Task LookupBlock_BySeqno_ShouldReturnBlockId()
    {
        // Arrange
        MasterchainInfo masterchainInfo = await _client.GetMasterchainInfoAsync();
        uint seqno = masterchainInfo.Last.Seqno - 10; // Look up a recent block

        // Act
        BlockId blockId = await _client.LookupBlockAsync(
            -1,
            long.MinValue,
            seqno
        );

        // Assert
        Assert.NotNull(blockId);
        Assert.Equal(-1, blockId.Workchain);
        Assert.Equal(long.MinValue, blockId.Shard);
        Assert.Equal(seqno, blockId.Seqno);
        Assert.NotNull(blockId.RootHash);
        Assert.Equal(32, blockId.RootHash.Length);
        Assert.NotNull(blockId.FileHash);
        Assert.Equal(32, blockId.FileHash.Length);

        _output.WriteLine($"Looked up block {seqno}:");
        _output.WriteLine($"  Root Hash: {blockId.RootHashHex}");
        _output.WriteLine($"  File Hash: {blockId.FileHashHex}");
    }

    [Fact]
    public async Task GetBlockHeader_ShouldReturnHeaderWithValidData()
    {
        // Arrange
        MasterchainInfo masterchainInfo = await _client.GetMasterchainInfoAsync();
        BlockId blockId = masterchainInfo.Last;

        // Act
        BlockHeader header = await _client.GetBlockHeaderAsync(blockId);

        // Assert
        Assert.NotNull(header);
        Assert.NotNull(header.Id);
        Assert.NotNull(header.HeaderProof);
        Assert.True(header.HeaderProof.Length > 0, "Header proof should not be empty");
        Assert.Equal(blockId.Seqno, header.Id.Seqno);
        Assert.Equal(blockId.Workchain, header.Id.Workchain);

        _output.WriteLine($"Block Header for seqno {blockId.Seqno}:");
        _output.WriteLine($"  Block ID: {header.Id}");
        _output.WriteLine($"  Mode: {header.Mode}");
        _output.WriteLine($"  Header Proof Size: {header.HeaderProof.Length} bytes");

        // Verify we can parse the proof as a cell
        Cell proofCell = Cell.FromBoc(header.HeaderProof)[0];
        Assert.NotNull(proofCell);
        _output.WriteLine($"  Proof Cell Type: {proofCell.Type}");

        // If it's a MerkleProof, we can unwrap it
        if (proofCell.IsExotic)
        {
            Cell unwrapped = proofCell.UnwrapProof();
            Assert.NotNull(unwrapped);
            _output.WriteLine($"  Unwrapped Cell Type: {unwrapped.Type}");
        }
    }

    [Fact]
    public async Task ListBlockTransactions_OnMasterchain_ShouldReturnTransactions()
    {
        // Arrange
        MasterchainInfo masterchainInfo = await _client.GetMasterchainInfoAsync();
        BlockId blockId = masterchainInfo.Last;

        // Act
        BlockTransactions blockTransactions = await _client.ListBlockTransactionsAsync(blockId, 20);

        // Assert
        Assert.NotNull(blockTransactions);
        Assert.NotNull(blockTransactions.BlockId);
        Assert.Equal(20u, blockTransactions.RequestedCount);
        Assert.NotNull(blockTransactions.Transactions);

        // Masterchain blocks might have few or no transactions, so we just validate the structure
        _output.WriteLine($"Masterchain Block Transactions (block {blockId.Seqno}):");
        _output.WriteLine($"  Total found: {blockTransactions.Transactions.Count}");
        _output.WriteLine($"  Incomplete: {blockTransactions.Incomplete}");

        foreach (BlockTransaction tx in blockTransactions.Transactions.Take(5))
        {
            Assert.NotNull(tx.Account);
            Assert.Equal(32, tx.Account.Length);
            Assert.True(tx.Lt > 0, "Logical time should be positive");
            Assert.NotNull(tx.Hash);
            Assert.Equal(32, tx.Hash.Length);

            _output.WriteLine($"    Tx: account={Convert.ToHexString(tx.Account).Substring(0, 16)}..., " +
                              $"lt={tx.Lt}, hash={Convert.ToHexString(tx.Hash).Substring(0, 16)}...");
        }
    }

    [Fact]
    public async Task ListBlockTransactions_OnWorkchainShard_ShouldReturnTransactions()
    {
        // Arrange
        MasterchainInfo masterchainInfo = await _client.GetMasterchainInfoAsync();
        BlockId[] shards = await _client.GetAllShardsInfoAsync(masterchainInfo.Last);

        // Find a workchain 0 shard
        BlockId? workchainShard = shards.FirstOrDefault(s => s.Workchain == 0);
        Assert.NotNull(workchainShard);

        // Act
        BlockTransactions blockTransactions = await _client.ListBlockTransactionsAsync(workchainShard);

        // Assert
        Assert.NotNull(blockTransactions);
        Assert.NotNull(blockTransactions.BlockId);
        Assert.NotNull(blockTransactions.Transactions);

        // Workchain shards typically have transactions
        if (blockTransactions.Transactions.Count > 0)
        {
            _output.WriteLine($"Workchain Shard Block Transactions (block {workchainShard.Seqno}):");
            _output.WriteLine($"  Shard: {workchainShard.Shard:X16}");
            _output.WriteLine($"  Total found: {blockTransactions.Transactions.Count}");
            _output.WriteLine($"  Incomplete: {blockTransactions.Incomplete}");

            foreach (BlockTransaction tx in blockTransactions.Transactions.Take(5))
            {
                Assert.NotNull(tx.Account);
                Assert.Equal(32, tx.Account.Length);
                Assert.True(tx.Lt > 0, "Logical time should be positive");
                Assert.NotNull(tx.Hash);
                Assert.Equal(32, tx.Hash.Length);

                _output.WriteLine($"    Tx: account={Convert.ToHexString(tx.Account).Substring(0, 16)}..., " +
                                  $"lt={tx.Lt}, hash={Convert.ToHexString(tx.Hash).Substring(0, 16)}...");
            }
        }
        else
        {
            _output.WriteLine($"No transactions in this shard block (seqno {workchainShard.Seqno})");
        }
    }

    [Fact]
    public async Task FullWorkflow_ShouldSucceed()
    {
        _output.WriteLine("=== Starting Full Lite Client Workflow Test ===\n");

        // Step 1: Get server time
        _output.WriteLine("Step 1: Getting server time...");
        DateTimeOffset serverTime = await _client.GetTimeAsync();
        Assert.True(serverTime > DateTimeOffset.MinValue);
        _output.WriteLine($"  Server time: {serverTime:yyyy-MM-dd HH:mm:ss} UTC\n");

        // Step 2: Get server version
        _output.WriteLine("Step 2: Getting server version...");
        (int Version, long Capabilities, int Now) version = await _client.GetVersionAsync();
        Assert.NotNull(version);
        _output.WriteLine($"  Version: {version.Version}");
        _output.WriteLine($"  Capabilities: {version.Capabilities}\n");

        // Step 3: Get masterchain info
        _output.WriteLine("Step 3: Getting masterchain info...");
        MasterchainInfo masterchainInfo = await _client.GetMasterchainInfoAsync();
        Assert.True(masterchainInfo.Last.Seqno > 0u);
        _output.WriteLine($"  Latest block: {masterchainInfo.Last.Seqno}");
        _output.WriteLine($"  Workchain: {masterchainInfo.Last.Workchain}");
        _output.WriteLine($"  Shard: {masterchainInfo.Last.Shard:X16}\n");

        // Step 4: Lookup an older block
        _output.WriteLine("Step 4: Looking up older block...");
        uint oldSeqno = masterchainInfo.Last.Seqno - 10;
        BlockId oldBlock = await _client.LookupBlockAsync(-1, long.MinValue, oldSeqno);
        Assert.Equal(oldSeqno, oldBlock.Seqno);
        _output.WriteLine($"  Found block {oldSeqno}");
        _output.WriteLine($"  Root hash: {oldBlock.RootHashHex.Substring(0, 32)}...\n");

        // Step 5: Get block header
        _output.WriteLine("Step 5: Getting block header...");
        BlockHeader header = await _client.GetBlockHeaderAsync(masterchainInfo.Last);
        Assert.NotNull(header);
        Assert.NotNull(header.HeaderProof);
        Assert.True(header.HeaderProof.Length > 0);
        _output.WriteLine($"  Block ID: {header.Id.Seqno}");
        _output.WriteLine($"  Mode: {header.Mode}");
        _output.WriteLine($"  Proof size: {header.HeaderProof.Length} bytes\n");

        // Step 6: Get all shards
        _output.WriteLine("Step 6: Getting all shards...");
        BlockId[] shards = await _client.GetAllShardsInfoAsync(masterchainInfo.Last);
        Assert.True(shards.Length > 0);
        _output.WriteLine($"  Found {shards.Length} shard(s)");

        BlockId[] wc0Shards = shards.Where(s => s.Workchain == 0).ToArray();
        Assert.True(wc0Shards.Length > 0, "Should have workchain 0 shards");
        _output.WriteLine($"  Workchain 0 shards: {wc0Shards.Length}\n");

        // Step 7: List transactions from a workchain shard
        _output.WriteLine("Step 7: Listing transactions from workchain shard...");
        BlockId shard = wc0Shards[0];
        BlockTransactions transactions = await _client.ListBlockTransactionsAsync(shard, 20);
        Assert.NotNull(transactions);
        _output.WriteLine($"  Shard: {shard.Shard:X16}");
        _output.WriteLine($"  Block seqno: {shard.Seqno}");
        _output.WriteLine($"  Transactions found: {transactions.Transactions.Count}");
        _output.WriteLine($"  Incomplete: {transactions.Incomplete}\n");

        _output.WriteLine("=== Full Workflow Test Completed Successfully ===");
    }

    [Fact]
    public async Task ConcurrentRequests_ShouldSucceed()
    {
        // Test that multiple concurrent requests work correctly
        Task<DateTimeOffset> task1 = _client.GetTimeAsync();
        Task<(int Version, long Capabilities, int Now)> task2 = _client.GetVersionAsync();
        Task<MasterchainInfo> task3 = _client.GetMasterchainInfoAsync();
        Task<DateTimeOffset> task4 = _client.GetTimeAsync();
        Task<DateTimeOffset> task5 = _client.GetTimeAsync();

        // Act
        await Task.WhenAll(task1, task2, task3, task4, task5);

        // Assert
        DateTimeOffset time1 = await task1;
        (int Version, long Capabilities, int Now) version = await task2;
        MasterchainInfo masterchainInfo = await task3;
        DateTimeOffset time2 = await task4;
        DateTimeOffset time3 = await task5;

        Assert.True(time1 > DateTimeOffset.MinValue);
        Assert.True(version.Version > 0);
        Assert.True(masterchainInfo.Last.Seqno > 0u);
        Assert.True(time2 > DateTimeOffset.MinValue);
        Assert.True(time3 > DateTimeOffset.MinValue);

        _output.WriteLine("Concurrent requests completed:");
        _output.WriteLine($"  Time 1: {time1:yyyy-MM-dd HH:mm:ss} UTC");
        _output.WriteLine($"  Version: {version.Version}");
        _output.WriteLine($"  Masterchain seqno: {masterchainInfo.Last.Seqno}");
        _output.WriteLine($"  Time 2: {time2:yyyy-MM-dd HH:mm:ss} UTC");
        _output.WriteLine($"  Time 3: {time3:yyyy-MM-dd HH:mm:ss} UTC");
    }
}