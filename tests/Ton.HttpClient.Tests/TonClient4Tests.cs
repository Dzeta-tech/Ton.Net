using System.Numerics;
using Ton.Core.Addresses;
using Ton.Core.Tuple;
using Ton.Core.Types;
using Ton.HttpClient.Api.Models;

namespace Ton.HttpClient.Tests;

[TestFixture]
[NonParallelizable]
[Ignore("Integration tests require stable API endpoint")]
public class TonClient4Tests
{
    [SetUp]
    public async Task Setup()
    {
        client = new TonClient4(new TonClient4Parameters
        {
            Endpoint = "https://mainnet-v4.tonhubapi.com"
        });

        testAddress = Address.Parse("EQBicYUqh1j9Lnqv9ZhECm0XNPaB7_HcwoBb3AJnYYfqB38_");

        // Get last block for seqno
        LastBlock lastBlock = await client.GetLastBlockAsync();
        seqno = lastBlock.Last.Seqno;

        await Task.Delay(500); // Rate limiting
    }

    [TearDown]
    public async Task TearDown()
    {
        await Task.Delay(500); // Rate limiting
        client.Dispose();
    }

    TonClient4 client = null!;
    Address testAddress = null!;
    int seqno;

    [Test]
    public async Task Test_GetAccountWithTransactions()
    {
        // Get account information
        AccountInfo account = await client.GetAccountAsync(seqno, testAddress);
        AccountLiteInfo accountLite = await client.GetAccountLiteAsync(seqno, testAddress);

        Assert.Multiple(() =>
        {
            Assert.That(account, Is.Not.Null);
            Assert.That(accountLite, Is.Not.Null);
        });
        
        // Check that both return similar data
        Assert.That(account.Account.Balance.Coins, Is.EqualTo(accountLite.Account.Balance.Coins));

        // Get transactions if account has any
        if (accountLite.Account.Last != null)
        {
            BigInteger lt = BigInteger.Parse(accountLite.Account.Last.Lt);
            byte[] hash = Convert.FromBase64String(accountLite.Account.Last.Hash);

            List<(BlockRef Block, Transaction Transaction)> transactions =
                await client.GetAccountTransactionsAsync(testAddress, lt, hash);
            Assert.That(transactions, Is.Not.Null);

            AccountChanged isChanged = await client.IsAccountChangedAsync(seqno, testAddress, lt);
            Assert.That(isChanged, Is.Not.Null);
        }
    }

    [Test]
    public async Task Test_GetAccountParsedTransactions()
    {
        AccountLiteInfo accountLite = await client.GetAccountLiteAsync(seqno, testAddress);
        Assert.That(accountLite, Is.Not.Null);
        Assert.That(accountLite.Account, Is.Not.Null);

        if (accountLite.Account.Last != null)
        {
            BigInteger lt = BigInteger.Parse(accountLite.Account.Last.Lt);
            byte[] hash = Convert.FromBase64String(accountLite.Account.Last.Hash);

            // GetAccountTransactionsAsync returns parsed transactions
            List<(BlockRef Block, Transaction Transaction)> transactions =
                await client.GetAccountTransactionsAsync(testAddress, lt, hash);

            Assert.That(transactions, Is.Not.Null);

            if (transactions.Count > 0)
            {
                Assert.Multiple(() =>
                {
                    Assert.That(transactions[0].Transaction, Is.Not.Null);
                    Assert.That(transactions[0].Block, Is.Not.Null);
                });
            }
        }
    }

    [Test]
    public async Task Test_GetConfig()
    {
        ConfigResponse config = await client.GetConfigAsync(seqno);

        Assert.That(config, Is.Not.Null);
        Assert.That(config.Config, Is.Not.Null);
        Assert.That(config.Config.Cell, Is.Not.Null);
    }

    [Test]
    public async Task Test_GetBlock()
    {
        BlockDetails block = await client.GetBlockAsync(seqno);

        Assert.That(block, Is.Not.Null);
        Assert.That(block.Shards, Is.Not.Null);
    }

    [Test]
    public async Task Test_GetExtraCurrencyInfo()
    {
        Address[] testAddresses = new[]
        {
            "-1:0000000000000000000000000000000000000000000000000000000000000000",
            "0:C4CAC12F5BC7EEF4CF5EC84EE68CCF860921A06CA0395EC558E53E37B13C3B08",
            "0:F5FFA780ACEE2A41663C1E32F50D771327275A42FC9D3FAB4F4D9CDE11CCA897"
        }.Select(Address.Parse).ToArray();

        string[] knownEc = ["239", "4294967279"];
        Dictionary<string, BigInteger>[] expectedEc =
        [
            new() { ["239"] = BigInteger.Parse("663333333334"), ["4294967279"] = BigInteger.Parse("998444444446") },
            new() { ["239"] = BigInteger.Parse("989097920") },
            new() { ["239"] = BigInteger.Parse("666666666"), ["4294967279"] = BigInteger.Parse("777777777") }
        ];

        for (int i = 0; i < testAddresses.Length; i++)
        {
            await Task.Delay(500); // Rate limiting

            AccountInfo res = await client.GetAccountAsync(seqno, testAddresses[i]);
            AccountLiteInfo resLite = await client.GetAccountLiteAsync(seqno, testAddresses[i]);
            Dictionary<string, BigInteger> expected = expectedEc[i];

            foreach (string testEc in knownEc)
                if (expected.TryGetValue(testEc, out BigInteger expCur))
                {
                    if (res.Account.Balance.Currencies != null &&
                        res.Account.Balance.Currencies.TryGetValue(testEc, out string? resCur))
                        Assert.That(BigInteger.Parse(resCur), Is.EqualTo(expCur),
                            $"Account full: Currency {testEc} mismatch for address {i}");

                    if (resLite.Account.Balance.Currencies != null &&
                        resLite.Account.Balance.Currencies.TryGetValue(testEc, out string? resLiteCur))
                        Assert.That(BigInteger.Parse(resLiteCur), Is.EqualTo(expCur),
                            $"Account lite: Currency {testEc} mismatch for address {i}");
                }
        }
    }

    [Test]
    public async Task Test_RunMethod()
    {
        // Run seqno method (most contracts have this)
        (int exitCode, TupleReader reader, string? resultRaw, BlockRef block, BlockRef shardBlock) =
            await client.RunMethodAsync(seqno, testAddress, "seqno");

        Assert.Multiple(() =>
        {
            Assert.That(exitCode, Is.EqualTo(0).Or.EqualTo(1), "Exit code should be 0 or 1");
            Assert.That(reader, Is.Not.Null);
            Assert.That(block, Is.Not.Null);
        });
    }
}