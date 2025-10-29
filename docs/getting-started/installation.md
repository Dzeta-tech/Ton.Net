# Installation

## Requirements

- **.NET 8.0 or .NET 9.0**
- Basic understanding of C# and async/await patterns
- (Optional) TON blockchain basics knowledge

## Installing Packages

Install packages via NuGet based on what you need:

### For Lite Client (Direct Node Access)

```bash
dotnet add package Ton.LiteClient
```

This automatically includes `Ton.Core`, `Ton.Adnl`, and `Ton.Crypto`.

### For HTTP Client (API Access)

```bash
dotnet add package Ton.HttpClient
```

This automatically includes `Ton.Core` and `Ton.Crypto`.

### For Smart Contract Development

```bash
dotnet add package Ton.Contracts
```

This includes wallet contracts and base contract functionality.

### Core Packages (Usually Included Automatically)

```bash
# Cell/BOC manipulation, addresses, types
dotnet add package Ton.Core

# Cryptography: Ed25519, mnemonics, hashing
dotnet add package Ton.Crypto

# Low-level ADNL protocol (for lite client)
dotnet add package Ton.Adnl
```

## Quick Start

```csharp
using Ton.LiteClient;
using Ton.Core.Addresses;

// Create client from TON global config
LiteClient client = await LiteClientFactory.CreateFromUrlAsync(
    "https://ton.org/global-config.json"
);

// Get masterchain info
MasterchainInfo info = await client.GetMasterchainInfoAsync();
Console.WriteLine($"Latest block: {info.Last.Seqno}");

// Get account balance
Address address = Address.Parse("EQCD39VS5jcptHL8vMjEXrzGaRcCVYto7HUn4bpAOg8xqB2N");
BlockId block = info.Last;
AccountState state = await client.GetAccountStateAsync(address, block);
Console.WriteLine($"Balance: {state.BalanceInTon} TON");
```
