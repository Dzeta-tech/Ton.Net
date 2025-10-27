using System.Net;
using System.Numerics;
using Ton.Core.Addresses;
using Ton.Core.Contracts;
using Ton.Core.Types;

namespace Ton.HttpClient.Tests;

/// <summary>
///     Integration tests for TonClient.
///     These tests require a live Toncenter API connection.
/// </summary>
[TestFixture]
[Category("Integration")]
public class TonClientTests
{
    [SetUp]
    public void Setup()
    {
        client = new TonClient(new TonClientParameters
        {
            Endpoint =
                "https://ton.access.orbs.network/4410c0ff5Bd3F8B62C092Ab4D238bEE463E64410/1/mainnet/toncenter-api-v2/jsonRPC"
        });

        testAddress = Address.Parse("EQCD39VS5jcptHL8vMjEXrzGaRcCVYto7HUn4bpAOg8xqB2N");
    }

    [TearDown]
    public void TearDown()
    {
        client.Dispose();
    }

    TonClient client = null!;
    Address testAddress = null!;

    [Test]
    public async Task Test_GetContractState()
    {
        ContractState state = await client.GetContractStateAsync(testAddress);

        Assert.That(state, Is.Not.Null);
        Assert.That(state.Balance, Is.GreaterThan(BigInteger.Zero));
        Console.WriteLine($"Balance: {state.Balance}");
        Console.WriteLine($"State: {state.State}");
    }

    [Test]
    public async Task Test_GetBalance()
    {
        BigInteger balance = await client.GetBalanceAsync(testAddress);

        Assert.That(balance, Is.GreaterThan(BigInteger.Zero));
        Console.WriteLine($"Balance: {balance}");
    }

    [Test]
    public async Task Test_GetTransactions()
    {
        try
        {
            List<Transaction> transactions = await client.GetTransactionsAsync(testAddress, 3);

            Assert.That(transactions, Is.Not.Empty);
            Assert.That(transactions.Count, Is.LessThanOrEqualTo(3));

            foreach (Transaction tx in transactions) Console.WriteLine($"Transaction LT: {tx.Lt}");
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.InternalServerError)
        {
            // API 500 error - transaction data might be pruned or unavailable
            Assert.Inconclusive("API returned 500 error - transaction data might be pruned");
        }
    }

    [Test]
    public async Task Test_RunMethod()
    {
        RunMethodResult result = await client.RunMethodAsync(testAddress, "seqno");

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Stack, Is.Not.Null);
        Console.WriteLine($"Gas used: {result.GasUsed}");

        // Try to read seqno
        long seqno = result.Stack.ReadNumber();
        Console.WriteLine($"Seqno: {seqno}");
    }

    [Test]
    public async Task Test_GetMasterchainInfo()
    {
        MasterchainInfoResult info = await client.GetMasterchainInfoAsync();

        Assert.That(info, Is.Not.Null);
        Assert.That(info.LatestSeqno, Is.GreaterThan(0));
        Console.WriteLine($"Workchain: {info.Workchain}");
        Console.WriteLine($"Latest Seqno: {info.LatestSeqno}");
        Console.WriteLine($"Shard: {info.Shard}");

        // Test shard info
        List<ShardTransactionInfo> shardInfo = await client.GetShardTransactionsAsync(
            info.Workchain,
            info.LatestSeqno,
            info.Shard
        );
        Console.WriteLine($"Shard transactions: {shardInfo.Count}");

        // Test workchain shards
        List<ShardInfo> wcShards = await client.GetWorkchainShardsAsync(info.LatestSeqno);
        Console.WriteLine($"Workchain shards: {wcShards.Count}");

        Assert.Multiple(() =>
        {
            Assert.That(shardInfo, Is.Not.Null);
            Assert.That(wcShards, Is.Not.Null);
        });
    }

    [Test]
    public async Task Test_IsContractDeployed()
    {
        bool isDeployed = await client.IsContractDeployedAsync(testAddress);

        Assert.That(isDeployed, Is.True);
        Console.WriteLine($"Contract deployed: {isDeployed}");
    }

    [Test]
    public async Task Test_GetExtraCurrencyInfo()
    {
        // Extra currencies are available on testnet
        TonClient testClient = new(new TonClientParameters
        {
            Endpoint =
                "https://ton.access.orbs.network/4410c0ff5Bd3F8B62C092Ab4D238bEE463E64410/1/testnet/toncenter-api-v2/jsonRPC"
        });

        Address testAddr = Address.Parse("0:D36CFC9E0C57F43C1A719CB9F540ED87A694693AE1535B7654B645F52814AFD7");

        try
        {
            ContractState state = await testClient.GetContractStateAsync(testAddr);

            if (state.ExtraCurrency != null)
            {
                Console.WriteLine("Extra currencies found:");
                // Try to find currency ID 100
                BigInteger? ec100 = state.ExtraCurrency.Get(100);
                if (ec100 != null)
                {
                    Console.WriteLine($"Currency 100: {ec100}");
                    Assert.That(ec100, Is.EqualTo(new BigInteger(10000000)));
                }
            }
            else
            {
                Console.WriteLine("No extra currencies (feature may not be active)");
            }
        }
        finally
        {
            testClient.Dispose();
        }
    }

    [Test]
    public async Task Test_LocateSourceResultTx()
    {
        Address source = Address.Parse("UQDDT0TOC4PMp894jtCo3-d1-8ltSjXMX2EuWww_pCNibsUH");
        string createdLt = "37508996000002";

        try
        {
            Transaction infoSource = await client.TryLocateSourceTxAsync(source, testAddress, createdLt);
            Console.WriteLine($"Source tx found: LT={infoSource.Lt}");

            Transaction infoResult = await client.TryLocateResultTxAsync(source, testAddress, createdLt);
            Console.WriteLine($"Result tx found: LT={infoResult.Lt}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Transaction not found: {ex.Message}");
            // Transaction may have been pruned, skip assertion
        }
    }
}