# Ton.NET

Modern .NET SDK for TON blockchain with 1:1 API compatibility with official JavaScript SDKs.

## Packages

| Package | Description | Version |
|---------|-------------|---------|
| **Ton.Core** | Core types, addresses, cells, BOC | [![NuGet](https://img.shields.io/nuget/v/Ton.Core.svg)](https://www.nuget.org/packages/Ton.Core/) |
| **Ton.Crypto** | Cryptography, mnemonics, Ed25519 | [![NuGet](https://img.shields.io/nuget/v/Ton.Crypto.svg)](https://www.nuget.org/packages/Ton.Crypto/) |
| **Ton.Client** | HTTP clients, wallets, jettons | [![NuGet](https://img.shields.io/nuget/v/Ton.Client.svg)](https://www.nuget.org/packages/Ton.Client/) |

## Installation

```bash
dotnet add package Ton.Core
dotnet add package Ton.Crypto
dotnet add package Ton.Client
```

## Quick Start

```csharp
using Ton.Core.Addresses;

// Parse and work with TON addresses
var address = Address.Parse("EQAs9VlT6S776tq3unJcP5Ogsj-ELLunLXuOb1EKcOQi4wJB");
Console.WriteLine($"Workchain: {address.WorkChain}");
Console.WriteLine($"Raw: {address.ToRawString()}");

// Convert between formats
var raw = "0:2cf55953e92efbeadab7ba725c3f93a0b23f842cbba72d7b8e6f510a70e422e3";
var friendly = Address.Parse(raw).ToString();
Console.WriteLine($"Friendly: {friendly}");

// Validate addresses
bool isFriendly = Address.IsFriendly("EQAs9VlT...");
bool isRaw = Address.IsRaw("0:2cf55953...");
```

## Features

### ✅ Ton.Core (v0.0.1)
- ✅ Address type (immutable, full compatibility with JS)
- ✅ CRC-16 checksum
- ⏳ BitString, BitReader, BitBuilder
- ⏳ Cell, Builder, Slice
- ⏳ Dictionary
- ⏳ Tuple types
- ⏳ TL-B schemas (Message, Transaction, Account, etc.)

### ⏳ Ton.Crypto (v0.0.1 - Coming Soon)
- ⏳ Mnemonic (BIP39)
- ⏳ Ed25519 signing
- ⏳ SHA-256/512, HMAC, PBKDF2
- ⏳ HD wallets

### ⏳ Ton.Client (v0.0.1 - Coming Soon)
- ⏳ HTTP API clients
- ⏳ Wallet contracts (V1-V5)
- ⏳ Jetton support
- ⏳ Multisig
- ⏳ Fee computation

## API Compatibility

This SDK maintains 1:1 API compatibility with official TON JavaScript SDKs:
- [@ton/core](https://github.com/ton-org/ton-core) → **Ton.Core**
- [@ton/crypto](https://github.com/ton-org/ton-crypto) → **Ton.Crypto**
- [@ton/ton](https://github.com/ton-org/ton) → **Ton.Client**

API naming follows C# conventions (PascalCase) while preserving the same method signatures and behavior.

## Development Status

🚧 **Early Development** - Currently implementing Phase 1 (Core types). API may change before 1.0.0 release.

See [API_INVENTORY.md](API_INVENTORY.md) for detailed implementation roadmap.

## Requirements

- .NET 9.0 or later

## License

MIT License - see LICENSE file for details.

## Contributing

Contributions welcome! This is a ground-up rewrite focused on:
- Clean, modern C# code
- Full test coverage
- Complete API compatibility with JS SDKs
- Comprehensive documentation

## Links

- [TON Documentation](https://docs.ton.org/)
- [JavaScript SDKs](https://github.com/ton-org)
- [Issue Tracker](https://github.com/Dzeta-tech/Ton.Net/issues)

