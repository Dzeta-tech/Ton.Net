# Ton.NET

A modern, comprehensive .NET SDK for the TON (The Open Network) blockchain. Built from scratch with clean architecture,
targeting compatibility with the official [TON JavaScript SDK](https://github.com/ton-org/ton).

[![Documentation](https://img.shields.io/badge/docs-online-blue)](https://dzeta-tech.github.io/Ton.Net/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

## ✨ Features

- 🎯 **API compatibility** with TON JavaScript SDK
- 🔒 **Type-safe** primitives and TL-B structures
- 🧪 **553+ tests** with comprehensive coverage
- 📦 **Modular architecture** - use only what you need
- 🚀 **Production-ready** with .NET 8 & 9 support
- 📚 **Complete documentation** with examples

## 📦 Packages

| Package            | Version                                                                                                       | Description                                   |
|--------------------|---------------------------------------------------------------------------------------------------------------|-----------------------------------------------|
| **Ton.Core**       | [![NuGet](https://img.shields.io/nuget/v/Ton.Core.svg)](https://www.nuget.org/packages/Ton.Core/)             | Core primitives: Cells, BOC, Addresses, Types |
| **Ton.Crypto**     | [![NuGet](https://img.shields.io/nuget/v/Ton.Crypto.svg)](https://www.nuget.org/packages/Ton.Crypto/)         | Ed25519, Mnemonics (BIP39), SHA, HMAC         |
| **Ton.Contracts**  | [![NuGet](https://img.shields.io/nuget/v/Ton.Contracts.svg)](https://www.nuget.org/packages/Ton.Contracts/)   | Smart contracts: Wallets V5R1                 |
| **Ton.LiteClient** | [![NuGet](https://img.shields.io/nuget/v/Ton.LiteClient.svg)](https://www.nuget.org/packages/Ton.LiteClient/) | ADNL protocol for direct node communication   |
| **Ton.HttpClient** | [![NuGet](https://img.shields.io/nuget/v/Ton.HttpClient.svg)](https://www.nuget.org/packages/Ton.HttpClient/) | HTTP API clients (Toncenter v2/v4)            |

## 🚀 Quick Start

Install packages:

```bash
dotnet add package Ton.LiteClient
```

Create a wallet and check balance:

```csharp
using Ton.Crypto.Mnemonic;
using Ton.Contracts.Wallets.V5;
using Ton.LiteClient;

// Generate mnemonic
var mnemonic = Mnemonic.New(24);
var keys = Mnemonic.ToWalletKey(mnemonic);

// Create wallet
var wallet = new WalletV5R1(keys.PublicKey);
Console.WriteLine($"Address: {wallet.Address}");

// Connect to blockchain
var client = await LiteClientFactory.CreateFromUrlAsync(
    "https://ton.org/global-config.json"
);

// Get balance
var info = await client.GetMasterchainInfoAsync();
var state = await client.GetAccountStateAsync(wallet.Address, info.Last);
Console.WriteLine($"Balance: {state.BalanceInTon} TON");
```

## 📚 Documentation

**[View Full Documentation →](https://dzeta-tech.github.io/Ton.Net/)**

### Getting Started

- **[Installation](https://dzeta-tech.github.io/Ton.Net/docs/getting-started/installation.html)** - Setup and
  requirements
- **[First Steps](https://dzeta-tech.github.io/Ton.Net/docs/getting-started/first-steps.html)** - Basic operations
- **[Key Concepts](https://dzeta-tech.github.io/Ton.Net/docs/getting-started/key-concepts.html)** - TON fundamentals

### Modules

- **[LiteClient](https://dzeta-tech.github.io/Ton.Net/docs/modules/liteclient/overview.html)** - Direct blockchain
  queries via ADNL
- **[HttpClient](https://dzeta-tech.github.io/Ton.Net/docs/modules/httpclient/overview.html)** - HTTP API access
- **[Core](https://dzeta-tech.github.io/Ton.Net/docs/modules/core/overview.html)** - Cells, Addresses, Types
- **[Crypto](https://dzeta-tech.github.io/Ton.Net/docs/modules/crypto/overview.html)** - Cryptography primitives
- **[Contracts](https://dzeta-tech.github.io/Ton.Net/docs/modules/contracts/overview.html)** - Wallet operations

### Guides

- **[Common Tasks](https://dzeta-tech.github.io/Ton.Net/docs/guides/overview.html)** - Practical examples and patterns

### API Reference

- **[API Documentation](https://dzeta-tech.github.io/Ton.Net/api/index.html)** - Complete API reference

## 🧪 Testing

```bash
dotnet test
```

**Test Coverage:**

- **298** Core tests (BOC, TL-B, Dictionaries, Addresses)
- **47** Crypto tests (Mnemonics, Ed25519, Hashing)
- **41** Contracts tests (Wallets)
- **156** ADNL tests (Protocol, Serialization, Crypto)
- **11** LiteClient tests (Integration)

**Total: 553 passing tests**

All tests validate compatibility with TON JavaScript SDK behavior.

## 🏗️ Architecture

```
Ton.LiteClient  → High-level blockchain queries (ADNL protocol)
    ├── Ton.Adnl      → Low-level ADNL protocol
    ├── Ton.Core      → Core primitives (Cell, Address, BOC)
    └── Ton.Crypto    → Cryptographic operations

Ton.HttpClient  → HTTP API client (Toncenter v2/v4)
    ├── Ton.Core
    └── Ton.Crypto

Ton.Contracts   → Smart contract implementations
    ├── Ton.Core
    └── Ton.Crypto
```

### Key Design Principles

- **Type Safety** - Nullable reference types, records, pattern matching
- **Zero External Dependencies** - Only System.Text.Json for HTTP clients
- **Performance** - Efficient cell serialization and hashing
- **Compatibility** - 1:1 API compatibility with TON JS SDK
- **Testability** - Comprehensive test coverage

## 🛠️ Development

### Requirements

- .NET 8.0 or .NET 9.0
- C# 12 language features

### Samples

Check out the `/samples` directory:

- **LiteClientPlayground** - LiteClient usage examples
- **AdnlSample** - Low-level ADNL protocol examples

## 🤝 Contributing

Contributions are welcome! This project aims for API compatibility with
the [TON JavaScript SDK](https://github.com/ton-org/ton).

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests for new functionality
5. Submit a pull request

## 📄 License

MIT License - see [LICENSE](LICENSE) file for details.

## 🔗 Resources

### Official TON

- [TON Website](https://ton.org/)
- [TON Documentation](https://docs.ton.org/)
- [TON GitHub](https://github.com/ton-blockchain/ton)
- [TON JavaScript SDK](https://github.com/ton-org/ton)

### Community

- [TON Dev Chat](https://t.me/tondev_eng)
- [TON Community](https://t.me/toncoin)

### Ton.NET

- [Documentation](https://dzeta-tech.github.io/Ton.Net/)
- [NuGet Packages](https://www.nuget.org/profiles/Ton.NET)
- [GitHub Repository](https://github.com/your-org/Ton.NET)
- [Report Issues](https://github.com/your-org/Ton.NET/issues)

---

Made with ❤️ for the TON community
