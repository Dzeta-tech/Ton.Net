# Guides Overview

Practical guides for common TON blockchain tasks using Ton.NET.

## Getting Started

New to Ton.NET? Start here:

1. **[Installation](../getting-started/installation.md)** - Set up Ton.NET in your project
2. **[First Steps](../getting-started/first-steps.md)** - Basic operations and concepts
3. **[Key Concepts](../getting-started/key-concepts.md)** - Understanding TON fundamentals

## Common Tasks

### Wallet Management

**Create New Wallet**
```csharp
using Ton.Crypto.Mnemonic;
using Ton.Contracts.Wallets.V5;

// Generate mnemonic
string[] mnemonic = Mnemonic.New(24);

// Derive keys
KeyPair keys = Mnemonic.ToWalletKey(mnemonic);

// Create wallet
WalletV5R1 wallet = new(keys.PublicKey);

Console.WriteLine($"Address: {wallet.Address}");
Console.WriteLine($"Mnemonic: {string.Join(" ", mnemonic)}");
```

**Recover Wallet**
```csharp
// Get mnemonic from user
string[] mnemonic = GetMnemonicFromUser();

// Validate
if (!Mnemonic.Validate(mnemonic))
{
    throw new ArgumentException("Invalid mnemonic");
}

// Restore wallet
KeyPair keys = Mnemonic.ToWalletKey(mnemonic);
WalletV5R1 wallet = new(keys.PublicKey);
```

**Check Balance**
```csharp
using Ton.LiteClient;

LiteClient client = await LiteClientFactory.CreateFromUrlAsync(
    "https://ton.org/global-config.json"
);

MasterchainInfo info = await client.GetMasterchainInfoAsync();
AccountState state = await client.GetAccountStateAsync(wallet.Address, info.Last);

Console.WriteLine($"Balance: {state.BalanceInTon:F4} TON");
Console.WriteLine($"State: {state.State}");
```

### Sending Transactions

**Simple Transfer**
```csharp
Address destination = Address.Parse("...");
decimal amount = 1.5m;  // TON

// Get seqno
int seqno = await GetCurrentSeqno(client, wallet.Address);

// Create transfer
var actions = new[]
{
    WalletV5OutActions.SendMessage(
        mode: SendMode.PayGasSeparately | SendMode.IgnoreErrors,
        message: CreateInternalMessage(
            destination,
            Coins.FromNano((long)(amount * 1_000_000_000))
        )
    )
};

Cell body = wallet.CreateTransferBody(
    keys.SecretKey,
    wallet.WalletId,
    DateTimeOffset.UtcNow.AddMinutes(5).ToUnixTimeSeconds(),
    seqno,
    actions
);

// Send (see module documentation for sending)
```

**Transfer with Comment**
```csharp
string comment = "Payment";
Cell commentCell = Builder.BeginCell()
    .StoreUint(0, 32)
    .StoreString(comment)
    .EndCell();

// Use commentCell as message body
```

### Querying Blockchain

**Get Account Info**
```csharp
LiteClient client = await LiteClientFactory.CreateFromUrlAsync(...);
MasterchainInfo info = await client.GetMasterchainInfoAsync();
Address address = Address.Parse("...");

AccountState state = await client.GetAccountStateAsync(address, info.Last);

Console.WriteLine($"Balance: {state.BalanceInTon} TON");
Console.WriteLine($"Is active: {state.IsActive}");
Console.WriteLine($"Is contract: {state.IsContract}");

if (state.Code != null)
{
    Console.WriteLine($"Code hash: {Convert.ToHexString(state.Code.Hash(0))}");
}
```

**Get Recent Blocks**
```csharp
MasterchainInfo info = await client.GetMasterchainInfoAsync();
Console.WriteLine($"Latest block: {info.Last.Seqno}");

// Get block from 1 hour ago
int timestamp = (int)DateTimeOffset.UtcNow.AddHours(-1).ToUnixTimeSeconds();
BlockId oldBlock = await client.LookupBlockByUtimeAsync(-1, -9223372036854775808, timestamp);
Console.WriteLine($"Block 1h ago: {oldBlock.Seqno}");
```

**List Transactions in Block**
```csharp
BlockTransactions txs = await client.ListBlockTransactionsAsync(blockId, count: 100);

foreach (BlockTransaction tx in txs.Transactions)
{
    Console.WriteLine($"Account: {Convert.ToHexString(tx.AccountHash)}");
    Console.WriteLine($"LT: {tx.Lt}");
}
```

### Working with Cells

**Build Cell**
```csharp
using Ton.Core.Boc;

Builder builder = Builder.BeginCell();
builder.StoreUint(123, 32);
builder.StoreAddress(address);
builder.StoreCoins(1_000_000_000);
builder.StoreBit(true);

Cell ref = Builder.BeginCell().StoreUint(456, 32).EndCell();
builder.StoreRef(ref);

Cell cell = builder.EndCell();
```

**Read Cell**
```csharp
Slice slice = cell.BeginParse();

uint value = slice.LoadUint(32);
Address addr = slice.LoadAddress();
BigInteger coins = slice.LoadCoins();
bool flag = slice.LoadBit();

Cell ref = slice.LoadRef();
```

**Serialize/Deserialize**
```csharp
// To BOC
byte[] boc = cell.ToBoc();
File.WriteAllBytes("data.boc", boc);

// From BOC
byte[] data = File.ReadAllBytes("data.boc");
Cell[] cells = Cell.FromBoc(data);
Cell root = cells[0];
```

### Cryptography

**Generate Keys**
```csharp
using Ton.Crypto.Mnemonic;
using Ton.Crypto.Ed25519;

string[] mnemonic = Mnemonic.New(24);
KeyPair keys = Mnemonic.ToWalletKey(mnemonic);
```

**Sign and Verify**
```csharp
byte[] message = Encoding.UTF8.GetBytes("Hello");

// Sign
byte[] signature = Ed25519.Sign(message, keys.SecretKey);

// Verify
bool isValid = Ed25519.Verify(signature, message, keys.PublicKey);
```

**Hash Data**
```csharp
using Ton.Crypto.Primitives;

byte[] data = Encoding.UTF8.GetBytes("data");
byte[] hash = Sha256.Hash(data);
```

## Advanced Topics

### Multiple Transfers in One Transaction

```csharp
var actions = new[]
{
    WalletV5OutActions.SendMessage(...),  // Transfer 1
    WalletV5OutActions.SendMessage(...),  // Transfer 2
    WalletV5OutActions.SendMessage(...)   // Transfer 3
};

Cell body = wallet.CreateTransferBody(keys.SecretKey, ..., actions);
```

### Custom Wallet IDs

```csharp
int customWalletId = WalletV5R1WalletId.Create(
    networkGlobalId: -239,  // mainnet
    workchain: 0,
    subwalletNumber: 0,
    version: 0
);

WalletV5R1 wallet = new(keys.PublicKey, walletId: customWalletId);
```

### Parse MerkleProof

```csharp
// Get header with proof
BlockHeader header = await client.GetBlockHeaderAsync(blockId);

// Parse proof
Cell proofCell = Cell.FromBoc(header.HeaderProof)[0];
Cell actualData = proofCell.UnwrapProof();

// Now parse actual data
Slice slice = actualData.BeginParse();
// ... parse block header
```

### Round-Robin Connection

```csharp
// Automatic from global config
LiteClient client = await LiteClientFactory.CreateFromUrlAsync(
    "https://ton.org/global-config.json"
);

// Manual setup
var servers = new[]
{
    ("server1.com", 14432, "base64_key1"),
    ("server2.com", 14432, "base64_key2")
};

LiteClient roundRobin = LiteClientFactory.CreateRoundRobin(servers);
```

## Troubleshooting

### Common Issues

**"Invalid mnemonic"**
```csharp
// Always validate user input
if (!Mnemonic.Validate(mnemonic))
{
    Console.WriteLine("Mnemonic is invalid");
    return;
}
```

**"Insufficient balance"**
```csharp
// Check balance before sending
AccountState state = await client.GetAccountStateAsync(address, block);
if (state.Balance < amountToSend + estimatedFee)
{
    throw new InvalidOperationException("Insufficient balance");
}
```

**"Transaction not confirmed"**
```csharp
// Wait for seqno increase
int oldSeqno = currentSeqno;
for (int i = 0; i < 30; i++)
{
    await Task.Delay(2000);
    int newSeqno = await GetCurrentSeqno(client, address);
    if (newSeqno > oldSeqno)
    {
        Console.WriteLine("Confirmed!");
        break;
    }
}
```

**"Connection timeout"**
```csharp
// Increase timeout or retry
try
{
    result = await client.GetAccountStateAsync(address, block, timeout: 10000);
}
catch (TimeoutException)
{
    // Retry with different server or increase timeout
}
```

## Best Practices

1. **Always validate addresses** before sending TON
2. **Check balances** before creating transactions
3. **Set appropriate timeouts** (5 minutes recommended)
4. **Clear private keys** from memory after use
5. **Use round-robin** for production applications
6. **Handle errors gracefully** with retries
7. **Cache rarely-changing data** when possible

## Module-Specific Guides

For detailed module documentation:

- **[LiteClient](../modules/liteclient/overview.md)** - Blockchain queries
- **[Core](../modules/core/overview.md)** - Data structures
- **[Crypto](../modules/crypto/overview.md)** - Cryptography
- **[Contracts](../modules/contracts/overview.md)** - Wallets
- **[HttpClient](../modules/httpclient/overview.md)** - HTTP API

## Example Projects

Check out the `/samples` directory in the repository:

- **LiteClientPlayground** - LiteClient usage examples
- **AdnlSample** - Low-level ADNL protocol usage

## Getting Help

- **Documentation**: Read module-specific guides
- **GitHub Issues**: Report bugs or request features
- **Examples**: Check sample projects in repository
