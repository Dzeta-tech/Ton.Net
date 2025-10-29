# HttpClient Module

The `Ton.HttpClient` module provides access to TON blockchain via HTTP APIs (Toncenter v2 and TON API v4).

## Features

- ✅ **HTTP/REST API** access
- ✅ **Easy debugging** with standard HTTP tools
- ✅ **Toncenter v2** API support
- ✅ **TON API v4** support
- ✅ **Contract interaction** via IContractProvider
- ✅ **Good for development** and prototyping

## Quick Start

```csharp
using Ton.HttpClient;
using Ton.Core.Addresses;

// Create client (Toncenter v2)
TonClient client = new(new TonClientParameters
{
    Endpoint = "https://toncenter.com/api/v2",
    ApiKey = "your-api-key"  // Optional for public endpoint
});

// Get balance
Address address = Address.Parse("EQCD39VS5jcptHL8vMjEXrzGaRcCVYto7HUn4bpAOg8xqB2N");
BigInteger balance = await client.GetBalanceAsync(address);
Console.WriteLine($"Balance: {(decimal)balance / 1_000_000_000m} TON");
```

## Client Providers

### Toncenter API (v2)

Traditional HTTP API, widely available:

```csharp
TonClient client = new(new TonClientParameters
{
    Endpoint = "https://toncenter.com/api/v2",
    ApiKey = "your-api-key",  // Get from @tonapibot
    Timeout = TimeSpan.FromSeconds(30)
});
```

**Mainnet endpoints:**
- `https://toncenter.com/api/v2` (rate-limited without API key)
- `https://tonapi.io/v2` (alternative)

**Testnet endpoint:**
- `https://testnet.toncenter.com/api/v2`

### TON API v4

Modern API with better performance:

```csharp
TonClient4 client = new(new TonClient4Parameters
{
    Endpoint = "https://mainnet-v4.tonhubapi.com",
    Timeout = TimeSpan.FromSeconds(30)
});
```

**Mainnet endpoints:**
- `https://mainnet-v4.tonhubapi.com`

**Testnet endpoint:**
- `https://testnet-v4.tonhubapi.com`

## Basic Operations

### Get Balance

```csharp
Address address = Address.Parse("EQCD39VS5jcptHL8vMjEXrzGaRcCVYto7HUn4bpAOg8xqB2N");
BigInteger balance = await client.GetBalanceAsync(address);

decimal tonBalance = (decimal)balance / 1_000_000_000m;
Console.WriteLine($"Balance: {tonBalance:F4} TON");
```

### Get Contract State

```csharp
ContractState state = await client.GetContractStateAsync(address);

Console.WriteLine($"Balance: {state.Balance}");
Console.WriteLine($"State: {state.State}");

if (state.State is ContractState.AccountStateInfo.Active active)
{
    byte[]? code = active.Code;
    byte[]? data = active.Data;
    Console.WriteLine("Contract is active");
}

if (state.Last != null)
{
    Console.WriteLine($"Last TX LT: {state.Last.Lt}");
    Console.WriteLine($"Last TX Hash: {Convert.ToBase64String(state.Last.Hash)}");
}
```

### Run Get Method

Execute contract get methods:

```csharp
// Simple get method
RunMethodResult result = await client.RunMethodAsync(
    address,
    methodName: "seqno"
);

int seqno = (int)result.Stack.ReadNumber();
Console.WriteLine($"Seqno: {seqno}");
Console.WriteLine($"Gas used: {result.GasUsed}");
```

### Run Get Method with Parameters

```csharp
// Get method with parameters
TupleItem[] stack = new[]
{
    new TupleItem.TupleItemInt(123)
};

RunMethodResult result = await client.RunMethodAsync(
    address,
    methodName: "get_value",
    stack: stack
);

BigInteger value = result.Stack.ReadNumber();
```

### Get Transactions

```csharp
// Get last 10 transactions
List<Transaction> txs = await client.GetTransactionsAsync(
    address,
    limit: 10
);

foreach (Transaction tx in txs)
{
    Console.WriteLine($"LT: {tx.Lt}");
    Console.WriteLine($"Hash: {Convert.ToHexString(tx.Hash)}");
    Console.WriteLine($"Time: {DateTimeOffset.FromUnixTimeSeconds(tx.Now)}");
    
    if (tx.InMsg != null)
    {
        // Process incoming message
    }
    
    foreach (Message outMsg in tx.OutMsgs)
    {
        // Process outgoing messages
    }
}
```

### Get Transactions with Pagination

```csharp
Address address = Address.Parse("...");
int limit = 100;
string? lt = null;
string? hash = null;

while (true)
{
    List<Transaction> txs = await client.GetTransactionsAsync(
        address,
        limit: limit,
        lt: lt,
        hash: hash
    );
    
    if (txs.Count == 0)
        break;
        
    // Process transactions
    foreach (Transaction tx in txs)
    {
        Console.WriteLine($"LT: {tx.Lt}, Time: {tx.Now}");
    }
    
    // Update pagination parameters
    Transaction lastTx = txs.Last();
    lt = lastTx.Lt.ToString();
    hash = Convert.ToBase64String(lastTx.Hash);
}
```

### Get Single Transaction

```csharp
string lt = "12345678";
string hash = "abcd...";

Transaction? tx = await client.GetTransactionAsync(address, lt, hash);

if (tx != null)
{
    Console.WriteLine($"Found transaction at LT {tx.Lt}");
}
```

### Send Message

```csharp
// Create message (see Contracts module for wallet examples)
Message message = ...;

// Serialize to BOC
Builder builder = Builder.BeginCell();
message.Store(builder);
Cell cell = builder.EndCell();
byte[] boc = cell.ToBoc();

// Send
await client.SendFileAsync(boc);

Console.WriteLine("Message sent successfully");
```

### Check if Contract Deployed

```csharp
bool isDeployed = await client.IsContractDeployedAsync(address);

if (isDeployed)
{
    Console.WriteLine("Contract is deployed and active");
}
else
{
    Console.WriteLine("Contract not deployed or not active");
}
```

## Masterchain Operations

### Get Masterchain Info

```csharp
MasterchainInfoResult info = await client.GetMasterchainInfoAsync();

Console.WriteLine($"Workchain: {info.Workchain}");
Console.WriteLine($"Init seqno: {info.InitSeqno}");
Console.WriteLine($"Latest seqno: {info.LatestSeqno}");
Console.WriteLine($"Shard: {info.Shard}");
```

### Get Shards

```csharp
int seqno = 1000000;
List<ShardInfo> shards = await client.GetWorkchainShardsAsync(seqno);

foreach (ShardInfo shard in shards)
{
    Console.WriteLine($"Workchain: {shard.Workchain}");
    Console.WriteLine($"Shard: {shard.Shard}");
    Console.WriteLine($"Seqno: {shard.Seqno}");
}
```

### Get Shard Transactions

```csharp
int workchain = 0;
int seqno = 1000000;
string shard = "-9223372036854775808";

List<ShardTransactionInfo> txs = await client.GetShardTransactionsAsync(
    workchain,
    seqno,
    shard
);

foreach (ShardTransactionInfo tx in txs)
{
    Console.WriteLine($"Account: {tx.Account}");
    Console.WriteLine($"LT: {tx.Lt}");
    Console.WriteLine($"Hash: {tx.Hash}");
}
```

## Transaction Location

### Locate Result Transaction

Find result transaction from source:

```csharp
Address source = Address.Parse("...");
Address destination = Address.Parse("...");
string createdLt = "12345678";

Transaction resultTx = await client.TryLocateResultTxAsync(
    source,
    destination,
    createdLt
);

Console.WriteLine($"Found result TX: {resultTx.Lt}");
```

### Locate Source Transaction

Find source transaction from destination:

```csharp
Transaction sourceTx = await client.TryLocateSourceTxAsync(
    source,
    destination,
    createdLt
);

Console.WriteLine($"Found source TX: {sourceTx.Lt}");
```

## Contract Interaction

### Open Contract

Use contracts with provider pattern:

```csharp
using Ton.Contracts.Wallets.V5;

// Create wallet
WalletV5R1 wallet = WalletV5R1.Create(0, publicKey);

// Open with provider
OpenedContract<WalletV5R1> openedWallet = client.Open(wallet);

// Use contract methods
int seqno = await openedWallet.Contract.GetSeqnoAsync(openedWallet.Provider);
BigInteger balance = await openedWallet.Contract.GetBalanceAsync(openedWallet.Provider);

Console.WriteLine($"Seqno: {seqno}");
Console.WriteLine($"Balance: {balance}");
```

### Custom Provider

Create provider for any address:

```csharp
IContractProvider provider = client.Provider(address);

// Get state
ContractState state = await provider.GetStateAsync();

// Run get method
ContractGetMethodResult result = await provider.GetAsync("seqno", []);
int seqno = (int)result.Stack.ReadNumber();
```

## Error Handling

### Handle API Errors

```csharp
try
{
    RunMethodResult result = await client.RunMethodAsync(address, "get_data");
}
catch (InvalidOperationException ex) when (ex.Message.Contains("exit_code"))
{
    Console.WriteLine($"Contract execution failed: {ex.Message}");
}
catch (HttpRequestException ex)
{
    Console.WriteLine($"Network error: {ex.Message}");
}
catch (TaskCanceledException)
{
    Console.WriteLine("Request timeout");
}
```

### Run Method with Error Code

Get exit code instead of throwing:

```csharp
RunMethodErrorResult result = await client.RunMethodWithErrorAsync(
    address,
    "may_fail_method"
);

if (result.ExitCode != 0)
{
    Console.WriteLine($"Method failed with exit code: {result.ExitCode}");
}
else
{
    // Process successful result
    BigInteger value = result.Stack.ReadNumber();
}
```

## Best Practices

### 1. Use API Keys

```csharp
// ✅ Good: Use API key for higher rate limits
TonClient client = new(new TonClientParameters
{
    Endpoint = "https://toncenter.com/api/v2",
    ApiKey = Environment.GetEnvironmentVariable("TONCENTER_API_KEY")
});

// ❌ Avoid: No API key (severe rate limits)
```

### 2. Handle Rate Limits

```csharp
// ✅ Good: Implement retry with exponential backoff
async Task<T> RetryAsync<T>(Func<Task<T>> action, int maxRetries = 3)
{
    for (int i = 0; i < maxRetries; i++)
    {
        try
        {
            return await action();
        }
        catch (HttpRequestException) when (i < maxRetries - 1)
        {
            await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, i)));
        }
    }
    throw new Exception("Max retries exceeded");
}
```

### 3. Cache Results

```csharp
// ✅ Good: Cache rarely changing data
private readonly Dictionary<Address, ContractState> stateCache = new();
private readonly TimeSpan cacheTimeout = TimeSpan.FromMinutes(1);

async Task<ContractState> GetCachedStateAsync(Address address)
{
    if (stateCache.TryGetValue(address, out ContractState? cached))
    {
        return cached;
    }
    
    ContractState state = await client.GetContractStateAsync(address);
    stateCache[address] = state;
    return state;
}
```

### 4. Set Appropriate Timeouts

```csharp
// ✅ Good: Configure timeout based on operation
TonClient client = new(new TonClientParameters
{
    Endpoint = "https://toncenter.com/api/v2",
    Timeout = TimeSpan.FromSeconds(30)  // Adjust based on needs
});
```

### 5. Dispose Properly

```csharp
// ✅ Good: Dispose client
using TonClient client = new(parameters);
// ... use client

// Or: Singleton pattern for application lifetime
public class MyService
{
    private readonly TonClient _client;
    
    public MyService()
    {
        _client = new TonClient(parameters);
    }
    
    public void Dispose()
    {
        _client.Dispose();
    }
}
```

## Comparison with LiteClient

| Feature | HttpClient | LiteClient |
|---------|-----------|-----------|
| Protocol | HTTP/REST | ADNL (binary) |
| Speed | Slower | Faster |
| Debugging | Easy | Harder |
| Rate Limits | Yes (API-dependent) | No |
| Load Balancing | Manual | Automatic |
| Reconnection | N/A | Automatic |
| Best For | Development, prototyping | Production |

## See Also

- [LiteClient Module](../liteclient/overview.md) - Production alternative
- [Contracts Module](../contracts/overview.md) - Wallet operations
- [Core Module](../core/overview.md) - Data structures
