# Installation

Install Ton.NET packages via NuGet.

## Requirements

- .NET 9.0 or higher

## Packages

```bash
# Core (required)
dotnet add package Ton.Core
dotnet add package Ton.Crypto

# Wallet operations
dotnet add package Ton.Contracts

# Blockchain access (choose one)
dotnet add package Ton.LiteClient   # ADNL protocol
dotnet add package Ton.HttpClient   # HTTP API
```

## Quick Test

```csharp
using Ton.Crypto;
using Ton.Contracts.Wallets;

var mnemonic = Mnemonic.Generate();
var keyPair = Mnemonic.ToKeyPair(mnemonic);
var wallet = new WalletV4R2(keyPair.PublicKey);

Console.WriteLine($"Address: {wallet.Address}");
Console.WriteLine($"Mnemonic: {string.Join(" ", mnemonic)}");
```

