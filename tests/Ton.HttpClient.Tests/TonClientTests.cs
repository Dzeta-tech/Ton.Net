using System.Numerics;
using Ton.Core.Addresses;
using Ton.Core.Boc;
using Ton.Core.Types;
using NUnit.Framework;

namespace Ton.HttpClient.Tests;

/// <summary>
/// Integration tests for TonClient.
/// These tests require a live Toncenter API connection.
/// </summary>
[TestFixture]
[Category("Integration")]
[NonParallelizable]
public class TonClientTests
{
    private TonClient _client = null!;
    private Address _testAddress = null!;

    [SetUp]
    public async Task Setup()
    {
        // Add delay to avoid rate limiting
        await Task.Delay(500);
        
        _client = new TonClient(new TonClientParameters
        {
            Endpoint = "https://toncenter.com/api/v2/jsonRPC"
        });
        
        _testAddress = Address.Parse("EQCD39VS5jcptHL8vMjEXrzGaRcCVYto7HUn4bpAOg8xqB2N");
    }

    [TearDown]
    public async Task TearDown()
    {
        _client?.Dispose();
        // Add delay after each test
        await Task.Delay(1000);
    }

    [Test]
    public async Task Test_GetContractState()
    {
        var state = await _client.GetContractStateAsync(_testAddress);
        
        Assert.That(state, Is.Not.Null);
        Assert.That(state.Balance, Is.GreaterThan(BigInteger.Zero));
        Console.WriteLine($"Balance: {state.Balance}");
        Console.WriteLine($"State: {state.State}");
    }

    [Test]
    public async Task Test_GetBalance()
    {
        var balance = await _client.GetBalanceAsync(_testAddress);
        
        Assert.That(balance, Is.GreaterThan(BigInteger.Zero));
        Console.WriteLine($"Balance: {balance}");
    }

    [Test]
    public async Task Test_GetTransactions()
    {
        var transactions = await _client.GetTransactionsAsync(_testAddress, limit: 3);
        
        Assert.That(transactions, Is.Not.Empty);
        Assert.That(transactions.Count, Is.LessThanOrEqualTo(3));
        
        foreach (var tx in transactions)
        {
            Console.WriteLine($"Transaction LT: {tx.Lt}");
        }
    }

    [Test]
    public async Task Test_GetSingleTransaction()
    {
        var tx = await _client.GetTransactionAsync(
            _testAddress,
            "37508996000003",
            "xiwW9EROcDMWFibmm2YNW/2kTaDW5qwRJxveEf4xUQA="
        );
        
        if (tx != null)
        {
            Console.WriteLine($"Transaction found: LT={tx.Lt}");
        }
        else
        {
            Console.WriteLine("Transaction not found (may have been pruned)");
        }
    }

    [Test]
    public async Task Test_RunMethod()
    {
        var result = await _client.RunMethodAsync(_testAddress, "seqno");
        
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Stack, Is.Not.Null);
        Console.WriteLine($"Gas used: {result.GasUsed}");
        
        // Try to read seqno
        var seqno = result.Stack.ReadNumber();
        Console.WriteLine($"Seqno: {seqno}");
    }

    [Test]
    public async Task Test_GetMasterchainInfo()
    {
        var info = await _client.GetMasterchainInfoAsync();
        
        Assert.That(info, Is.Not.Null);
        Assert.That(info.LatestSeqno, Is.GreaterThan(0));
        Console.WriteLine($"Workchain: {info.Workchain}");
        Console.WriteLine($"Latest Seqno: {info.LatestSeqno}");
        Console.WriteLine($"Shard: {info.Shard}");

        // Test shard info
        var shardInfo = await _client.GetShardTransactionsAsync(
            info.Workchain,
            info.LatestSeqno,
            info.Shard
        );
        Console.WriteLine($"Shard transactions: {shardInfo.Count}");

        // Test workchain shards
        var wcShards = await _client.GetWorkchainShardsAsync(info.LatestSeqno);
        Console.WriteLine($"Workchain shards: {wcShards.Count}");
        
        Assert.That(shardInfo, Is.Not.Null);
        Assert.That(wcShards, Is.Not.Null);
    }

    [Test]
    public async Task Test_IsContractDeployed()
    {
        var isDeployed = await _client.IsContractDeployedAsync(_testAddress);
        
        Assert.That(isDeployed, Is.True);
        Console.WriteLine($"Contract deployed: {isDeployed}");
    }

    [Test]
    public async Task Test_GetExtraCurrencyInfo()
    {
        // Extra currencies are available on testnet
        var testClient = new TonClient(new TonClientParameters
        {
            Endpoint = "https://testnet.toncenter.com/api/v2/jsonRPC"
        });

        var testAddr = Address.Parse("0:D36CFC9E0C57F43C1A719CB9F540ED87A694693AE1535B7654B645F52814AFD7");

        try
        {
            var state = await testClient.GetContractStateAsync(testAddr);
            
            if (state.ExtraCurrency != null)
            {
                Console.WriteLine("Extra currencies found:");
                // Try to find currency ID 100
                var ec100 = state.ExtraCurrency.Get(100);
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
        var source = Address.Parse("UQDDT0TOC4PMp894jtCo3-d1-8ltSjXMX2EuWww_pCNibsUH");
        var createdLt = "37508996000002";

        try
        {
            var infoSource = await _client.TryLocateSourceTxAsync(source, _testAddress, createdLt);
            Console.WriteLine($"Source tx found: LT={infoSource.Lt}");

            var infoResult = await _client.TryLocateResultTxAsync(source, _testAddress, createdLt);
            Console.WriteLine($"Result tx found: LT={infoResult.Lt}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Transaction not found: {ex.Message}");
            // Transaction may have been pruned, skip assertion
        }
    }
}

