# First Steps

This guide walks you through the basics of using Ton.NET.

## Choosing a Client

Ton.NET provides two ways to interact with TON blockchain:

### LiteClient (Recommended)

- **Direct node connection** via ADNL protocol
- **Fast and efficient** - no HTTP overhead
- **Automatic load balancing** across multiple servers
- **Best for production** applications

```csharp
using Ton.LiteClient;

// Automatically connects to TON network from global config
LiteClient client = await LiteClientFactory.CreateFromUrlAsync(
    "https://ton.org/global-config.json"
);
```

### HttpClient (Alternative)

- **HTTP API** via Toncenter or TON HTTP API v4
- **Easier to debug** with standard HTTP tools
- **Good for prototyping** and development
- **May have rate limits** on public endpoints

```csharp
using Ton.HttpClient;

TonClient client = new(new TonClientParameters 
{
    Endpoint = "https://toncenter.com/api/v2",
    ApiKey = "your-api-key" // optional for public endpoint
});
```

## Basic Operations

### Getting Blockchain Info

```csharp
// Get latest masterchain block
MasterchainInfo info = await client.GetMasterchainInfoAsync();
Console.WriteLine($"Latest block seqno: {info.Last.Seqno}");
Console.WriteLine($"Workchain: {info.Last.Workchain}");

// Get extended info with capabilities
MasterchainInfoExt infoExt = await client.GetMasterchainInfoExtAsync();
Console.WriteLine($"Protocol version: {infoExt.Version}");
Console.WriteLine($"Server capabilities: {infoExt.Capabilities}");
Console.WriteLine($"Server time: {DateTimeOffset.FromUnixTimeSeconds(infoExt.Now)}");
```

### Working with Addresses

TON addresses come in two formats:

```csharp
using Ton.Core.Addresses;

// Parse friendly format (base64)
Address friendly = Address.Parse("EQCD39VS5jcptHL8vMjEXrzGaRcCVYto7HUn4bpAOg8xqB2N");

// Parse raw format (workchain:hash)
Address raw = Address.Parse("0:83dfd552e63729b472fcbcc8c45ebcc6691702558b68ec7527e1ba403a0f31a8");

// Both represent the same address
Console.WriteLine(friendly.Equals(raw)); // true

// Convert to different formats
string friendlyStr = friendly.ToString(AddressType.Base64, bounceableTag: true);
string rawStr = friendly.ToString(AddressType.Raw);
```

### Getting Account State

```csharp
Address address = Address.Parse("EQCD39VS5jcptHL8vMjEXrzGaRcCVYto7HUn4bpAOg8xqB2N");
BlockId block = info.Last; // use latest block

AccountState state = await client.GetAccountStateAsync(address, block);

Console.WriteLine($"Address: {state.Address}");
Console.WriteLine($"Balance: {state.BalanceInTon:F4} TON");
Console.WriteLine($"State: {state.State}");
Console.WriteLine($"Is active: {state.IsActive}");
Console.WriteLine($"Is contract: {state.IsContract}");

if (state.Code != null)
{
    Console.WriteLine($"Contract code hash: {Convert.ToHexString(state.Code.Hash(0))}");
}
```

### Looking Up Blocks

```csharp
// Lookup by seqno
BlockId block = await client.LookupBlockAsync(
    workchain: -1,  // masterchain
    shard: -9223372036854775808, // full shard
    seqno: 1000000
);

// Lookup by timestamp
BlockId blockByTime = await client.LookupBlockByUtimeAsync(
    workchain: -1,
    shard: -9223372036854775808,
    utime: 1672531200 // Unix timestamp
);

// Lookup by logical time
BlockId blockByLt = await client.LookupBlockByLtAsync(
    workchain: -1,
    shard: -9223372036854775808,
    lt: 1000000000
);
```

### Listing Transactions in a Block

```csharp
BlockId block = info.Last;
BlockTransactions txs = await client.ListBlockTransactionsAsync(block, count: 10);

Console.WriteLine($"Block has {txs.Transactions.Count} transactions");
Console.WriteLine($"Incomplete: {txs.Incomplete}");

foreach (var tx in txs.Transactions)
{
    Console.WriteLine($"Account: {Convert.ToHexString(tx.AccountHash)}");
    Console.WriteLine($"LT: {tx.Lt}");
    Console.WriteLine($"Hash: {Convert.ToHexString(tx.Hash)}");
}
```

## Error Handling

All methods can throw exceptions. Always use try-catch for production code:

```csharp
try
{
    AccountState state = await client.GetAccountStateAsync(address, block);
    Console.WriteLine($"Balance: {state.BalanceInTon} TON");
}
catch (TimeoutException)
{
    Console.WriteLine("Request timed out");
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"Invalid operation: {ex.Message}");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}
```

## Connection Management

`LiteClient` handles connections automatically:
- Connects on first request
- Reconnects automatically if disconnected
- No manual connection management needed
- Uses `Dispose()` to clean up resources

```csharp
// Use 'using' for automatic disposal
await using LiteClient client = await LiteClientFactory.CreateFromUrlAsync(
    "https://ton.org/global-config.json"
);

// Client automatically connects and manages connection
// ... use client ...

// Automatically disposed at end of scope
```

## Next Steps

- Learn about [Key Concepts](key-concepts.md)
- Explore [LiteClient Guide](../modules/liteclient/overview.md)
- Work with [Cells and BOC](../modules/core/overview.md)
- Create [Wallets and Send Transactions](../modules/contracts/overview.md)
