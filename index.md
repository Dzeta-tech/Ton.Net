# Ton.NET Documentation

**Ton.NET** is a comprehensive .NET SDK for building applications on the TON blockchain with 1:1 API compatibility with official TON JavaScript libraries.

## Quick Start

```bash
dotnet add package Ton.Core
dotnet add package Ton.Crypto
dotnet add package Ton.LiteClient
```

```csharp
using Ton.Crypto;
using Ton.Contracts.Wallets;
using Ton.LiteClient;

// Generate a new wallet
var mnemonic = Mnemonic.Generate();
var keyPair = Mnemonic.ToKeyPair(mnemonic);
var wallet = new WalletV4R2(keyPair.PublicKey);

// Connect to TON blockchain
var client = await LiteClientFactory.CreateFromUrlAsync(
    "https://ton.org/global-config.json");

// Check wallet balance
var info = await client.GetMasterchainInfoAsync();
var state = await client.GetAccountStateAsync(wallet.Address, info.Last);
Console.WriteLine($"Balance: {state.BalanceInTon} TON");
```

## Modules

- **[Ton.Core](docs/modules/core/overview.md)** - Cells, BOC, Addresses
- **[Ton.Crypto](docs/modules/crypto/overview.md)** - Mnemonics, Ed25519
- **[Ton.Contracts](docs/modules/contracts/overview.md)** - Wallet contracts
- **[Ton.LiteClient](docs/modules/liteclient/overview.md)** - Blockchain queries via ADNL
- **[Ton.HttpClient](docs/modules/httpclient/overview.md)** - REST API client

## API Reference

Browse the complete [API Reference](api/) for detailed documentation of all types and methods.

## Links

- [GitHub Repository](https://github.com/continuation-team/Ton.NET)
- [NuGet Packages](https://www.nuget.org/packages?q=Ton.)
- [License: MIT](https://github.com/continuation-team/Ton.NET/blob/main/LICENSE)

