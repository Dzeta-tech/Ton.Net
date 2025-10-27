# Ton.NET

A modern, comprehensive .NET SDK for the TON (The Open Network) blockchain. Built from scratch with clean architecture, targeting compatibility with the official [TON JavaScript SDK](https://github.com/ton-org/ton).

**Why Ton.NET?**
- ✨ Modern C# 12 with nullable reference types
- 🎯 Targeting API compatibility with TON JS SDK
- 🔒 Type-safe primitives and TL-B structures
- 🧪 360+ tests with full coverage
- 📦 Modular architecture
- 🚀 Production-ready

> **Note:** This is a complete rewrite replacing the legacy TonSdk.NET. It provides a cleaner, more maintainable codebase with improved compatibility.

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
using Ton.Core.Boc;
using Ton.Core.Addresses;
using Ton.Core.Types;

// Generate mnemonic
var mnemonic = Mnemonic.New(); // 24 words
var keyPair = Mnemonic.ToWalletKey(mnemonic);

// Create WalletV5R1
var wallet = WalletV5R1.Create(
    workchain: 0, 
    publicKey: keyPair.PublicKey
);
Console.WriteLine($"Address: {wallet.Address}");

// Connect to blockchain
var client = new TonClient(new TonClientParameters 
{ 
    Endpoint = "https://toncenter.com/api/v2/jsonRPC",
    ApiKey = "your-api-key" // optional
});

var opened = client.Open(wallet);

// Get balance
var balance = await opened.Contract.GetBalanceAsync(opened.Provider);
Console.WriteLine($"Balance: {balance / 1_000_000_000m} TON");

// Get seqno
var seqno = await opened.Contract.GetSeqnoAsync(opened.Provider);

// Create message with comment
var body = Builder.BeginCell()
    .StoreUint(0, 32) // text comment opcode
    .StoreStringTail("Hello TON!")
    .EndCell();

var message = new MessageRelaxed(
    new CommonMessageInfoRelaxed.Internal(
        IhrDisabled: true,
        Bounce: true,
        Bounced: false,
        Src: null,
        Dest: Address.Parse("EQCD39VS5jcptHL8vMjEXrzGaRcCVYto7HUn4bpAOg8xqB2N"),
        Value: new CurrencyCollection(1_000_000_000), // 1 TON
        IhrFee: 0,
        ForwardFee: 0,
        CreatedLt: 0,
        CreatedAt: 0
    ),
    body,
    StateInit: null
);

// Send transfer
var transfer = wallet.CreateTransfer(
    seqno: seqno,
    secretKey: keyPair.SecretKey,
    messages: new List<MessageRelaxed> { message },
    sendMode: SendMode.SendPayFwdFeesSeparately | SendMode.SendIgnoreErrors
);

await opened.Contract.SendAsync(opened.Provider, transfer);
Console.WriteLine("Transfer sent!");
```

### Working with Cells and BOC

```csharp
using Ton.Core.Boc;

// Create a cell
var cell = Builder.BeginCell()
    .StoreUint(123, 32)
    .StoreAddress(Address.Parse("EQ..."))
    .StoreStringTail("Hello")
    .EndCell();

// Serialize to BOC
var boc = cell.ToBoc();

// Deserialize from BOC
var cells = Cell.FromBoc(boc);
var loadedCell = cells[0];

// Read from cell
var slice = loadedCell.BeginParse();
var number = slice.LoadUint(32);
var address = slice.LoadAddress();
var text = slice.LoadStringTail();
```

### Using TonClient4 (v4 API)

```csharp
using Ton.HttpClient;

var client = new TonClient4(new TonClient4Parameters
{
    Endpoint = "https://mainnet-v4.tonhubapi.com"
});

// Get last block
var lastBlock = await client.GetLastBlockAsync();
Console.WriteLine($"Last block: {lastBlock.Last.Seqno}");

// Get account state
var account = await client.GetAccountAsync(
    lastBlock.Last.Seqno, 
    Address.Parse("EQ...")
);

Console.WriteLine($"Balance: {account.Account.Balance}");
```

### Send Modes

TON blockchain supports various send modes that control message behavior:

```csharp
// Basic send mode - pay fees from message value
SendMode.SendDefault

// Pay fees separately from message value
SendMode.SendPayFwdFeesSeparately

// Ignore errors during action phase
SendMode.SendIgnoreErrors

// Bounce transaction on action failure (no effect with SendIgnoreErrors)
SendMode.SendBounceIfActionFail

// Destroy contract if balance reaches zero
SendMode.SendDestroyIfZero

// Carry remaining inbound message value (+64)
SendMode.SendRemainingValue

// Carry all remaining contract balance (+128)
SendMode.SendRemainingBalance

// Common combinations:
// - Standard transfer: SendPayFwdFeesSeparately | SendIgnoreErrors
// - Send all balance: SendRemainingBalance | SendDestroyIfZero | SendIgnoreErrors
```

See [TON Documentation on Message Modes](https://docs.ton.org/develop/smart-contracts/messages#message-modes) for details.

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
- 327 tests in Ton.Core.Tests (BOC, TL-B, Dictionaries, Contracts)
- 18 tests in Ton.Crypto.Tests (Mnemonic, Ed25519, Hashing)
- 15 tests in Ton.HttpClient.Tests (TonClient v2/v4)

Total: **360 passing unit + integration tests**

All tests validate compatibility with the TON JavaScript SDK's behavior.

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

### Key Design Principles

- **Type Safety**: Nullable reference types, records, and pattern matching
- **Zero Dependencies**: Only System.Text.Json for HTTP clients
- **Performance**: Efficient cell serialization with proper hashing
- **Compatibility**: API designed to match TON JS SDK patterns
- **Testability**: Every feature has corresponding tests from JS SDK

## 🛠️ Tools

### Wallet Playground

An interactive console app for testing wallet operations:

```bash
cd tools/WalletPlayground
dotnet run
```

Features:
- Generate or load mnemonic phrases
- Create WalletV5R1 instances
- Check balance and seqno
- Send transfers with comments
- Deploy wallets
- Send all remaining balance (destroy wallet)

## 🤝 Contributing

Contributions are welcome! This project aims for API compatibility with the TON JavaScript SDK.

## 📄 License

MIT License - see [LICENSE](LICENSE) file for details.

## 🔗 Links

- [TON Official Website](https://ton.org/)
- [TON GitHub](https://github.com/ton-blockchain/ton)
- [TON Community](https://t.me/tondev_eng)
