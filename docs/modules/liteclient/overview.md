# LiteClient Module

The `Ton.LiteClient` module provides direct access to TON blockchain via the ADNL protocol. It's the recommended way for production applications.

## Features

- ✅ **Direct node communication** via ADNL protocol
- ✅ **Automatic load balancing** across multiple servers
- ✅ **Automatic reconnection** on connection loss
- ✅ **Query retry logic** for reliability
- ✅ **Type-safe API** with frontend models
- ✅ **No rate limits** (depends on node capacity)

## Quick Start

```csharp
using Ton.LiteClient;
using Ton.Core.Addresses;

// Connect to TON mainnet
LiteClient client = await LiteClientFactory.CreateFromUrlAsync(
    "https://ton.org/global-config.json"
);

// Get latest block
MasterchainInfo info = await client.GetMasterchainInfoAsync();
Console.WriteLine($"Latest block: {info.Last.Seqno}");

// Get account balance
Address address = Address.Parse("EQCD39VS5jcptHL8vMjEXrzGaRcCVYto7HUn4bpAOg8xqB2N");
AccountState state = await client.GetAccountStateAsync(address, info.Last);
Console.WriteLine($"Balance: {state.BalanceInTon} TON");
```

## Factory Methods

### CreateFromUrlAsync

Automatically connects to TON network from config URL:

```csharp
// Mainnet
LiteClient client = await LiteClientFactory.CreateFromUrlAsync(
    "https://ton.org/global-config.json"
);

// Testnet
LiteClient testnet = await LiteClientFactory.CreateFromUrlAsync(
    "https://ton.org/testnet-global.config.json"
);
```

**Behavior:**
- Downloads and parses network config
- If 1 server → creates `LiteSingleEngine`
- If multiple servers → creates `LiteRoundRobinEngine`
- Automatically manages connections

### Create

Connect to a single server:

```csharp
LiteClient client = LiteClientFactory.Create(
    host: "65.109.14.188",
    port: 14432,
    serverPublicKeyBase64: "aF91CuUHuuOv9rm2W5+O/4h38M3sRm40DtZRrwb6fJ4="
);
```

### CreateRoundRobin

Explicitly use multiple servers:

```csharp
var servers = new[]
{
    ("65.109.14.188", 14432, "aF91CuUHuuOv9rm2W5+O/4h38M3sRm40DtZRrwb6fJ4="),
    ("65.21.7.173", 50552, "QnGFe9kihW4YKBq1RQODjpL4f+j4OKvqYPNVXZaSkp0=")
};

LiteClient client = LiteClientFactory.CreateRoundRobin(
    servers.Select(s => (s.Item1, s.Item2, s.Item3)).ToArray(),
    reconnectTimeoutMs: 10000
);
```

## Core Methods

### GetMasterchainInfoAsync

Get latest masterchain state:

```csharp
MasterchainInfo info = await client.GetMasterchainInfoAsync();

Console.WriteLine($"Latest block seqno: {info.Last.Seqno}");
Console.WriteLine($"Workchain: {info.Last.Workchain}");
Console.WriteLine($"Shard: {info.Last.Shard}");
Console.WriteLine($"Root hash: {Convert.ToHexString(info.Last.RootHash)}");
Console.WriteLine($"File hash: {Convert.ToHexString(info.Last.FileHash)}");
```

### GetMasterchainInfoExtAsync

Get extended info with server capabilities:

```csharp
MasterchainInfoExt info = await client.GetMasterchainInfoExtAsync();

Console.WriteLine($"Protocol version: {info.Version}");
Console.WriteLine($"Capabilities: {info.Capabilities}");
Console.WriteLine($"Last block time: {info.LastUtime}");
Console.WriteLine($"Server time: {info.Now}");
```

### GetAccountStateAsync

Get complete account state at specific block:

```csharp
Address address = Address.Parse("EQCD39VS5jcptHL8vMjEXrzGaRcCVYto7HUn4bpAOg8xqB2N");
BlockId block = info.Last;

AccountState state = await client.GetAccountStateAsync(address, block);

// Balance
Console.WriteLine($"Balance: {state.Balance} nanoTON");
Console.WriteLine($"Balance: {state.BalanceInTon:F4} TON");

// State
Console.WriteLine($"State: {state.State}");
Console.WriteLine($"Is active: {state.IsActive}");
Console.WriteLine($"Is contract: {state.IsContract}");

// Code and data (if contract)
if (state.Code != null)
{
    Console.WriteLine($"Code size: {state.Code.Bits.Length} bits");
    Console.WriteLine($"Code hash: {Convert.ToHexString(state.Code.Hash(0))}");
}

if (state.Data != null)
{
    Console.WriteLine($"Data size: {state.Data.Bits.Length} bits");
}

// Last transaction
if (state.LastTransaction != null)
{
    Console.WriteLine($"Last TX LT: {state.LastTransaction.Lt}");
    Console.WriteLine($"Last TX hash: {Convert.ToHexString(state.LastTransaction.Hash)}");
}
```

### LookupBlockAsync

Find block by seqno:

```csharp
BlockId block = await client.LookupBlockAsync(
    workchain: -1,  // -1 = masterchain, 0 = basechain
    shard: -9223372036854775808,  // full shard
    seqno: 1000000
);

Console.WriteLine($"Found block: {block.Seqno}");
```

### LookupBlockByUtimeAsync

Find block by timestamp:

```csharp
int timestamp = (int)DateTimeOffset.UtcNow.AddHours(-1).ToUnixTimeSeconds();

BlockId block = await client.LookupBlockByUtimeAsync(
    workchain: -1,
    shard: -9223372036854775808,
    utime: timestamp
);

Console.WriteLine($"Block at timestamp {timestamp}: seqno {block.Seqno}");
```

### LookupBlockByLtAsync

Find block by logical time:

```csharp
BlockId block = await client.LookupBlockByLtAsync(
    workchain: -1,
    shard: -9223372036854775808,
    lt: 1000000000
);
```

### GetBlockHeaderAsync

Get block header with proof:

```csharp
BlockHeader header = await client.GetBlockHeaderAsync(blockId);

Console.WriteLine($"Block: {header.Id.Seqno}");
Console.WriteLine($"Mode: {header.Mode}");
Console.WriteLine($"Proof size: {header.HeaderProof.Length} bytes");

// Parse proof (MerkleProof cell)
Cell proofCell = Cell.FromBoc(header.HeaderProof)[0];
Cell actualHeader = proofCell.UnwrapProof();  // Extract data from MerkleProof
```

### GetAllShardsInfoAsync

Get all shard blocks for a masterchain block (with full shard description):

```csharp
ShardDescr[] shards = await client.GetAllShardsInfoAsync(masterchainBlock);

Console.WriteLine($"Found {shards.Length} shard blocks:");
foreach (ShardDescr shard in shards)
{
    Console.WriteLine(
        $"  Workchain: {shard.Workchain}, Shard: {shard.Shard}, Seqno: {shard.Seqno}, " +
        $"StartLt: {shard.StartLt}, EndLt: {shard.EndLt}, GenUtime: {shard.GenUtime}");
}
```

### ListBlockTransactionsAsync

List transactions in a block:

```csharp
BlockTransactions txs = await client.ListBlockTransactionsAsync(
    blockId,
    count: 100,  // max transactions to fetch
    after: null   // for pagination
);

Console.WriteLine($"Transactions: {txs.Transactions.Count}");
Console.WriteLine($"Incomplete: {txs.Incomplete}");

foreach (BlockTransaction tx in txs.Transactions)
{
    Console.WriteLine($"Account: {Convert.ToHexString(tx.AccountHash)}");
    Console.WriteLine($"LT: {tx.Lt}");
    Console.WriteLine($"Hash: {Convert.ToHexString(tx.Hash)}");
}

// Pagination
if (txs.Incomplete && txs.Transactions.Count > 0)
{
    var lastTx = txs.Transactions.Last();
    var nextPage = await client.ListBlockTransactionsAsync(
        blockId,
        count: 100,
        after: new LiteServerTransactionId3 { 
            Account = lastTx.AccountHash, 
            Lt = lastTx.Lt 
        }
    );
}
```

### GetConfigAsync

Get blockchain configuration:

```csharp
ConfigInfo config = await client.GetConfigAsync(blockId);

Console.WriteLine($"Config from block: {config.Block.Seqno}");
Console.WriteLine($"State proof size: {config.StateProof.Length}");
Console.WriteLine($"Config proof size: {config.ConfigProof.Length}");

// Parse config from proof
Cell[] cells = Cell.FromBoc(config.ConfigProof);
// ... parse config parameters
```

### GetTimeAsync

Get current server time:

```csharp
DateTimeOffset serverTime = await client.GetTimeAsync();
Console.WriteLine($"Server time: {serverTime}");
```

### GetVersionAsync

Get server version and capabilities:

```csharp
(int version, long capabilities, int now) = await client.GetVersionAsync();

Console.WriteLine($"Version: {version}");
Console.WriteLine($"Capabilities: {capabilities}");
Console.WriteLine($"Server timestamp: {now}");
```

## Engines

### ILiteEngine Interface

All engines implement `ILiteEngine`:

```csharp
public interface ILiteEngine : IDisposable
{
    bool IsReady { get; }
    bool IsClosed { get; }
    
    Task<TResponse> QueryAsync<TRequest, TResponse>(
        TRequest request,
        Func<TLReadBuffer, TResponse> responseReader,
        int timeout = 5000,
        CancellationToken cancellationToken = default
    ) where TRequest : ILiteRequest;
    
    Task CloseAsync();
    
    event EventHandler? Connected;
    event EventHandler? Ready;
    event EventHandler? Closed;
    event EventHandler<Exception>? Error;
}
```

### LiteSingleEngine

Manages connection to a single lite server:

**Features:**
- Automatic connection on first query
- Automatic reconnection on failure
- Query queuing during reconnection
- Resends pending queries after reconnect

**Events:**
```csharp
LiteSingleEngine engine = new("host", 1234, publicKey);

engine.Connected += (s, e) => Console.WriteLine("Connected!");
engine.Ready += (s, e) => Console.WriteLine("Ready!");
engine.Closed += (s, e) => Console.WriteLine("Closed");
engine.Error += (s, ex) => Console.WriteLine($"Error: {ex.Message}");
```

### LiteRoundRobinEngine

Load balances queries across multiple engines:

**Features:**
- Distributes queries round-robin
- Automatically skips unavailable engines
- Retries on failure (up to 200 attempts for no engines, 20 for errors)
- 100ms delay between retries
- Tracks ready engines dynamically

```csharp
ILiteEngine[] engines = {
    new LiteSingleEngine("server1", port1, key1),
    new LiteSingleEngine("server2", port2, key2),
    new LiteSingleEngine("server3", port3, key3)
};

LiteRoundRobinEngine roundRobin = new(engines);

Console.WriteLine($"Total engines: {roundRobin.EngineCount}");
Console.WriteLine($"Ready engines: {roundRobin.ReadyEngineCount}");
```

## Error Handling

### Common Exceptions

```csharp
try
{
    AccountState state = await client.GetAccountStateAsync(address, block);
}
catch (TimeoutException)
{
    // Query timed out (default 5000ms)
    Console.WriteLine("Request timed out");
}
catch (InvalidOperationException ex)
{
    // Engine closed or invalid state
    Console.WriteLine($"Operation failed: {ex.Message}");
}
catch (IOException ex)
{
    // Network/connection error
    Console.WriteLine($"Connection error: {ex.Message}");
}
catch (Exception ex)
{
    // Other errors
    Console.WriteLine($"Error: {ex.Message}");
}
```

### Timeout Configuration

```csharp
// Per-method timeout
AccountState state = await client.GetAccountStateAsync(
    address, 
    block,
    timeout: 10000  // 10 seconds
);

// Cancellation token
CancellationTokenSource cts = new(TimeSpan.FromSeconds(30));
AccountState state = await client.GetAccountStateAsync(
    address,
    block,
    cancellationToken: cts.Token
);
```

## Best Practices

### 1. Use Round-Robin for Production

```csharp
// ✅ Good: Automatic load balancing
LiteClient client = await LiteClientFactory.CreateFromUrlAsync(
    "https://ton.org/global-config.json"
);

// ❌ Avoid: Single point of failure
LiteClient client = LiteClientFactory.Create("single-server", 1234, key);
```

### 2. Reuse Client Instance

```csharp
// ✅ Good: One client per application
public class MyService
{
    private readonly LiteClient _client;
    
    public MyService()
    {
        _client = await LiteClientFactory.CreateFromUrlAsync(...);
    }
}

// ❌ Avoid: Creating client for each request
public async Task GetBalance(Address addr)
{
    using var client = await LiteClientFactory.CreateFromUrlAsync(...);
    // ...
}
```

### 3. Handle Errors Gracefully

```csharp
// ✅ Good: Retry logic
int retries = 3;
for (int i = 0; i < retries; i++)
{
    try
    {
        return await client.GetAccountStateAsync(address, block);
    }
    catch (TimeoutException) when (i < retries - 1)
    {
        await Task.Delay(1000);
    }
}
```

### 4. Dispose Properly

```csharp
// ✅ Good: Using statement
await using LiteClient client = await LiteClientFactory.CreateFromUrlAsync(...);
// ...

// Or: Explicit disposal
LiteClient client = await LiteClientFactory.CreateFromUrlAsync(...);
try
{
    // ...
}
finally
{
    client.Dispose();
}
```

## Network Configs

### Mainnet

```
https://ton.org/global-config.json
```

### Testnet

```
https://ton.org/testnet-global.config.json
```

### Custom Network

```csharp
NetworkConfig config = new()
{
    LiteServers = new List<LiteServerConfig>
    {
        new()
        {
            Ip = 1098487780,  // IP as integer
            Port = 14432,
            Id = new PublicKeyConfig
            {
                Key = "aF91CuUHuuOv9rm2W5+O/4h38M3sRm40DtZRrwb6fJ4="
            }
        }
    }
};

LiteClient client = LiteClientFactory.CreateFromConfig(config);
```

## See Also

- [Getting Started](../../getting-started/installation.md)
- [Key Concepts](../../getting-started/key-concepts.md)
- [Core Module](../core/overview.md)
