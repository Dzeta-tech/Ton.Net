using Ton.LiteClient;
using Ton.LiteClient.Models;

Console.WriteLine("🚀 TON LiteClient Playground\n");

// Server configuration (must be provided as command line arguments)
if (args.Length < 3)
{
    Console.WriteLine("Usage: dotnet run <host> <port> <publicKey>");
    Console.WriteLine("Example: dotnet run 135.181.140.212 13206 uNRRL+6jLpjLRHZfCr2f8CLWQB5vcvI1Wc4NzK8VbFQ=");
    return;
}

string host = args[0];
int port = int.Parse(args[1]);
string publicKey = args[2];

Console.WriteLine($"📡 Connecting to {host}:{port}...\n");

// Create lite client - connection happens automatically on first request
using LiteClient client = LiteClientFactory.Create(host, port, publicKey);

try
{
    // 1. Get Masterchain Info (connection happens automatically here)
    Console.WriteLine("📊 Getting masterchain info...");
    MasterchainInfo masterchainInfo = await client.GetMasterchainInfoAsync();

    Console.WriteLine("✅ Connected and ready!\n");

    Console.WriteLine($"Latest block: seqno {masterchainInfo.Last.Seqno}");
    Console.WriteLine($"Workchain: {masterchainInfo.Last.Workchain}");
    Console.WriteLine($"Shard: {masterchainInfo.Last.Shard:X16}");
    Console.WriteLine($"Root hash: {masterchainInfo.Last.RootHashHex}");
    Console.WriteLine($"File hash: {masterchainInfo.Last.FileHashHex}");
    Console.WriteLine();

    // 2. Get All Shards for this block
    Console.WriteLine("🔍 Getting all shards info...");
    BlockId[] shards = await client.GetAllShardsInfoAsync(masterchainInfo.Last);

    Console.WriteLine($"Found {shards.Length} shard(s)");
    Console.WriteLine();

    // 3. Get transactions from a workchain 0 shard block
    int totalTransactions = 0;
    BlockId blockToQuery;

    if (shards.Length == 0)
    {
        Console.WriteLine("No shards found, using masterchain block...\n");
        blockToQuery = masterchainInfo.Last;
    }
    else
    {
        // Find a workchain 0 shard
        BlockId? workchainShard = shards.FirstOrDefault(s => s.Workchain == 0);
        if (workchainShard != null)
        {
            Console.WriteLine($"Using workchain 0 shard: {workchainShard.Shard:X16}, seqno:{workchainShard.Seqno}\n");
            blockToQuery = workchainShard;
        }
        else
        {
            Console.WriteLine("No workchain 0 shards found, using masterchain...\n");
            blockToQuery = masterchainInfo.Last;
        }
    }

    // List transactions in the selected block
    Console.WriteLine($"📝 Listing transactions in block {blockToQuery.Seqno}...");
    BlockTransactions transactions = await client.ListBlockTransactionsAsync(blockToQuery);

    Console.WriteLine(
        $"Block: wc:{transactions.BlockId.Workchain}, shard:{transactions.BlockId.Shard:X16}, seqno:{transactions.BlockId.Seqno}");
    Console.WriteLine($"Requested: {transactions.RequestedCount}");
    Console.WriteLine($"Found: {transactions.Transactions.Count}");
    Console.WriteLine($"Incomplete: {transactions.Incomplete}");
    Console.WriteLine();

    if (transactions.Transactions.Count > 0)
    {
        Console.WriteLine("Transactions:");
        foreach (BlockTransaction tx in transactions.Transactions.Take(10))
        {
            string accountHex = Convert.ToHexString(tx.Account);
            string hashHex = Convert.ToHexString(tx.Hash);

            Console.WriteLine($"  • Account: {(accountHex.Length > 16 ? "..." + accountHex[^16..] : accountHex)}");
            Console.WriteLine($"    LT: {tx.Lt}");
            Console.WriteLine($"    Hash: {(hashHex.Length > 16 ? hashHex[..16] + "..." : hashHex)}");
            Console.WriteLine();
        }

        if (transactions.Transactions.Count > 10)
            Console.WriteLine($"  ... and {transactions.Transactions.Count - 10} more transactions");
    }
    else
    {
        Console.WriteLine("No transactions found in this block");
    }

    totalTransactions = transactions.Transactions.Count;

    Console.WriteLine($"✅ Done! Total transactions found: {totalTransactions}");
}
catch (TimeoutException ex)
{
    Console.WriteLine($"❌ Connection/Query timeout: {ex.Message}");
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Error: {ex.Message}");
    if (ex.InnerException != null)
        Console.WriteLine($"   Inner: {ex.InnerException.Message}");
}