using Ton.Core.Addresses;
using Ton.Core.Boc;
using Ton.Core.Contracts;
using Ton.Core.Types;
using Ton.LiteClient.Models;
using Xunit.Abstractions;
using AccountState = Ton.LiteClient.Models.AccountState;

namespace Ton.LiteClient.Tests;

/// <summary>
///     Integration tests for LiteClient using public TON lite servers.
///     These tests require network access and may take longer to execute.
/// </summary>
[Trait("Category", "Integration")]
public class LiteClientIntegrationTests(ITestOutputHelper output) : IAsyncLifetime
{
    // TON mainnet global config URL
    const string MainnetConfigUrl = "https://ton.org/global-config.json";
    LiteClient client = null!;

    public async Task InitializeAsync()
    {
        // Create lite client from mainnet config - will use round-robin if multiple servers available
        // Connection happens automatically on first request
        client = await LiteClientFactory.CreateFromUrlAsync(MainnetConfigUrl);
        output.WriteLine("Created LiteClient from mainnet config");
    }

    public Task DisposeAsync()
    {
        client?.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task GetTime_ShouldReturnCurrentTime()
    {
        // Act
        DateTimeOffset serverTime = await client.GetTimeAsync();

        // Assert
        DateTimeOffset currentTime = DateTimeOffset.UtcNow;
        Assert.True(serverTime > DateTimeOffset.MinValue, "Server time should be valid");
        Assert.True(Math.Abs((serverTime - currentTime).TotalMinutes) < 5,
            "Server time should be within 5 minutes of current time");

        output.WriteLine($"Server time: {serverTime:yyyy-MM-dd HH:mm:ss} UTC");
        output.WriteLine($"Local time: {currentTime:yyyy-MM-dd HH:mm:ss} UTC");
        output.WriteLine($"Difference: {(serverTime - currentTime).TotalSeconds:F1} seconds");
    }

    [Fact]
    public async Task GetVersion_ShouldReturnVersionInfo()
    {
        // Act
        (int Version, long Capabilities, int Now) version = await client.GetVersionAsync();

        // Assert
        Assert.True(version.Version > 0, "Version number should be positive");
        Assert.True(version.Capabilities >= 0, "Capabilities should be non-negative");

        output.WriteLine($"LiteServer Version: {version.Version}");
        output.WriteLine($"Capabilities: {version.Capabilities}");
        output.WriteLine($"Now: {version.Now}");
    }

    [Fact]
    public async Task GetMasterchainInfo_ShouldReturnValidBlock()
    {
        // Act
        MasterchainInfo masterchainInfo = await client.GetMasterchainInfoAsync();

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

        output.WriteLine("Latest Masterchain Block:");
        output.WriteLine($"  Seqno: {masterchainInfo.Last.Seqno}");
        output.WriteLine($"  Workchain: {masterchainInfo.Last.Workchain}");
        output.WriteLine($"  Shard: {masterchainInfo.Last.Shard:X16}");
        output.WriteLine($"  Root Hash: {masterchainInfo.Last.RootHashHex}");
        output.WriteLine($"  File Hash: {masterchainInfo.Last.FileHashHex}");
    }

    [Fact]
    public async Task GetAllShardsInfo_ShouldReturnWorkchainShards()
    {
        // Arrange
        MasterchainInfo masterchainInfo = await client.GetMasterchainInfoAsync();

        // Act
        BlockId[] shards = await client.GetAllShardsInfoAsync(masterchainInfo.Last);

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

            output.WriteLine($"Shard: wc={shard.Workchain}, shard={shard.Shard:X16}, " +
                             $"seqno={shard.Seqno}, " +
                             $"rootHash={shard.RootHashHex.Substring(0, 16)}..., " +
                             $"fileHash={shard.FileHashHex.Substring(0, 16)}...");
        }

        output.WriteLine($"Total shards: {shards.Length}");
        output.WriteLine($"Workchain 0 shards: {workchain0Shards.Length}");
    }

    [Fact]
    public async Task LookupBlock_BySeqno_ShouldReturnBlockId()
    {
        // Arrange
        MasterchainInfo masterchainInfo = await client.GetMasterchainInfoAsync();
        uint seqno = masterchainInfo.Last.Seqno - 10; // Look up a recent block

        // Act
        BlockId blockId = await client.LookupBlockAsync(
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

        output.WriteLine($"Looked up block {seqno}:");
        output.WriteLine($"  Root Hash: {blockId.RootHashHex}");
        output.WriteLine($"  File Hash: {blockId.FileHashHex}");
    }

    [Fact]
    public async Task GetBlockHeader_ShouldReturnHeaderWithValidData()
    {
        // Arrange
        MasterchainInfo masterchainInfo = await client.GetMasterchainInfoAsync();
        BlockId blockId = masterchainInfo.Last;

        // Act
        BlockHeader header = await client.GetBlockHeaderAsync(blockId);

        // Assert
        Assert.NotNull(header);
        Assert.NotNull(header.Id);
        Assert.NotNull(header.HeaderProof);
        Assert.True(header.HeaderProof.Length > 0, "Header proof should not be empty");
        Assert.Equal(blockId.Seqno, header.Id.Seqno);
        Assert.Equal(blockId.Workchain, header.Id.Workchain);

        output.WriteLine($"Block Header for seqno {blockId.Seqno}:");
        output.WriteLine($"  Block ID: {header.Id}");
        output.WriteLine($"  Mode: {header.Mode}");
        output.WriteLine($"  Header Proof Size: {header.HeaderProof.Length} bytes");

        // Verify we can parse the proof as a cell
        Cell proofCell = Cell.FromBoc(header.HeaderProof)[0];
        Assert.NotNull(proofCell);
        output.WriteLine($"  Proof Cell Type: {proofCell.Type}");

        // If it's a MerkleProof, we can unwrap it
        if (proofCell.IsExotic)
        {
            Cell unwrapped = proofCell.UnwrapProof();
            Assert.NotNull(unwrapped);
            output.WriteLine($"  Unwrapped Cell Type: {unwrapped.Type}");
        }
    }

    [Fact]
    public async Task GetAccountState_DurovWallet_ShouldReturnValidState()
    {
        // Arrange - Durov's wallet address
        Address address = Address.Parse("UQDYzZmfsrGzhObKJUw4gzdeIxEai3jAFbiGKGwxvxHinf4K");
        MasterchainInfo masterchainInfo = await client.GetMasterchainInfoAsync();
        BlockId blockId = masterchainInfo.Last;

        // Act
        AccountState accountState = await client.GetAccountStateAsync(address, blockId);

        // Assert
        Assert.NotNull(accountState);
        Assert.Equal(address, accountState.Address);
        Assert.NotNull(accountState.Block);
        Assert.NotNull(accountState.ShardBlock);

        output.WriteLine($"Account State for {address.ToString(bounceable: true)}:");
        output.WriteLine($"  Balance: {accountState.BalanceInTon:F4} TON");
        output.WriteLine($"  State: {accountState.State}");
        output.WriteLine($"  Is Active: {accountState.IsActive}");
        output.WriteLine($"  Is Contract: {accountState.IsContract}");
        output.WriteLine($"  Block: {accountState.Block}");
        output.WriteLine($"  Shard Block: {accountState.ShardBlock}");

        if (accountState.LastTransaction != null)
        {
            output.WriteLine($"  Last TX LT: {accountState.LastTransaction.Lt}");
            output.WriteLine($"  Last TX Hash: {Convert.ToHexString(accountState.LastTransaction.Hash)[..16]}...");
        }

        if (accountState.Code != null) output.WriteLine($"  Has Code: Yes ({accountState.Code.Bits.Length} bits)");

        if (accountState.Data != null) output.WriteLine($"  Has Data: Yes ({accountState.Data.Bits.Length} bits)");

        // Durov's wallet should have a balance
        Assert.True(accountState.Balance > 0, "Durov's wallet should have a positive balance");
    }

    [Fact]
    public async Task GetAccountTransactions_DurovWallet_ShouldReturnTransactions()
    {
        // Arrange - Durov's wallet
        Address address = Address.Parse("UQDYzZmfsrGzhObKJUw4gzdeIxEai3jAFbiGKGwxvxHinf4K");
        MasterchainInfo masterchainInfo = await client.GetMasterchainInfoAsync();
        AccountState accountState = await client.GetAccountStateAsync(address, masterchainInfo.Last);

        // Skip if no transactions
        if (accountState.LastTransaction == null)
        {
            output.WriteLine("No transactions found for this account");
            return;
        }

        // Act
        AccountTransactions transactions = await client.GetAccountTransactionsAsync(
            address,
            10,
            accountState.LastTransaction.Lt,
            accountState.LastTransaction.Hash
        );

        // Assert
        Assert.NotNull(transactions);
        Assert.NotNull(transactions.Transactions);
        Assert.True(transactions.Transactions.Count > 0, "Should have at least one transaction");

        output.WriteLine($"Retrieved {transactions.Transactions.Count} transactions:");
        foreach (Transaction tx in transactions.Transactions.Take(5))
            output.WriteLine($"  LT: {tx.Lt}, Hash: {Convert.ToHexString(tx.Hash())[..16]}...");
    }

    [Fact]
    public async Task ContractProvider_DurovWallet_ShouldWorkCorrectly()
    {
        // Arrange - Durov's wallet
        Address address = Address.Parse("UQDYzZmfsrGzhObKJUw4gzdeIxEai3jAFbiGKGwxvxHinf4K");
        IContractProvider provider = client.Provider(address);

        // Act
        ContractState state = await provider.GetStateAsync();

        // Assert
        Assert.NotNull(state);
        Assert.True(state.Balance > 0, "Durov's wallet should have balance");

        output.WriteLine("Contract State via Provider:");
        output.WriteLine($"  Balance: {state.Balance} nanotons");
        output.WriteLine($"  State Type: {state.State.GetType().Name}");
        output.WriteLine($"  Is Active: {state.State is ContractState.AccountStateInfo.Active}");

        if (state.Last != null) output.WriteLine($"  Last TX LT: {state.Last.Lt}");
    }

    [Fact]
    public async Task ListBlockTransactions_OnMasterchain_ShouldReturnTransactions()
    {
        // Arrange
        MasterchainInfo masterchainInfo = await client.GetMasterchainInfoAsync();
        BlockId blockId = masterchainInfo.Last;

        // Act
        BlockTransactions blockTransactions = await client.ListBlockTransactionsAsync(blockId, 20);

        // Assert
        Assert.NotNull(blockTransactions);
        Assert.NotNull(blockTransactions.BlockId);
        Assert.Equal(20u, blockTransactions.RequestedCount);
        Assert.NotNull(blockTransactions.Transactions);

        // Masterchain blocks might have few or no transactions, so we just validate the structure
        output.WriteLine($"Masterchain Block Transactions (block {blockId.Seqno}):");
        output.WriteLine($"  Total found: {blockTransactions.Transactions.Count}");
        output.WriteLine($"  Incomplete: {blockTransactions.Incomplete}");

        foreach (BlockTransaction tx in blockTransactions.Transactions.Take(5))
        {
            Assert.NotNull(tx.Account);
            Assert.Equal(32, tx.Account.Hash.Length);
            Assert.True(tx.Lt > 0, "Logical time should be positive");
            Assert.NotNull(tx.Hash);
            Assert.Equal(32, tx.Hash.Length);

            output.WriteLine($"    Tx: account={tx.Account}, " +
                             $"lt={tx.Lt}, hash={Convert.ToHexString(tx.Hash).Substring(0, 16)}...");
        }
    }

    [Fact]
    public async Task ListBlockTransactions_OnWorkchainShard_ShouldReturnTransactions()
    {
        // Arrange
        MasterchainInfo masterchainInfo = await client.GetMasterchainInfoAsync();
        BlockId[] shards = await client.GetAllShardsInfoAsync(masterchainInfo.Last);

        // Find a workchain 0 shard
        BlockId? workchainShard = shards.FirstOrDefault(s => s.Workchain == 0);
        Assert.NotNull(workchainShard);

        // Act
        BlockTransactions blockTransactions = await client.ListBlockTransactionsAsync(workchainShard);

        // Assert
        Assert.NotNull(blockTransactions);
        Assert.NotNull(blockTransactions.BlockId);
        Assert.NotNull(blockTransactions.Transactions);

        // Workchain shards typically have transactions
        if (blockTransactions.Transactions.Count > 0)
        {
            output.WriteLine($"Workchain Shard Block Transactions (block {workchainShard.Seqno}):");
            output.WriteLine($"  Shard: {workchainShard.Shard:X16}");
            output.WriteLine($"  Total found: {blockTransactions.Transactions.Count}");
            output.WriteLine($"  Incomplete: {blockTransactions.Incomplete}");

            foreach (BlockTransaction tx in blockTransactions.Transactions.Take(5))
            {
                Assert.NotNull(tx.Account);
                Assert.Equal(32, tx.Account.Hash.Length);
                Assert.True(tx.Lt > 0, "Logical time should be positive");
                Assert.NotNull(tx.Hash);
                Assert.Equal(32, tx.Hash.Length);

                output.WriteLine($"    Tx: account={tx.Account}, " +
                                 $"lt={tx.Lt}, hash={Convert.ToHexString(tx.Hash).Substring(0, 16)}...");
            }
        }
        else
        {
            output.WriteLine($"No transactions in this shard block (seqno {workchainShard.Seqno})");
        }
    }

    [Fact]
    public async Task FullWorkflow_ShouldSucceed()
    {
        output.WriteLine("=== Starting Full Lite Client Workflow Test ===\n");

        // Step 1: Get server time
        output.WriteLine("Step 1: Getting server time...");
        DateTimeOffset serverTime = await client.GetTimeAsync();
        Assert.True(serverTime > DateTimeOffset.MinValue);
        output.WriteLine($"  Server time: {serverTime:yyyy-MM-dd HH:mm:ss} UTC\n");

        // Step 2: Get server version
        output.WriteLine("Step 2: Getting server version...");
        (int Version, long Capabilities, int Now) version = await client.GetVersionAsync();
        output.WriteLine($"  Version: {version.Version}");
        output.WriteLine($"  Capabilities: {version.Capabilities}\n");

        // Step 3: Get masterchain info
        output.WriteLine("Step 3: Getting masterchain info...");
        MasterchainInfo masterchainInfo = await client.GetMasterchainInfoAsync();
        Assert.True(masterchainInfo.Last.Seqno > 0u);
        output.WriteLine($"  Latest block: {masterchainInfo.Last.Seqno}");
        output.WriteLine($"  Workchain: {masterchainInfo.Last.Workchain}");
        output.WriteLine($"  Shard: {masterchainInfo.Last.Shard:X16}\n");

        // Step 4: Lookup an older block
        output.WriteLine("Step 4: Looking up older block...");
        uint oldSeqno = masterchainInfo.Last.Seqno - 10;
        BlockId oldBlock = await client.LookupBlockAsync(-1, long.MinValue, oldSeqno);
        Assert.Equal(oldSeqno, oldBlock.Seqno);
        output.WriteLine($"  Found block {oldSeqno}");
        output.WriteLine($"  Root hash: {oldBlock.RootHashHex.Substring(0, 32)}...\n");

        // Step 5: Get block header
        output.WriteLine("Step 5: Getting block header...");
        BlockHeader header = await client.GetBlockHeaderAsync(masterchainInfo.Last);
        Assert.NotNull(header);
        Assert.NotNull(header.HeaderProof);
        Assert.True(header.HeaderProof.Length > 0);
        output.WriteLine($"  Block ID: {header.Id.Seqno}");
        output.WriteLine($"  Mode: {header.Mode}");
        output.WriteLine($"  Proof size: {header.HeaderProof.Length} bytes\n");

        // Step 6: Get all shards
        output.WriteLine("Step 6: Getting all shards...");
        BlockId[] shards = await client.GetAllShardsInfoAsync(masterchainInfo.Last);
        Assert.True(shards.Length > 0);
        output.WriteLine($"  Found {shards.Length} shard(s)");

        BlockId[] wc0Shards = shards.Where(s => s.Workchain == 0).ToArray();
        Assert.True(wc0Shards.Length > 0, "Should have workchain 0 shards");
        output.WriteLine($"  Workchain 0 shards: {wc0Shards.Length}\n");

        // Step 7: List transactions from a workchain shard
        output.WriteLine("Step 7: Listing transactions from workchain shard...");
        BlockId shard = wc0Shards[0];
        BlockTransactions transactions = await client.ListBlockTransactionsAsync(shard, 20);
        Assert.NotNull(transactions);
        output.WriteLine($"  Shard: {shard.Shard:X16}");
        output.WriteLine($"  Block seqno: {shard.Seqno}");
        output.WriteLine($"  Transactions found: {transactions.Transactions.Count}");
        output.WriteLine($"  Incomplete: {transactions.Incomplete}\n");

        output.WriteLine("=== Full Workflow Test Completed Successfully ===");
    }

    [Fact]
    public async Task ConcurrentRequests_ShouldSucceed()
    {
        // Test that multiple concurrent requests work correctly
        Task<DateTimeOffset> task1 = client.GetTimeAsync();
        Task<(int Version, long Capabilities, int Now)> task2 = client.GetVersionAsync();
        Task<MasterchainInfo> task3 = client.GetMasterchainInfoAsync();
        Task<DateTimeOffset> task4 = client.GetTimeAsync();
        Task<DateTimeOffset> task5 = client.GetTimeAsync();

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

        output.WriteLine("Concurrent requests completed:");
        output.WriteLine($"  Time 1: {time1:yyyy-MM-dd HH:mm:ss} UTC");
        output.WriteLine($"  Version: {version.Version}");
        output.WriteLine($"  Masterchain seqno: {masterchainInfo.Last.Seqno}");
        output.WriteLine($"  Time 2: {time2:yyyy-MM-dd HH:mm:ss} UTC");
        output.WriteLine($"  Time 3: {time3:yyyy-MM-dd HH:mm:ss} UTC");
    }
}