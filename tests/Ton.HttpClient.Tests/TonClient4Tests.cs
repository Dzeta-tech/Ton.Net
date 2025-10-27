using System.Numerics;
using NUnit.Framework;
using Ton.Core.Addresses;

namespace Ton.HttpClient.Tests;

[TestFixture]
[NonParallelizable]
public class TonClient4Tests
{
    private TonClient4 _client = null!;
    private Address _testAddress = null!;
    private int _seqno;

    [SetUp]
    public async Task Setup()
    {
        _client = new TonClient4(new TonClient4Parameters
        {
            Endpoint = "https://mainnet-v4.tonhubapi.com"
        });
        
        _testAddress = Address.Parse("EQBicYUqh1j9Lnqv9ZhECm0XNPaB7_HcwoBb3AJnYYfqB38_");
        
        // Get last block for seqno
        var lastBlock = await _client.GetLastBlockAsync();
        _seqno = lastBlock.Last.Seqno;
        
        await Task.Delay(500); // Rate limiting
    }

    [TearDown]
    public async Task TearDown()
    {
        await Task.Delay(500); // Rate limiting
        _client.Dispose();
    }

    [Test]
    public async Task Test_GetAccountWithTransactions()
    {
        // Get account information
        var account = await _client.GetAccountAsync(_seqno, _testAddress);
        var accountLite = await _client.GetAccountLiteAsync(_seqno, _testAddress);

        Assert.That(account, Is.Not.Null);
        Assert.That(accountLite, Is.Not.Null);
        Assert.That(account.Account, Is.Not.Null);
        Assert.That(accountLite.Account, Is.Not.Null);

        // Check that both return similar data
        Assert.That(account.Account.Balance.Coins, Is.EqualTo(accountLite.Account.Balance.Coins));
        
        // Get transactions if account has any
        if (accountLite.Account.Last != null)
        {
            var lt = BigInteger.Parse(accountLite.Account.Last.Lt);
            var hash = Convert.FromBase64String(accountLite.Account.Last.Hash);
            
            var transactions = await _client.GetAccountTransactionsAsync(_testAddress, lt, hash);
            Assert.That(transactions, Is.Not.Null);
            
            var isChanged = await _client.IsAccountChangedAsync(_seqno, _testAddress, lt);
            Assert.That(isChanged, Is.Not.Null);
        }
    }

    [Test]
    public async Task Test_GetAccountParsedTransactions()
    {
        var accountLite = await _client.GetAccountLiteAsync(_seqno, _testAddress);
        Assert.That(accountLite, Is.Not.Null);
        Assert.That(accountLite.Account, Is.Not.Null);

        if (accountLite.Account.Last != null)
        {
            var lt = BigInteger.Parse(accountLite.Account.Last.Lt);
            var hash = Convert.FromBase64String(accountLite.Account.Last.Hash);
            
            // GetAccountTransactionsAsync returns parsed transactions
            var transactions = await _client.GetAccountTransactionsAsync(_testAddress, lt, hash);
            
            Assert.That(transactions, Is.Not.Null);
            
            if (transactions.Count > 0)
            {
                Assert.That(transactions[0].Transaction, Is.Not.Null);
                Assert.That(transactions[0].Block, Is.Not.Null);
            }
        }
    }

    [Test]
    public async Task Test_GetConfig()
    {
        var config = await _client.GetConfigAsync(_seqno);
        
        Assert.That(config, Is.Not.Null);
        Assert.That(config.Config, Is.Not.Null);
        Assert.That(config.Config.Cell, Is.Not.Null);
    }

    [Test]
    public async Task Test_GetBlock()
    {
        var block = await _client.GetBlockAsync(_seqno);
        
        Assert.That(block, Is.Not.Null);
        Assert.That(block.Shards, Is.Not.Null);
    }

    [Test]
    public async Task Test_GetExtraCurrencyInfo()
    {
        var testAddresses = new[]
        {
            "-1:0000000000000000000000000000000000000000000000000000000000000000",
            "0:C4CAC12F5BC7EEF4CF5EC84EE68CCF860921A06CA0395EC558E53E37B13C3B08",
            "0:F5FFA780ACEE2A41663C1E32F50D771327275A42FC9D3FAB4F4D9CDE11CCA897"
        }.Select(Address.Parse).ToArray();

        var knownEc = new[] { "239", "4294967279" };
        var expectedEc = new Dictionary<string, BigInteger>[]
        {
            new() { ["239"] = BigInteger.Parse("663333333334"), ["4294967279"] = BigInteger.Parse("998444444446") },
            new() { ["239"] = BigInteger.Parse("989097920") },
            new() { ["239"] = BigInteger.Parse("666666666"), ["4294967279"] = BigInteger.Parse("777777777") }
        };

        for (int i = 0; i < testAddresses.Length; i++)
        {
            await Task.Delay(500); // Rate limiting
            
            var res = await _client.GetAccountAsync(_seqno, testAddresses[i]);
            var resLite = await _client.GetAccountLiteAsync(_seqno, testAddresses[i]);
            var expected = expectedEc[i];

            foreach (var testEc in knownEc)
            {
                if (expected.TryGetValue(testEc, out var expCur))
                {
                    if (res.Account.Balance.Currencies != null && 
                        res.Account.Balance.Currencies.TryGetValue(testEc, out var resCur))
                    {
                        Assert.That(BigInteger.Parse(resCur), Is.EqualTo(expCur), 
                            $"Account full: Currency {testEc} mismatch for address {i}");
                    }
                    
                    if (resLite.Account.Balance.Currencies != null && 
                        resLite.Account.Balance.Currencies.TryGetValue(testEc, out var resLiteCur))
                    {
                        Assert.That(BigInteger.Parse(resLiteCur), Is.EqualTo(expCur), 
                            $"Account lite: Currency {testEc} mismatch for address {i}");
                    }
                }
            }
        }
    }

    [Test]
    public async Task Test_RunMethod()
    {
        // Run seqno method (most contracts have this)
        var (exitCode, reader, resultRaw, block, shardBlock) = await _client.RunMethodAsync(_seqno, _testAddress, "seqno");
        
        Assert.That(exitCode, Is.EqualTo(0).Or.EqualTo(1), "Exit code should be 0 or 1");
        Assert.That(reader, Is.Not.Null);
        Assert.That(block, Is.Not.Null);
    }
}

