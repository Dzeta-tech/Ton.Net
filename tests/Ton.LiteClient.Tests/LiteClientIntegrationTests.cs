using System.Numerics;
using Ton.Adnl.Protocol;
using Ton.Core.Addresses;
using Ton.Core.Boc;
using Ton.Core.Contracts;
using Ton.Core.Types;
using Ton.LiteClient.Models;
using AccountState = Ton.LiteClient.Models.AccountState;

namespace Ton.LiteClient.Tests;

/// <summary>
///     Integration tests for LiteClient using public TON lite servers.
///     These tests require network access and may take longer to execute.
/// </summary>
[TestFixture]
[Category("Integration")]
[NonParallelizable]
public class LiteClientIntegrationTests
{
    // TON mainnet global config URL
    const string MainnetConfigUrl = "https://ton.org/global-config.json";
    LiteClient client = null!;

    [SetUp]
    public async Task Setup()
    {
        // Create lite client from mainnet config - will use round-robin if multiple servers available
        // Connection happens automatically on first request
        client = await LiteClientFactory.CreateFromUrlAsync(MainnetConfigUrl);
        await TestContext.Out.WriteLineAsync("Created LiteClient from mainnet config");
    }

    [TearDown]
    public void TearDown()
    {
        client?.Dispose();
    }

    [Test]
    public async Task GetTime_ShouldReturnCurrentTime()
    {
        // Act
        DateTimeOffset serverTime = await client.GetTimeAsync();

        // Assert
        DateTimeOffset currentTime = DateTimeOffset.UtcNow;
        Assert.Multiple(() =>
        {
            Assert.That(serverTime, Is.GreaterThan(DateTimeOffset.MinValue), "Server time should be valid");
            Assert.That(Math.Abs((serverTime - currentTime).TotalMinutes) < 5, Is.True,
                "Server time should be within 5 minutes of current time");
        });

        await TestContext.Out.WriteLineAsync($"Server time: {serverTime:yyyy-MM-dd HH:mm:ss} UTC");
        await TestContext.Out.WriteLineAsync($"Local time: {currentTime:yyyy-MM-dd HH:mm:ss} UTC");
        await TestContext.Out.WriteLineAsync($"Difference: {(serverTime - currentTime).TotalSeconds:F1} seconds");
    }

    [Test]
    public async Task GetVersion_ShouldReturnVersionInfo()
    {
        // Act
        (int Version, long Capabilities, int Now) version = await client.GetVersionAsync();

        Assert.Multiple(() =>
        {
            // Assert
            Assert.That(version.Version, Is.GreaterThan(0), "Version number should be positive");
            Assert.That(version.Capabilities >= 0, Is.True, "Capabilities should be non-negative");
        });

        await TestContext.Out.WriteLineAsync($"LiteServer Version: {version.Version}");
        await TestContext.Out.WriteLineAsync($"Capabilities: {version.Capabilities}");
        await TestContext.Out.WriteLineAsync($"Now: {version.Now}");
    }

    [Test]
    public async Task GetMasterchainInfo_ShouldReturnValidBlock()
    {
        // Act
        MasterchainInfo masterchainInfo = await client.GetMasterchainInfoAsync();

        // Assert
        Assert.That(masterchainInfo, Is.Not.Null);
        Assert.That(masterchainInfo.Last, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(masterchainInfo.Last.Workchain, Is.EqualTo(-1));
            Assert.That(masterchainInfo.Last.Shard, Is.EqualTo(long.MinValue));
            Assert.That(masterchainInfo.Last.Seqno, Is.GreaterThan(0u), "Block seqno should be positive");
            Assert.That(masterchainInfo.Last.RootHash, Is.Not.Null);
        });
        Assert.Multiple(() =>
        {
            Assert.That(masterchainInfo.Last.RootHash.Length, Is.EqualTo(32));
            Assert.That(masterchainInfo.Last.FileHash, Is.Not.Null);
        });
        Assert.Multiple(() =>
        {
            Assert.That(masterchainInfo.Last.FileHash.Length, Is.EqualTo(32));

            Assert.That(masterchainInfo.Init, Is.Not.Null);
        });
        Assert.That(masterchainInfo.Init.Workchain, Is.EqualTo(-1));

        await TestContext.Out.WriteLineAsync("Latest Masterchain Block:");
        await TestContext.Out.WriteLineAsync($"  Seqno: {masterchainInfo.Last.Seqno}");
        await TestContext.Out.WriteLineAsync($"  Workchain: {masterchainInfo.Last.Workchain}");
        await TestContext.Out.WriteLineAsync($"  Shard: {masterchainInfo.Last.Shard:X16}");
        await TestContext.Out.WriteLineAsync($"  Root Hash: {masterchainInfo.Last.RootHashHex}");
        await TestContext.Out.WriteLineAsync($"  File Hash: {masterchainInfo.Last.FileHashHex}");
    }

    [Test]
    public async Task GetAllShardsInfo_ShouldReturnWorkchainShards()
    {
        // Arrange
        MasterchainInfo masterchainInfo = await client.GetMasterchainInfoAsync();

        // Act
        BlockId[] shards = await client.GetAllShardsInfoAsync(masterchainInfo.Last);

        // Assert
        Assert.That(shards, Is.Not.Null);
        Assert.That(shards.Length, Is.GreaterThan(0), "Should have at least one shard");

        // Validate that we have workchain 0 shards
        BlockId[] workchain0Shards = shards.Where(s => s.Workchain == 0).ToArray();
        Assert.That(workchain0Shards.Length, Is.GreaterThan(0), "Should have at least one shard for workchain 0");

        // Validate each shard has proper data
        foreach (BlockId shard in shards)
        {
            Assert.Multiple(() =>
            {
                Assert.That(shard.Seqno, Is.GreaterThan(0u), $"Shard seqno should be positive for shard {shard.Shard:X16}");
                Assert.That(shard.RootHash, Is.Not.Null);
            });
            Assert.Multiple(() =>
            {
                Assert.That(shard.RootHash.Length, Is.EqualTo(32));
                Assert.That(shard.FileHash, Is.Not.Null);
            });
            Assert.That(shard.FileHash.Length, Is.EqualTo(32));

            await TestContext.Out.WriteLineAsync($"Shard: wc={shard.Workchain}, shard={shard.Shard:X16}, " +
                                                 $"seqno={shard.Seqno}, " +
                                                 $"rootHash={shard.RootHashHex.Substring(0, 16)}..., " +
                                                 $"fileHash={shard.FileHashHex.Substring(0, 16)}...");
        }

        await TestContext.Out.WriteLineAsync($"Total shards: {shards.Length}");
        await TestContext.Out.WriteLineAsync($"Workchain 0 shards: {workchain0Shards.Length}");
    }

    [Test]
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
        Assert.That(blockId, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(blockId.Workchain, Is.EqualTo(-1));
            Assert.That(blockId.Shard, Is.EqualTo(long.MinValue));
            Assert.That(blockId.Seqno, Is.EqualTo(seqno));
            Assert.That(blockId.RootHash, Is.Not.Null);
        });
        Assert.Multiple(() =>
        {
            Assert.That(blockId.RootHash.Length, Is.EqualTo(32));
            Assert.That(blockId.FileHash, Is.Not.Null);
        });
        Assert.That(blockId.FileHash.Length, Is.EqualTo(32));

        await TestContext.Out.WriteLineAsync($"Looked up block {seqno}:");
        await TestContext.Out.WriteLineAsync($"  Root Hash: {blockId.RootHashHex}");
        await TestContext.Out.WriteLineAsync($"  File Hash: {blockId.FileHashHex}");
    }

    [Test]
    public async Task GetBlockHeader_ShouldReturnHeaderWithValidData()
    {
        // Arrange
        MasterchainInfo masterchainInfo = await client.GetMasterchainInfoAsync();
        BlockId blockId = masterchainInfo.Last;

        // Act
        BlockHeader header = await client.GetBlockHeaderAsync(blockId);

        // Assert
        Assert.That(header, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(header.Id, Is.Not.Null);
            Assert.That(header.HeaderProof, Is.Not.Null);
        });
        Assert.Multiple(() =>
        {
            Assert.That(header.HeaderProof.Length, Is.GreaterThan(0), "Header proof should not be empty");
            Assert.That(header.Id.Seqno, Is.EqualTo(blockId.Seqno));
            Assert.That(header.Id.Workchain, Is.EqualTo(blockId.Workchain));
        });

        await TestContext.Out.WriteLineAsync($"Block Header for seqno {blockId.Seqno}:");
        await TestContext.Out.WriteLineAsync($"  Block ID: {header.Id}");
        await TestContext.Out.WriteLineAsync($"  Mode: {header.Mode}");
        await TestContext.Out.WriteLineAsync($"  Header Proof Size: {header.HeaderProof.Length} bytes");

        // Verify we can parse the proof as a cell
        Cell proofCell = Cell.FromBoc(header.HeaderProof)[0];
        Assert.That(proofCell, Is.Not.Null);
        await TestContext.Out.WriteLineAsync($"  Proof Cell Type: {proofCell.Type}");

        // If it's a MerkleProof, we can unwrap it
        if (proofCell.IsExotic)
        {
            Cell unwrapped = proofCell.UnwrapProof();
            Assert.That(unwrapped, Is.Not.Null);
            await TestContext.Out.WriteLineAsync($"  Unwrapped Cell Type: {unwrapped.Type}");
        }
    }

    [Test]
    public async Task GetAccountState_DurovWallet_ShouldReturnValidState()
    {
        // Arrange - Durov's wallet address
        Address address = Address.Parse("UQDYzZmfsrGzhObKJUw4gzdeIxEai3jAFbiGKGwxvxHinf4K");
        MasterchainInfo masterchainInfo = await client.GetMasterchainInfoAsync();
        BlockId blockId = masterchainInfo.Last;

        // Act
        AccountState accountState = await client.GetAccountStateAsync(address, blockId);

        // Assert
        Assert.That(accountState, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(accountState.Address, Is.EqualTo(address));
            Assert.That(accountState.Block, Is.Not.Null);
            Assert.That(accountState.ShardBlock, Is.Not.Null);
        });

        await TestContext.Out.WriteLineAsync($"Account State for {address.ToString(bounceable: true)}:");
        await TestContext.Out.WriteLineAsync($"  Balance: {accountState.BalanceInTon:F4} TON");
        await TestContext.Out.WriteLineAsync($"  State: {accountState.State}");
        await TestContext.Out.WriteLineAsync($"  Is Active: {accountState.IsActive}");
        await TestContext.Out.WriteLineAsync($"  Is Contract: {accountState.IsContract}");
        await TestContext.Out.WriteLineAsync($"  Block: {accountState.Block}");
        await TestContext.Out.WriteLineAsync($"  Shard Block: {accountState.ShardBlock}");

        if (accountState.LastTransaction != null)
        {
            await TestContext.Out.WriteLineAsync($"  Last TX LT: {accountState.LastTransaction.Lt}");
            await TestContext.Out.WriteLineAsync($"  Last TX Hash: {Convert.ToHexString(accountState.LastTransaction.Hash)[..16]}...");
        }

        if (accountState.Code != null) await TestContext.Out.WriteLineAsync($"  Has Code: Yes ({accountState.Code.Bits.Length} bits)");

        if (accountState.Data != null) await TestContext.Out.WriteLineAsync($"  Has Data: Yes ({accountState.Data.Bits.Length} bits)");

        // Durov's wallet should have a balance
        Assert.That(accountState.Balance, Is.GreaterThan(0), "Durov's wallet should have a positive balance");
    }

    [Test]
    public async Task GetAccountTransactions_DurovWallet_ShouldReturnTransactions()
    {
        // Arrange - Durov's wallet
        Address address = Address.Parse("UQDYzZmfsrGzhObKJUw4gzdeIxEai3jAFbiGKGwxvxHinf4K");
        MasterchainInfo masterchainInfo = await client.GetMasterchainInfoAsync();
        AccountState accountState = await client.GetAccountStateAsync(address, masterchainInfo.Last);

        // Skip if no transactions
        if (accountState.LastTransaction == null)
        {
            await TestContext.Out.WriteLineAsync("No transactions found for this account");
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
        Assert.That(transactions, Is.Not.Null);
        Assert.That(transactions.Transactions, Is.Not.Null);
        Assert.That(transactions.Transactions.Count, Is.GreaterThan(0), "Should have at least one transaction");

        await TestContext.Out.WriteLineAsync($"Retrieved {transactions.Transactions.Count} transactions:");
        foreach (Transaction tx in transactions.Transactions.Take(5))
            await TestContext.Out.WriteLineAsync($"  LT: {tx.Lt}, Hash: {Convert.ToHexString(tx.Hash())[..16]}...");
    }

    [Test]
    public async Task ContractProvider_DurovWallet_ShouldWorkCorrectly()
    {
        // Arrange - Durov's wallet
        Address address = Address.Parse("UQDYzZmfsrGzhObKJUw4gzdeIxEai3jAFbiGKGwxvxHinf4K");
        IContractProvider provider = client.Provider(address);

        // Act
        ContractState state = await provider.GetStateAsync();

        // Assert
        Assert.That(state, Is.Not.Null);
        Assert.That(state.Balance, Is.GreaterThan(0), "Durov's wallet should have balance");

        await TestContext.Out.WriteLineAsync("Contract State via Provider:");
        await TestContext.Out.WriteLineAsync($"  Balance: {state.Balance} nanotons");
        await TestContext.Out.WriteLineAsync($"  State Type: {state.State.GetType().Name}");
        await TestContext.Out.WriteLineAsync($"  Is Active: {state.State is ContractState.AccountStateInfo.Active}");

        if (state.Last != null) await TestContext.Out.WriteLineAsync($"  Last TX LT: {state.Last.Lt}");
    }

    [Test]
    public async Task ListBlockTransactions_OnMasterchain_ShouldReturnTransactions()
    {
        // Arrange
        MasterchainInfo masterchainInfo = await client.GetMasterchainInfoAsync();
        BlockId blockId = masterchainInfo.Last;

        // Act
        BlockTransactions blockTransactions = await client.ListBlockTransactionsAsync(blockId, 20);

        // Assert
        Assert.That(blockTransactions, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(blockTransactions.BlockId, Is.Not.Null);
            Assert.That(blockTransactions.RequestedCount, Is.EqualTo(20u));
            Assert.That(blockTransactions.Transactions, Is.Not.Null);
        });

        // Masterchain blocks might have few or no transactions, so we just validate the structure
        await TestContext.Out.WriteLineAsync($"Masterchain Block Transactions (block {blockId.Seqno}):");
        await TestContext.Out.WriteLineAsync($"  Total found: {blockTransactions.Transactions.Count}");
        await TestContext.Out.WriteLineAsync($"  Incomplete: {blockTransactions.Incomplete}");

        foreach (BlockTransaction tx in blockTransactions.Transactions.Take(5))
        {
            Assert.That(tx.Account, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(tx.Account.Hash.Length, Is.EqualTo(32));
                Assert.That(tx.Lt, Is.GreaterThan(0), "Logical time should be positive");
                Assert.That(tx.Hash, Is.Not.Null);
            });
            Assert.That(tx.Hash.Length, Is.EqualTo(32));

            await TestContext.Out.WriteLineAsync($"    Tx: account={tx.Account}, " +
                                                 $"lt={tx.Lt}, hash={Convert.ToHexString(tx.Hash).Substring(0, 16)}...");
        }
    }

    [Test]
    public async Task ListBlockTransactions_OnWorkchainShard_ShouldReturnTransactions()
    {
        // Arrange
        MasterchainInfo masterchainInfo = await client.GetMasterchainInfoAsync();
        BlockId[] shards = await client.GetAllShardsInfoAsync(masterchainInfo.Last);

        // Find a workchain 0 shard
        BlockId? workchainShard = shards.FirstOrDefault(s => s.Workchain == 0);
        Assert.That(workchainShard, Is.Not.Null);

        // Act
        BlockTransactions blockTransactions = await client.ListBlockTransactionsAsync(workchainShard);

        // Assert
        Assert.That(blockTransactions, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(blockTransactions.BlockId, Is.Not.Null);
            Assert.That(blockTransactions.Transactions, Is.Not.Null);
        });

        // Workchain shards typically have transactions
        if (blockTransactions.Transactions.Count > 0)
        {
            await TestContext.Out.WriteLineAsync($"Workchain Shard Block Transactions (block {workchainShard.Seqno}):");
            await TestContext.Out.WriteLineAsync($"  Shard: {workchainShard.Shard:X16}");
            await TestContext.Out.WriteLineAsync($"  Total found: {blockTransactions.Transactions.Count}");
            await TestContext.Out.WriteLineAsync($"  Incomplete: {blockTransactions.Incomplete}");

            foreach (BlockTransaction tx in blockTransactions.Transactions.Take(5))
            {
                Assert.That(tx.Account, Is.Not.Null);
                Assert.Multiple(() =>
                {
                    Assert.That(tx.Account.Hash.Length, Is.EqualTo(32));
                    Assert.That(tx.Lt, Is.GreaterThan(0), "Logical time should be positive");
                    Assert.That(tx.Hash, Is.Not.Null);
                });
                Assert.That(tx.Hash.Length, Is.EqualTo(32));

                await TestContext.Out.WriteLineAsync($"    Tx: account={tx.Account}, " +
                                                     $"lt={tx.Lt}, hash={Convert.ToHexString(tx.Hash).Substring(0, 16)}...");
            }
        }
        else
        {
            await TestContext.Out.WriteLineAsync($"No transactions in this shard block (seqno {workchainShard.Seqno})");
        }
    }

    [Test]
    public async Task FullWorkflow_ShouldSucceed()
    {
        await TestContext.Out.WriteLineAsync("=== Starting Full Lite Client Workflow Test ===\n");

        // Step 1: Get server time
        await TestContext.Out.WriteLineAsync("Step 1: Getting server time...");
        DateTimeOffset serverTime = await client.GetTimeAsync();
        Assert.That(serverTime, Is.GreaterThan(DateTimeOffset.MinValue));
        await TestContext.Out.WriteLineAsync($"  Server time: {serverTime:yyyy-MM-dd HH:mm:ss} UTC\n");

        // Step 2: Get server version
        await TestContext.Out.WriteLineAsync("Step 2: Getting server version...");
        (int Version, long Capabilities, int Now) version = await client.GetVersionAsync();
        await TestContext.Out.WriteLineAsync($"  Version: {version.Version}");
        await TestContext.Out.WriteLineAsync($"  Capabilities: {version.Capabilities}\n");

        // Step 3: Get masterchain info
        await TestContext.Out.WriteLineAsync("Step 3: Getting masterchain info...");
        MasterchainInfo masterchainInfo = await client.GetMasterchainInfoAsync();
        Assert.That(masterchainInfo.Last.Seqno, Is.GreaterThan(0u));
        await TestContext.Out.WriteLineAsync($"  Latest block: {masterchainInfo.Last.Seqno}");
        await TestContext.Out.WriteLineAsync($"  Workchain: {masterchainInfo.Last.Workchain}");
        await TestContext.Out.WriteLineAsync($"  Shard: {masterchainInfo.Last.Shard:X16}\n");

        // Step 4: Lookup an older block
        await TestContext.Out.WriteLineAsync("Step 4: Looking up older block...");
        uint oldSeqno = masterchainInfo.Last.Seqno - 10;
        BlockId oldBlock = await client.LookupBlockAsync(-1, long.MinValue, oldSeqno);
        Assert.That(oldBlock.Seqno, Is.EqualTo(oldSeqno));
        await TestContext.Out.WriteLineAsync($"  Found block {oldSeqno}");
        await TestContext.Out.WriteLineAsync($"  Root hash: {oldBlock.RootHashHex.Substring(0, 32)}...\n");

        // Step 5: Get block header
        await TestContext.Out.WriteLineAsync("Step 5: Getting block header...");
        BlockHeader header = await client.GetBlockHeaderAsync(masterchainInfo.Last);
        Assert.That(header, Is.Not.Null);
        Assert.That(header.HeaderProof, Is.Not.Null);
        Assert.That(header.HeaderProof.Length, Is.GreaterThan(0));
        await TestContext.Out.WriteLineAsync($"  Block ID: {header.Id.Seqno}");
        await TestContext.Out.WriteLineAsync($"  Mode: {header.Mode}");
        await TestContext.Out.WriteLineAsync($"  Proof size: {header.HeaderProof.Length} bytes\n");

        // Step 6: Get all shards
        await TestContext.Out.WriteLineAsync("Step 6: Getting all shards...");
        BlockId[] shards = await client.GetAllShardsInfoAsync(masterchainInfo.Last);
        Assert.That(shards.Length, Is.GreaterThan(0));
        await TestContext.Out.WriteLineAsync($"  Found {shards.Length} shard(s)");

        BlockId[] wc0Shards = shards.Where(s => s.Workchain == 0).ToArray();
        Assert.That(wc0Shards.Length, Is.GreaterThan(0), "Should have workchain 0 shards");
        await TestContext.Out.WriteLineAsync($"  Workchain 0 shards: {wc0Shards.Length}\n");

        // Step 7: List transactions from a workchain shard
        await TestContext.Out.WriteLineAsync("Step 7: Listing transactions from workchain shard...");
        BlockId shard = wc0Shards[0];
        BlockTransactions transactions = await client.ListBlockTransactionsAsync(shard, 20);
        Assert.That(transactions, Is.Not.Null);
        await TestContext.Out.WriteLineAsync($"  Shard: {shard.Shard:X16}");
        await TestContext.Out.WriteLineAsync($"  Block seqno: {shard.Seqno}");
        await TestContext.Out.WriteLineAsync($"  Transactions found: {transactions.Transactions.Count}");
        await TestContext.Out.WriteLineAsync($"  Incomplete: {transactions.Incomplete}\n");

        await TestContext.Out.WriteLineAsync("=== Full Workflow Test Completed Successfully ===");
    }

    [Test]
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

        Assert.Multiple(() =>
        {
            Assert.That(time1, Is.GreaterThan(DateTimeOffset.MinValue));
            Assert.That(version.Version, Is.GreaterThan(0));
            Assert.That(masterchainInfo.Last.Seqno, Is.GreaterThan(0u));
            Assert.That(time2, Is.GreaterThan(DateTimeOffset.MinValue));
            Assert.That(time3, Is.GreaterThan(DateTimeOffset.MinValue));
        });

        await TestContext.Out.WriteLineAsync("Concurrent requests completed:");
        await TestContext.Out.WriteLineAsync($"  Time 1: {time1:yyyy-MM-dd HH:mm:ss} UTC");
        await TestContext.Out.WriteLineAsync($"  Version: {version.Version}");
        await TestContext.Out.WriteLineAsync($"  Masterchain seqno: {masterchainInfo.Last.Seqno}");
        await TestContext.Out.WriteLineAsync($"  Time 2: {time2:yyyy-MM-dd HH:mm:ss} UTC");
        await TestContext.Out.WriteLineAsync($"  Time 3: {time3:yyyy-MM-dd HH:mm:ss} UTC");
    }

    #region RunMethodAsync Tests

    [Test]
    public async Task RunMethod_GetSeqno_ShouldReturnNumber()
    {
        // Arrange - Durov's wallet
        Address address = Address.Parse("UQDYzZmfsrGzhObKJUw4gzdeIxEai3jAFbiGKGwxvxHinf4K");
        MasterchainInfo info = await client.GetMasterchainInfoAsync();

        // Act
        RunMethodResult result = await client.RunMethodAsync(
            info.Last,
            address,
            "seqno",
            []
        );

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(0));
            Assert.That(result.Stack.Remaining, Is.GreaterThan(0), "Stack should have at least one value");
        });

        BigInteger seqno = result.Stack.ReadBigInteger();
        Assert.That(seqno >= 0, Is.True, "Seqno should be non-negative");

        await TestContext.Out.WriteLineAsync($"Durov's wallet seqno: {seqno}");
        await TestContext.Out.WriteLineAsync($"Exit code: {result.ExitCode}");
        await TestContext.Out.WriteLineAsync($"Gas used: {result.GasUsed}");
    }

    [Test]
    public async Task RunMethod_GetPublicKey_ShouldReturnPublicKey()
    {
        // Arrange - Durov's wallet
        Address address = Address.Parse("UQDYzZmfsrGzhObKJUw4gzdeIxEai3jAFbiGKGwxvxHinf4K");
        MasterchainInfo info = await client.GetMasterchainInfoAsync();

        // Act
        RunMethodResult result = await client.RunMethodAsync(
            info.Last,
            address,
            "get_public_key",
            []
        );

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.ExitCode, Is.EqualTo(0));
            Assert.That(result.Stack.Remaining, Is.GreaterThan(0));
        });

        BigInteger publicKey = result.Stack.ReadBigInteger();
        Assert.That(publicKey, Is.Not.EqualTo(BigInteger.Zero));

        await TestContext.Out.WriteLineAsync($"Public key (as BigInt): {publicKey}");
        await TestContext.Out.WriteLineAsync($"Public key length: {publicKey.GetByteCount()} bytes");
    }

    [Test]
    public async Task RunMethod_NonExistentMethod_ShouldReturnNonZeroExitCode()
    {
        // Arrange - Durov's wallet
        Address address = Address.Parse("UQDYzZmfsrGzhObKJUw4gzdeIxEai3jAFbiGKGwxvxHinf4K");
        MasterchainInfo info = await client.GetMasterchainInfoAsync();

        // Act
        RunMethodResult result = await client.RunMethodAsync(
            info.Last,
            address,
            "non_existent_method_12345",
            []
        );

        // Assert - Method not found typically returns exit code 11
        Assert.That(result, Is.Not.Null);
        Assert.That(result.ExitCode, Is.Not.EqualTo(0));

        await TestContext.Out.WriteLineAsync($"Non-existent method exit code: {result.ExitCode}");
    }

    #endregion

    #region GetConfigAsync Tests

    [Test]
    public async Task GetConfig_ShouldReturnValidConfig()
    {
        // Arrange
        MasterchainInfo info = await client.GetMasterchainInfoAsync();

        // Act
        ConfigInfo config = await client.GetConfigAsync(info.Last);

        // Assert
        Assert.That(config, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(config.Block, Is.Not.Null);
            Assert.That(config.StateProof.Length, Is.GreaterThan(0), "StateProof should not be empty");
            Assert.That(config.ConfigProof.Length, Is.GreaterThan(0), "ConfigProof should not be empty");
        });

        await TestContext.Out.WriteLineAsync($"Config for block {config.Block.Seqno}:");
        await TestContext.Out.WriteLineAsync($"  StateProof size: {config.StateProof.Length} bytes");
        await TestContext.Out.WriteLineAsync($"  ConfigProof size: {config.ConfigProof.Length} bytes");

        // Verify we can parse proofs as BOC
        Cell[] stateProof = Cell.FromBoc(config.StateProof);
        Cell[] configProof = Cell.FromBoc(config.ConfigProof);

        Assert.Multiple(() =>
        {
            Assert.That(stateProof.Length, Is.GreaterThan(0), "Should be able to parse StateProof as BOC");
            Assert.That(configProof.Length, Is.GreaterThan(0), "Should be able to parse ConfigProof as BOC");
        });

        await TestContext.Out.WriteLineAsync($"  StateProof cells: {stateProof.Length}");
        await TestContext.Out.WriteLineAsync($"  ConfigProof cells: {configProof.Length}");
    }

    #endregion

    #region Lookup Method Tests

    [Test]
    public async Task LookupBlockByUtime_ShouldFindBlock()
    {
        // Arrange - Get current time and go back 5 minutes
        DateTimeOffset now = await client.GetTimeAsync();
        int utime = (int)now.ToUnixTimeSeconds() - 300; // 5 minutes ago

        // Act
        BlockId block = await client.LookupBlockByUtimeAsync(-1, long.MinValue, utime);

        // Assert
        Assert.That(block, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(block.Workchain, Is.EqualTo(-1));
            Assert.That(block.Shard, Is.EqualTo(long.MinValue));
            Assert.That(block.Seqno, Is.GreaterThan(0u));
            Assert.That(block.RootHash, Is.Not.Null);
        });
        Assert.That(block.RootHash.Length, Is.EqualTo(32));

        await TestContext.Out.WriteLineAsync($"Found block by utime {utime}:");
        await TestContext.Out.WriteLineAsync($"  Seqno: {block.Seqno}");
        await TestContext.Out.WriteLineAsync($"  Root Hash: {block.RootHashHex}");
    }

    [Test]
    public async Task LookupBlockByLt_ShouldFindBlock()
    {
        // Arrange - Get a recent transaction's LT
        Address address = Address.Parse("UQDYzZmfsrGzhObKJUw4gzdeIxEai3jAFbiGKGwxvxHinf4K");
        MasterchainInfo info = await client.GetMasterchainInfoAsync();
        AccountState state = await client.GetAccountStateAsync(address, info.Last);

        // Skip if no transactions
        if (state.LastTransaction == null)
        {
            await TestContext.Out.WriteLineAsync("No transactions found, skipping test");
            return;
        }

        long lt = (long)state.LastTransaction.Lt;

        // Act
        BlockId block = await client.LookupBlockByLtAsync(
            address.Workchain,
            long.MinValue, // Will be resolved by server
            lt
        );

        // Assert
        Assert.That(block, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(block.Workchain, Is.EqualTo(address.Workchain));
            Assert.That(block.Seqno, Is.GreaterThan(0u));
        });

        await TestContext.Out.WriteLineAsync($"Found block by LT {lt}:");
        await TestContext.Out.WriteLineAsync($"  Workchain: {block.Workchain}");
        await TestContext.Out.WriteLineAsync($"  Shard: {block.Shard:X16}");
        await TestContext.Out.WriteLineAsync($"  Seqno: {block.Seqno}");
    }

    #endregion

    #region GetMasterchainInfoExt Tests

    [Test]
    public async Task GetMasterchainInfoExt_ShouldReturnExtendedInfo()
    {
        // Act
        MasterchainInfoExt infoExt = await client.GetMasterchainInfoExtAsync();

        // Assert
        Assert.That(infoExt, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(infoExt.Version, Is.GreaterThan(0), "Version should be positive");
            Assert.That(infoExt.Capabilities >= 0, Is.True, "Capabilities should be non-negative");
            Assert.That(infoExt.Last, Is.Not.Null);
        });
        Assert.Multiple(() =>
        {
            Assert.That(infoExt.Last.Workchain, Is.EqualTo(-1));
            Assert.That(infoExt.Last.Seqno, Is.GreaterThan(0u));
            Assert.That(infoExt.LastUtime, Is.GreaterThan(0), "LastUtime should be positive");
        });

        await TestContext.Out.WriteLineAsync("Extended Masterchain Info:");
        await TestContext.Out.WriteLineAsync($"  Version: {infoExt.Version}");
        await TestContext.Out.WriteLineAsync($"  Capabilities: {infoExt.Capabilities}");
        await TestContext.Out.WriteLineAsync($"  Last block: {infoExt.Last.Seqno}");
        await TestContext.Out.WriteLineAsync($"  Last utime: {infoExt.LastUtime}");

        // Verify timestamp is recent (within last hour)
        long currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Assert.Multiple(() =>
        {
            Assert.That(infoExt.LastUtime, Is.LessThanOrEqualTo(currentTime), "LastUtime should not be in the future");
            Assert.That(infoExt.LastUtime, Is.GreaterThan(currentTime - 3600), "LastUtime should be recent (within 1 hour)");
        });
    }

    #endregion

    #region Error Handling Tests

    [Test]
    public async Task GetAccountState_NonExistentAccount_ShouldReturnUninitializedState()
    {
        // Arrange - Generate random address that likely doesn't exist
        byte[] hash = new byte[32];
        Random.Shared.NextBytes(hash);
        Address address = new(0, hash);

        MasterchainInfo info = await client.GetMasterchainInfoAsync();

        // Act
        AccountState state = await client.GetAccountStateAsync(address, info.Last);

        // Assert
        Assert.That(state, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(state.State, Is.EqualTo(AccountStorageState.Uninitialized));
            Assert.That(state.Balance, Is.EqualTo(BigInteger.Zero));
            Assert.That(state.Code, Is.Null);
            Assert.That(state.Data, Is.Null);
            Assert.That(state.LastTransaction, Is.Null);
        });

        await TestContext.Out.WriteLineAsync($"Non-existent account ({address}):");
        await TestContext.Out.WriteLineAsync($"  State: {state.State}");
        await TestContext.Out.WriteLineAsync($"  Balance: {state.Balance}");
    }

    [Test]
    public async Task GetAccountTransactions_NonExistentAccount_ShouldReturnEmptyList()
    {
        // Arrange - Generate random address
        byte[] hash = new byte[32];
        Random.Shared.NextBytes(hash);
        Address address = new(0, hash);

        // Act
        AccountTransactions transactions = await client.GetAccountTransactionsAsync(
            address,
            10,
            0, // LT = 0
            new byte[32] // Empty hash
        );

        // Assert
        Assert.That(transactions, Is.Not.Null);
        Assert.That(transactions.Transactions, Is.Not.Null);
        Assert.That(transactions.Transactions, Is.Empty);

        await TestContext.Out.WriteLineAsync($"Non-existent account transactions: {transactions.Transactions.Count}");
    }

    #endregion

    #region Provider GetAsync Tests

    [Test]
    public async Task Provider_GetAsync_ShouldExecuteSeqno()
    {
        // Arrange - Durov's wallet
        Address address = Address.Parse("UQDYzZmfsrGzhObKJUw4gzdeIxEai3jAFbiGKGwxvxHinf4K");
        IContractProvider provider = client.Provider(address);

        // Act
        ContractGetMethodResult result = await provider.GetAsync("seqno", []);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Stack, Is.Not.Null);
        Assert.That(result.Stack.Remaining, Is.GreaterThan(0));

        BigInteger seqno = result.Stack.ReadBigInteger();
        Assert.That(seqno >= 0, Is.True);

        await TestContext.Out.WriteLineAsync($"Provider GetAsync seqno result: {seqno}");
        await TestContext.Out.WriteLineAsync($"  Gas used: {result.GasUsed}");
    }

    [Test]
    public async Task Provider_GetAsync_InvalidMethod_ShouldThrowComputeError()
    {
        // Arrange - Durov's wallet
        Address address = Address.Parse("UQDYzZmfsrGzhObKJUw4gzdeIxEai3jAFbiGKGwxvxHinf4K");
        IContractProvider provider = client.Provider(address);

        // Act & Assert
        Assert.ThrowsAsync<ComputeError>(async () =>
        {
            await provider.GetAsync("invalid_method_xyz", []);
        });

        await TestContext.Out.WriteLineAsync("Invalid method correctly threw ComputeError");
    }

    #endregion

    #region Pagination Tests

    [Test]
    public async Task ListBlockTransactions_WithPagination_ShouldRetrieveMultiplePages()
    {
        // Arrange - Find a busy workchain block
        MasterchainInfo info = await client.GetMasterchainInfoAsync();
        BlockId[] shards = await client.GetAllShardsInfoAsync(info.Last);
        BlockId? shard = shards.FirstOrDefault(s => s.Workchain == 0);

        if (shard == null)
        {
            await TestContext.Out.WriteLineAsync("No workchain 0 shards found, skipping test");
            return;
        }

        // Act - First page
        BlockTransactions page1 = await client.ListBlockTransactionsAsync(shard, 10);

        await TestContext.Out.WriteLineAsync($"Block transactions pagination test (block {shard.Seqno}):");
        await TestContext.Out.WriteLineAsync($"  Page 1: {page1.Transactions.Count} transactions");
        await TestContext.Out.WriteLineAsync($"  Incomplete: {page1.Incomplete}");

        // If incomplete and we have transactions, try second page
        if (page1.Incomplete && page1.Transactions.Count > 0)
        {
            BlockTransaction lastTx = page1.Transactions.Last();
            LiteServerTransactionId3 after = new()
            {
                Account = lastTx.Account.Hash,
                Lt = (long)lastTx.Lt
            };

            // Act - Second page
            BlockTransactions page2 = await client.ListBlockTransactionsAsync(shard, 10, after);

            // Assert
            Assert.That(page2, Is.Not.Null);
            await TestContext.Out.WriteLineAsync($"  Page 2: {page2.Transactions.Count} transactions");

            // Verify no overlap
            if (page1.Transactions.Count > 0 && page2.Transactions.Count > 0)
            {
                BlockTransaction lastOfPage1 = page1.Transactions.Last();
                BlockTransaction firstOfPage2 = page2.Transactions.First();

                // They should be different
                bool different = lastOfPage1.Lt != firstOfPage2.Lt ||
                                 !lastOfPage1.Account.Equals(firstOfPage2.Account);

                Assert.That(different, Is.True, "Pages should not overlap");
            }
        }
        else
        {
            await TestContext.Out.WriteLineAsync("  Block has all transactions in first page or is empty");
        }
    }

    [Test]
    public async Task GetAccountTransactions_WithLimit_ShouldRespectLimit()
    {
        // Arrange - Durov's wallet
        Address address = Address.Parse("UQDYzZmfsrGzhObKJUw4gzdeIxEai3jAFbiGKGwxvxHinf4K");
        MasterchainInfo info = await client.GetMasterchainInfoAsync();
        AccountState state = await client.GetAccountStateAsync(address, info.Last);

        // Skip if no transactions
        if (state.LastTransaction == null)
        {
            await TestContext.Out.WriteLineAsync("No transactions found, skipping test");
            return;
        }

        // Act - Request only 5 transactions
        AccountTransactions transactions = await client.GetAccountTransactionsAsync(
            address,
            5,
            state.LastTransaction.Lt,
            state.LastTransaction.Hash
        );

        // Assert
        Assert.That(transactions, Is.Not.Null);
        Assert.That(transactions.Transactions, Is.Not.Null);
        Assert.That(transactions.Transactions.Count, Is.LessThanOrEqualTo(5), "Should not exceed requested count");

        await TestContext.Out.WriteLineAsync($"Requested 5 transactions, got: {transactions.Transactions.Count}");

        // Verify transactions are in descending LT order
        for (int i = 0; i < transactions.Transactions.Count - 1; i++)
        {
            Assert.That(
                transactions.Transactions[i].Lt >= transactions.Transactions[i + 1].Lt, Is.True,
                "Transactions should be in descending LT order"
            );
        }
    }

    [Test]
    public async Task Provider_GetTransactionsAsync_ShouldPaginateCorrectly()
    {
        // Arrange - Durov's wallet
        Address address = Address.Parse("UQDYzZmfsrGzhObKJUw4gzdeIxEai3jAFbiGKGwxvxHinf4K");
        IContractProvider provider = client.Provider(address);

        // Get account state first
        ContractState state = await provider.GetStateAsync();

        // Skip if no transactions
        if (state.Last == null)
        {
            await TestContext.Out.WriteLineAsync("No transactions found, skipping test");
            return;
        }

        // Act - Get last 10 transactions
        Transaction[] transactions = await provider.GetTransactionsAsync(
            address,
            state.Last.Lt,
            state.Last.Hash,
            10
        );

        // Assert
        Assert.That(transactions, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(transactions.Length, Is.LessThanOrEqualTo(10), "Should respect limit");
            Assert.That(transactions.Length, Is.GreaterThan(0), "Should have at least one transaction");
        });

        await TestContext.Out.WriteLineAsync($"Provider GetTransactionsAsync returned {transactions.Length} transactions");

        // Verify no duplicates
        HashSet<(BigInteger Lt, string Hash)> seen = [];
        foreach (Transaction tx in transactions)
        {
            string hashHex = Convert.ToHexString(tx.Hash());
            bool added = seen.Add((tx.Lt, hashHex));
            Assert.That(added, Is.True, $"Found duplicate transaction: LT={tx.Lt}, Hash={hashHex}");
        }

        await TestContext.Out.WriteLineAsync("  No duplicate transactions found");
    }

    #endregion
}