# Ton.NET

A comprehensive .NET SDK for the TON (The Open Network) blockchain, providing 1:1 API compatibility with the official [TON JavaScript SDK](https://github.com/ton-org/ton).

## 📦 Packages

| Package | Version | Description |
|---------|---------|-------------|
| **Ton.Core** | [![NuGet](https://img.shields.io/nuget/v/Ton.Core.svg)](https://www.nuget.org/packages/Ton.Core/) | Core primitives: Cells, BOC, Addresses, Types |
| **Ton.Crypto** | [![NuGet](https://img.shields.io/nuget/v/Ton.Crypto.svg)](https://www.nuget.org/packages/Ton.Crypto/) | Ed25519, Mnemonics (BIP39), SHA, HMAC |
| **Ton.Contracts** | [![NuGet](https://img.shields.io/nuget/v/Ton.Contracts.svg)](https://www.nuget.org/packages/Ton.Contracts/) | Smart contracts: Wallets, Jettons, NFTs |
| **Ton.HttpClient** | [![NuGet](https://img.shields.io/nuget/v/Ton.HttpClient.svg)](https://www.nuget.org/packages/Ton.HttpClient/) | HTTP API clients (Toncenter v2/v4) |

## 🚀 Quick Start

```bash
dotnet add package Ton.Core
dotnet add package Ton.Crypto
dotnet add package Ton.Contracts
dotnet add package Ton.HttpClient
```

### Create and Use a Wallet

```csharp
using Ton.Contracts.Wallets.V5;
using Ton.Crypto.Mnemonic;
using Ton.HttpClient;

// Generate mnemonic
var mnemonic = Mnemonic.Generate();
var keyPair = Mnemonic.ToKeyPair(mnemonic);

// Create wallet
var wallet = WalletV5R1.Create(0, keyPair.PublicKey);
Console.WriteLine($"Address: {wallet.Address}");

// Connect to blockchain
var client = new TonClient(new TonClientParameters 
{ 
    Endpoint = "https://toncenter.com/api/v2/jsonRPC" 
});

var opened = client.Open(wallet);

// Get balance
var balance = await opened.Contract.GetBalanceAsync(opened.Provider);
Console.WriteLine($"Balance: {balance} nanotons");

// Send transfer
var transfer = wallet.CreateTransfer(
    seqno: await opened.Contract.GetSeqnoAsync(opened.Provider),
    secretKey: keyPair.SecretKey,
    messages: new[] { 
        new MessageRelaxed(/* ... */) 
    },
    sendMode: SendMode.PayFeesSeparately
);

await opened.Contract.SendAsync(opened.Provider, transfer);
```

## 📋 Implementation Status

### ✅ Completed

- **Core Primitives**: Cell, BOC serialization, Address, BitString, Dictionary
- **TL-B Types**: All 37 types (Message, Transaction, Account, StateInit, etc.)
- **Cryptography**: Ed25519, BIP39 mnemonics, SHA-256/512, HMAC, PBKDF2
- **HTTP Clients**: Toncenter API v2 and v4
- **Wallets**: V5R1 (transfers, extensions, auth modes)
- **Contract System**: IContract, IContractProvider, OpenedContract

### 🚧 In Progress

- **Wallets**: V1R1, V1R2, V1R3, V2R1, V2R2, V3R1, V3R2, V4R1, V4R2
- **Jettons**: JettonMaster, JettonWallet
- **NFTs**: NFTCollection, NFTItem
- **ADNL**: Lite client for direct node communication

## 🧪 Testing

```bash
dotnet test
```

**Test Coverage:**
- 327 tests in Ton.Core.Tests
- 18 tests in Ton.Crypto.Tests  
- 41 tests in Ton.Contracts.Tests (including integration tests)
- 15 tests in Ton.HttpClient.Tests

Total: **401 passing tests**

## 📚 Documentation

- **TON Documentation**: https://docs.ton.org/
- **TON JavaScript SDK**: https://github.com/ton-org/ton
- **TL-B Schemas**: https://github.com/ton-blockchain/ton/tree/master/crypto/block

## 🏗️ Architecture

```
Ton.Core          → Core blockchain primitives (Cell, Address, BOC, etc.)
Ton.Crypto        → Cryptographic operations (Ed25519, Mnemonics)
Ton.Contracts     → Smart contract implementations (Wallets, Jettons, NFTs)
Ton.HttpClient    → HTTP API clients (Toncenter v2/v4)
```

## 🤝 Contributing

Contributions are welcome! This project aims for API compatibility with the TON JavaScript SDK.

## 📄 License

MIT License - see [LICENSE](LICENSE) file for details.

## 🔗 Links

- [TON Official Website](https://ton.org/)
- [TON GitHub](https://github.com/ton-blockchain/ton)
- [TON Community](https://t.me/tondev_eng)
