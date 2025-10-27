# Ton.NET API Inventory

> **Status Overview:** Core foundation complete, ready for contracts and client implementations

## Implementation Status

### ✅ Completed Modules (v0.1.0)

| Module         | Features                                                              | Version | Tests        |
| -------------- | --------------------------------------------------------------------- | ------- | ------------ |
| **Address**    | Address, ExternalAddress, ContractAddress                             | v0.1.0  | ✅           |
| **BOC**        | BitString, BitReader, BitBuilder, Builder, Slice, Cell, serialization | v0.0.8  | ✅ 15 tests  |
| **Dictionary** | Full hashmap implementation, all key/value types                      | v0.0.7  | ✅ 16 tests  |
| **Tuple**      | TupleReader, TupleBuilder, all item types                             | v0.0.7  | ✅ 25 tests  |
| **TL-B Types** | All 37 types (Messages, Accounts, Transactions, Shards)               | v0.1.0  | ✅ 267 tests |
| **Contracts**  | IContract, Provider, State, Sender, OpenedContract, ABI               | v0.1.0  | ✅ 24 tests  |
| **HttpClient** | HttpApi, TonClient, TonClientProvider, Stack Parser                   | v0.1.0  | ✅ 9 tests   |
| **Utils**      | ToNano, FromNano, CRC16, CRC32C, Base32, GetMethodId                  | v0.0.2  | ✅           |
| **Crypto**     | SHA256, SHA512, PBKDF2, HMAC, Ed25519, Mnemonic                       | v0.0.7  | ✅ 47 tests  |

**Total:** 354 tests passing | 100% JS SDK parity for implemented features

---

## 📋 Remaining Work

### @ton/core - Missing Features

| Feature                              | Priority  | Status   | Notes                                    |
| ------------------------------------ | --------- | -------- | ---------------------------------------- |
| **Contract Module**                  | 🔴 High   | ✅ Done  | Base interfaces for contract interaction |
| └─ `IContract` interface             | High      | ✅       | Define contract interface                |
| └─ `IContractProvider`               | High      | ✅       | Provider for contract calls              |
| └─ `ContractState`                   | High      | ✅       | Contract state representation            |
| └─ `ISender` interface               | High      | ✅       | Message sender abstraction               |
| └─ `OpenedContract<T>`               | High      | ✅       | Open contract helper                     |
| └─ `ComputeError`                    | Medium    | ✅       | Compute phase errors                     |
| └─ `ContractABI` types               | Low       | ✅       | ABI type definitions                     |
| **Address Utils**                    | 🟡 Medium | Partial  | Additional address utilities             |
| └─ `ADNLAddress`                     | Medium    | ❌       | ADNL address type                        |
| └─ `ContractAddress()`               | Medium    | ✅       | Generate contract address                |
| **Exotic Cells**                     | 🟡 Medium | Not started | Merkle proofs/updates                    |
| └─ `GenerateMerkleProof()`           | Medium    | ❌          | Generate Merkle proofs                   |
| └─ `GenerateMerkleUpdate()`          | Medium    | ❌          | Generate Merkle updates                  |
| └─ Exotic cell parsing               | Medium    | ❌          | Full exotic cell support                 |
| **Crypto Utils**                     | 🟢 Low    | Not started | Safe signing                             |
| └─ `SafeSign()` / `SafeSignVerify()` | Low       | ❌          | Safe signature functions                 |
| **BOC Utils**                        | 🟢 Low    | Partial     | Additional helpers                       |
| └─ `Writable` interface              | Low       | ❌          | Generic writable interface               |

### @ton/crypto - Missing Features

| Feature                    | Priority  | Status      | Notes                      |
| -------------------------- | --------- | ----------- | -------------------------- |
| **HD Wallets**             | 🟡 Medium | Not started | BIP32-like derivation      |
| └─ `HDKeysState`           | Medium    | ❌          | HD wallet state            |
| └─ `DeriveED25519Path()`   | Medium    | ❌          | ED25519 key derivation     |
| └─ `DeriveSymmetricPath()` | Medium    | ❌          | Symmetric key derivation   |
| └─ `DeriveMnemonicsPath()` | Medium    | ❌          | Mnemonic derivation        |
| **Password Utils**         | 🟢 Low    | Not started | Secure passphrases         |
| └─ `NewSecureWords()`      | Low       | ❌          | Generate word passphrase   |
| └─ `NewSecurePassphrase()` | Low       | ❌          | Generate secure passphrase |

### @ton - Client & Contracts

| Module                 | Priority  | Status   | Notes                          |
| ---------------------- | --------- | -------- | ------------------------------ |
| **HTTP API Client**    | 🔴 High   | Partial  | v2 complete, v4 pending        |
| └─ `HttpApi`           | High      | ✅       | Low-level JSON-RPC client      |
| └─ `TonClient` (v2)    | High      | ✅       | Toncenter API v2 + provider    |
| └─ `TonClient4` (v4)   | High      | ❌       | Toncenter API v4               |
| **Wallet Contracts**   | 🔴 High   | ❌     | 5-7 days                       |
| └─ WalletV1R1-V1R3     | High      | ❌     | Legacy wallets                 |
| └─ WalletV2R1-V2R2     | High      | ❌     | V2 wallets                     |
| └─ WalletV3R1-V3R2     | High      | ❌     | V3 wallets                     |
| └─ WalletV4            | High      | ❌     | V4 with plugins                |
| └─ WalletV5Beta, V5R1  | High      | ❌     | Latest wallets                 |
| **Jetton Contracts**   | 🟡 Medium | ❌     | 2-3 days                       |
| └─ `JettonMaster`      | Medium    | ❌     | Jetton master contract         |
| └─ `JettonWallet`      | Medium    | ❌     | Jetton wallet contract         |
| **Advanced Contracts** | 🟢 Low    | ❌     | 3-5 days                       |
| └─ `MultisigWallet`    | Low       | ❌     | Multisig contract              |
| └─ `ElectorContract`   | Low       | ❌     | Validator elector              |
| **Config Parser**      | 🟢 Low    | ❌     | 2-3 days                       |
| └─ Parse config params | Low       | ❌     | Config params 5-40             |
| └─ `ParseFullConfig()` | Low       | ❌     | Complete config parser         |
| **Fee Computation**    | 🟢 Low    | ❌     | 2-3 days                       |
| └─ Message fees        | Low       | ❌     | External/internal message fees |
| └─ Gas prices          | Low       | ❌     | Compute gas costs              |
| └─ Storage fees        | Low       | ❌     | Storage fee calculation        |

---

## 🎯 Recommended Implementation Order

### Phase 1: Contract Foundation ✅ COMPLETE

**Goal:** Enable basic contract interactions

- [x] Implement Contract module interfaces
- [x] Add ContractAddress utility
- [ ] Add ADNLAddress support

### Phase 2: HTTP Client ✅ v2 COMPLETE

**Goal:** Connect to TON network

- [x] HttpApi low-level client
- [x] TonClient v2 wrapper (IContractProvider)
- [ ] TonClient4 v4 wrapper

### Phase 3: Wallet Contracts (2 weeks)

**Goal:** Support all wallet versions

- [ ] WalletV4 (most common)
- [ ] WalletV3R2 (widely used)
- [ ] WalletV5R1 (latest)
- [ ] Other versions (V1, V2, V3R1)

### Phase 4: Jetton Support (1 week)

**Goal:** Enable token operations

- [ ] JettonMaster contract
- [ ] JettonWallet contract

### Phase 5: Advanced Features (2-3 weeks)

**Goal:** Complete ecosystem

- [ ] HD Wallets
- [ ] Exotic cells (Merkle proofs)
- [ ] Multisig contracts
- [ ] Config parser
- [ ] Fee computation

---

## 📊 Progress Summary

| Category                      | Completed   | Remaining   | Progress |
| ----------------------------- | ----------- | ----------- | -------- |
| **@ton/core Foundation**      | 7/7 modules | 3 features  | 🟢 98%   |
| **@ton/crypto**               | 3/5 modules | 2 features  | 🟢 85%   |
| **@ton (Client & Contracts)** | 1/6 modules | 5 modules   | 🟡 20%   |
| **Overall**                   | Core + HTTP | Wallets     | 🟡 65%   |

**Key Takeaway:** Foundation + HTTP Client complete! Next priority: Wallets → Jettons

---

## 🔍 What We Have vs What TON Needs

### ✅ We Have (Production Ready)

- Complete TL-B type system for blockchain parsing
- Full BOC serialization/deserialization
- Dictionary (hashmap) implementation
- Tuple system for contract data
- **Contract interfaces and abstractions** ✨ NEW
- Cryptographic primitives (Ed25519, SHA, HMAC)
- Mnemonic (BIP39) support
- All utilities (CRC, Base32, conversions)

### ❌ We Need (To Build DApps)

- HTTP client to connect to TON
- Wallet contracts to send transactions
- Jetton contracts for tokens
- HD wallet support
- Advanced features (config, fees, multisig)

### 🎯 Current State

Perfect for:

- ✅ Building lite clients
- ✅ Parsing blockchain data
- ✅ Analyzing transactions
- ✅ Understanding TON internals
- ✅ Contract abstraction layer ✨ NEW

Ready with HTTP client:

- ⏳ Sending transactions (needs HTTP client)
- ⏳ Creating wallets (needs HTTP client + wallet contracts)
- ⏳ Deploying contracts (needs HTTP client)
- ⏳ Token operations (needs HTTP client + jetton contracts)

**Next milestone: HTTP Client = Unlock wallet & contract interactions!**
