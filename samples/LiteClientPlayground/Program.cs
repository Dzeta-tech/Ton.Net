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

// Create lite client
using LiteClient client = LiteClient.Create(host, port, publicKey);

// Wait for connection
TaskCompletionSource readyTask = new();
client.Engine.Ready += (s, e) =>
{
    Console.WriteLine("✅ Connected and ready!\n");
    readyTask.TrySetResult();
};

client.Engine.Error += (s, ex) =>
{
    Console.WriteLine($"❌ Error: {ex.Message}");
    readyTask.TrySetException(ex);
};

// Wait for ready with timeout
using CancellationTokenSource cts = new(TimeSpan.FromSeconds(15));
await Task.WhenAny(readyTask.Task, Task.Delay(-1, cts.Token));

if (!readyTask.Task.IsCompleted)
{
    Console.WriteLine("❌ Connection timeout!");
    return;
}

try
{
    // 1. Get Masterchain Info
    Console.WriteLine("📊 Step 1: Getting masterchain info...");
    MasterchainInfo masterchainInfo = await client.GetMasterchainInfoAsync();

    Console.WriteLine($"   Latest block: seqno {masterchainInfo.Last.Seqno}");
    Console.WriteLine($"   Workchain: {masterchainInfo.Last.Workchain}");
    Console.WriteLine($"   Shard: {masterchainInfo.Last.Shard:X16}");
    Console.WriteLine($"   Root hash: {masterchainInfo.Last.RootHashHex}");
    Console.WriteLine($"   File hash: {masterchainInfo.Last.FileHashHex}");
    Console.WriteLine();

    // 2. Get All Shards for this block
    Console.WriteLine("🔍 Step 2: Getting all shards info...");
    BlockId[] shards = await client.GetAllShardsInfoAsync(masterchainInfo.Last);

    Console.WriteLine($"   Found {shards.Length} shard(s)");
    Console.WriteLine();

    // 3. Get transactions from a workchain 0 shard block
    int totalTransactions = 0;
    BlockId blockToQuery;

    if (shards.Length == 0)
    {
        Console.WriteLine("   No shards found");
        Console.WriteLine("   Falling back to masterchain block...\n");
        blockToQuery = masterchainInfo.Last;
    }
    else
    {
        // Find a workchain 0 shard
        BlockId? workchainShard = shards.FirstOrDefault(s => s.Workchain == 0);
        if (workchainShard != null)
        {
            Console.WriteLine($"   Using workchain 0 shard: {workchainShard.Shard:X16}, seqno:{workchainShard.Seqno}");
            Console.WriteLine();

            // Use the shard block ID directly (already has root_hash and file_hash)
            blockToQuery = workchainShard;
            
            Console.WriteLine($"   Block: wc:{blockToQuery.Workchain}, shard:{blockToQuery.Shard:X16}, seqno:{blockToQuery.Seqno}");
            Console.WriteLine($"   Root hash: {blockToQuery.RootHashHex}");
            Console.WriteLine($"   File hash: {blockToQuery.FileHashHex}");
            Console.WriteLine();
        }
        else
        {
            Console.WriteLine("   No workchain 0 shards found, using masterchain...\n");
            blockToQuery = masterchainInfo.Last;
        }
    }

    // Step 3: List transactions in the selected block
    Console.WriteLine($"📝 Step 3: Listing transactions in block {blockToQuery.Seqno}...");
    var transactions = await client.ListBlockTransactionsAsync(blockToQuery, count: 40);

    Console.WriteLine($"   Block: wc:{transactions.BlockId.Workchain}, shard:{transactions.BlockId.Shard:X16}, seqno:{transactions.BlockId.Seqno}");
    Console.WriteLine($"   Requested: {transactions.RequestedCount}");
    Console.WriteLine($"   Found: {transactions.Transactions.Count}");
    Console.WriteLine($"   Incomplete: {transactions.Incomplete}");
    Console.WriteLine();

    if (transactions.Transactions.Count > 0)
    {
        Console.WriteLine("   Transactions:");
        foreach (var tx in transactions.Transactions.Take(10))
        {
            string accountHex = Convert.ToHexString(tx.Account);
            string hashHex = Convert.ToHexString(tx.Hash);

            Console.WriteLine($"     • Account: {(accountHex.Length > 16 ? "..." + accountHex[^16..] : accountHex)}");
            Console.WriteLine($"       LT: {tx.Lt}");
            Console.WriteLine($"       Hash: {(hashHex.Length > 16 ? hashHex[..16] + "..." : hashHex)}");
            Console.WriteLine();
        }

        if (transactions.Transactions.Count > 10)
            Console.WriteLine($"     ... and {transactions.Transactions.Count - 10} more transactions");
    }
    else
    {
        Console.WriteLine("   No transactions found in this block");
    }

    totalTransactions = transactions.Transactions.Count;

    Console.WriteLine($"✅ Done! Total transactions found: {totalTransactions}");
    Console.WriteLine();
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Error: {ex.Message}");
    Console.WriteLine($"   Stack trace: {ex.StackTrace}");
}

// Keep alive briefly
await Task.Delay(500);