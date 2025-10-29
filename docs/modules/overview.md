# Modules Overview

Ton.NET is organized into modular packages, each handling specific aspects of TON blockchain interaction.

## Core Modules

### [LiteClient](liteclient/overview.md)
Direct blockchain access via ADNL protocol.

**Use when you need:**
- Production-ready blockchain queries
- Fast, efficient communication
- Automatic load balancing
- No rate limits

```csharp
LiteClient client = await LiteClientFactory.CreateFromUrlAsync(
    "https://ton.org/global-config.json"
);
AccountState state = await client.GetAccountStateAsync(address, block);
```

### [HttpClient](httpclient/overview.md)
HTTP API access via Toncenter and TON API v4.

**Use when you need:**
- Easy debugging with HTTP tools
- Rapid prototyping
- Web-friendly communication
- Simple integration

```csharp
TonClient client = new(new TonClientParameters 
{ 
    Endpoint = "https://toncenter.com/api/v2" 
});
BigInteger balance = await client.GetBalanceAsync(address);
```

### [Core](core/overview.md)
Fundamental data structures: Cells, Addresses, Types.

**Use for:**
- Cell/BOC manipulation
- Address parsing and formatting
- Working with blockchain types
- Dictionary operations

```csharp
Builder builder = Builder.BeginCell();
builder.StoreUint(123, 32);
builder.StoreAddress(address);
Cell cell = builder.EndCell();
```

### [Crypto](crypto/overview.md)
Cryptographic primitives: Ed25519, Mnemonics, Hashing.

**Use for:**
- Generating and validating mnemonics
- Key derivation
- Signing and verification
- Hashing operations

```csharp
string[] mnemonic = Mnemonic.New(24);
KeyPair keys = Mnemonic.ToWalletKey(mnemonic);
byte[] signature = Ed25519.Sign(message, keys.SecretKey);
```

### [Contracts](contracts/overview.md)
Smart contract implementations: Wallets, utilities.

**Use for:**
- Creating wallets
- Sending transactions
- Contract interaction
- Wallet operations

```csharp
WalletV5R1 wallet = new(publicKey);
Cell transfer = wallet.CreateTransferBody(secretKey, walletId, ...);
```

## Module Dependencies

```
Ton.LiteClient
├── Ton.Adnl
├── Ton.Core
└── Ton.Crypto

Ton.HttpClient
├── Ton.Core
└── Ton.Crypto

Ton.Contracts
├── Ton.Core
└── Ton.Crypto

Ton.Core
└── (no dependencies)

Ton.Crypto
└── (no dependencies)

Ton.Adnl
└── Ton.Crypto
```

## Choosing the Right Module

### For Blockchain Queries

**Production:**
```csharp
// ✅ Use LiteClient
LiteClient client = await LiteClientFactory.CreateFromUrlAsync(...);
```

**Development/Debugging:**
```csharp
// ✅ Use HttpClient
TonClient client = new(parameters);
```

### For Cell Operations

```csharp
// ✅ Always use Core
using Ton.Core.Boc;
Builder builder = Builder.BeginCell();
```

### For Cryptography

```csharp
// ✅ Always use Crypto
using Ton.Crypto.Mnemonic;
string[] mnemonic = Mnemonic.New(24);
```

### For Wallet Operations

```csharp
// ✅ Use Contracts + (LiteClient or HttpClient)
using Ton.Contracts.Wallets.V5;
WalletV5R1 wallet = new(publicKey);
```

## Installation Guide

### Minimal Setup (Read-only)

```bash
dotnet add package Ton.LiteClient
# Includes: Core, Adnl, Crypto
```

### Full Setup (With Wallets)

```bash
dotnet add package Ton.LiteClient
dotnet add package Ton.Contracts
```

### Alternative (HTTP API)

```bash
dotnet add package Ton.HttpClient
dotnet add package Ton.Contracts  # if sending transactions
```

## Common Patterns

### Pattern 1: Read Account State

```csharp
using Ton.LiteClient;
using Ton.Core.Addresses;

LiteClient client = await LiteClientFactory.CreateFromUrlAsync(...);
Address address = Address.Parse("...");
MasterchainInfo info = await client.GetMasterchainInfoAsync();
AccountState state = await client.GetAccountStateAsync(address, info.Last);

Console.WriteLine($"Balance: {state.BalanceInTon} TON");
```

### Pattern 2: Create and Send Transaction

```csharp
using Ton.Crypto.Mnemonic;
using Ton.Contracts.Wallets.V5;
using Ton.LiteClient;

// Get keys
string[] mnemonic = GetMnemonic();
KeyPair keys = Mnemonic.ToWalletKey(mnemonic);

// Create wallet
WalletV5R1 wallet = new(keys.PublicKey);

// Get seqno
LiteClient client = await LiteClientFactory.CreateFromUrlAsync(...);
int seqno = await GetSeqno(client, wallet.Address);

// Create transfer
Cell transfer = wallet.CreateTransferBody(
    keys.SecretKey,
    wallet.WalletId,
    validUntil: DateTimeOffset.UtcNow.AddMinutes(5).ToUnixTimeSeconds(),
    seqno: seqno,
    actions: [...]
);

// Send (implementation depends on client type)
```

### Pattern 3: Parse Blockchain Data

```csharp
using Ton.Core.Boc;
using Ton.Core.Types;

// Get transaction BOC
byte[] boc = ...;

// Parse
Cell[] cells = Cell.FromBoc(boc);
Cell root = cells[0];
Transaction tx = Transaction.Load(root.BeginParse(), root);

Console.WriteLine($"LT: {tx.Lt}");
Console.WriteLine($"Hash: {Convert.ToHexString(tx.Hash)}");
```

## Next Steps

- [Getting Started Guide](../getting-started/installation.md)
- [Key Concepts](../getting-started/key-concepts.md)
- [Module-Specific Documentation](liteclient/overview.md)
