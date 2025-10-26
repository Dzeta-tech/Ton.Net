# TON.NET API Inventory

This document tracks the implementation status of all APIs from the JavaScript SDKs.

## Ton.Core (@ton/core)

### Address Module
- [x] `Address` class with `Parse()`, `ToString()`, `Equals()` ✅ v0.0.1
- [ ] `ExternalAddress` class
- [ ] `ADNLAddress` class
- [ ] `ContractAddress()` function

### BOC (Bag of Cells) Module
- [x] `BitString` class - immutable bit string ✅ v0.0.3
- [x] `BitReader` class - sequential bit reading ✅ v0.0.3
- [x] `BitBuilder` class - sequential bit writing ✅ v0.0.3
- [x] `Builder` class - cell builder with `BeginCell()` ✅ v0.0.4
- [x] `Slice` class - cell reader ✅ v0.0.4
- [x] `Cell` class - fundamental data structure ✅ v0.0.4
- [x] `CellType` enum ✅ v0.0.4
- [x] `LevelMask` class ✅ v0.0.4
- [ ] `Writable` interface

### Dictionary Module
- [x] `Dictionary<TKey, TValue>` class ✅ v0.0.7
- [x] `DictionaryKey<T>` interface ✅ v0.0.7
- [x] `DictionaryValue<T>` interface ✅ v0.0.7
- [x] Built-in key types (Uint, Int, BigInt, BigUint, Address, Buffer, BitString) ✅ v0.0.7
- [x] Built-in value types (Uint, Int, BigInt, Bool, Address, Cell, Buffer, VarUint, BitString, nested Dict) ✅ v0.0.7
- [x] Get/Set/Delete/Has/Keys/Values/Clear/Enumeration ✅ v0.0.7
- [x] StoreDictDirect/LoadDictDirect extensions ✅ v0.0.7
- [x] StoreDict/LoadDict (with ref) extensions ✅ v0.0.7
- [x] 16 comprehensive tests covering all functionality ✅ v0.0.7

### Exotic Cells (Merkle functionality planned for future)
- [x] CellType enum with exotic types (PrunedBranch, MerkleProof, MerkleUpdate) ✅ v0.0.7
- [ ] `GenerateMerkleProof()` - Merkle proof generation (planned)
- [ ] `GenerateMerkleUpdate()` - Merkle update generation (planned)
- [ ] Full exotic cell parsing/validation (planned)

### Tuple Module
- [x] `TupleItem` types (Null, Int, NaN, Cell, Slice, Builder, Tuple) ✅ v0.0.7
- [x] `TupleReader` class with type-safe accessors ✅ v0.0.7
- [x] `TupleBuilder` class for constructing tuples ✅ v0.0.7
- [x] `ParseTuple()` / `SerializeTuple()` functions ✅ v0.0.7
- [x] ReadLispList support for cons-style lists ✅ v0.0.7
- [x] LoadStringTail/StoreStringTail for multi-cell strings ✅ v0.0.7
- [x] 25 comprehensive tests covering all functionality ✅ v0.0.7

### Types Module (TL-B Schemas)
- [ ] `Account` - account data structure
- [ ] `AccountState` - account state (Active, Frozen, Uninitialized)
- [ ] `AccountStatus` enum
- [ ] `AccountStatusChange` - status transition
- [ ] `AccountStorage` - storage info
- [ ] `CommonMessageInfo` - internal/external message info
- [ ] `CommonMessageInfoRelaxed` - relaxed message info
- [ ] `ComputeSkipReason` enum
- [ ] `CurrencyCollection` - currency amounts
- [ ] `DepthBalanceInfo` - account depth/balance
- [ ] `ExtraCurrency` - extra currencies
- [ ] `HashUpdate` - hash update
- [ ] `LibRef` - library reference
- [ ] `MasterchainStateExtra` - masterchain state
- [ ] `Message` - blockchain message
- [ ] `MessageRelaxed` - relaxed message
- [ ] `OutList` - output message list
- [ ] `ReserveMode` enum
- [ ] `SendMode` enum/flags
- [ ] `ShardAccount` - shard account data
- [ ] `ShardAccounts` - collection of shard accounts
- [ ] `ShardIdent` - shard identifier
- [ ] `ShardStateUnsplit` - shard state
- [ ] `SimpleLibrary` - simple library
- [ ] `SplitMergeInfo` - split/merge data
- [ ] `StateInit` - contract initialization state
- [ ] `StorageExtraInfo` - storage extra info
- [ ] `StorageInfo` - storage information
- [ ] `StorageUsed` - storage usage statistics
- [ ] `TickTock` - tick-tock flag
- [ ] `Transaction` - blockchain transaction
- [ ] `TransactionActionPhase` - action phase
- [ ] `TransactionBouncePhase` - bounce phase
- [ ] `TransactionComputePhase` - compute phase
- [ ] `TransactionCreditPhase` - credit phase
- [ ] `TransactionDescription` - full transaction description
- [ ] `TransactionStoragePhase` - storage phase

### Contract Module
- [ ] `Contract` interface
- [ ] `ContractProvider` interface
- [ ] `ContractState` class
- [ ] `Sender` interface
- [ ] `SenderArguments` type
- [ ] `OpenContract<T>()` function
- [ ] `ComputeError` class
- [ ] `ContractABI` types (Error, TypeRef, Field, Argument, Getter, Type, ReceiverMessage, Receiver)

### Utility Functions
- [x] `ToNano()` / `FromNano()` - coin conversion ✅ v0.0.2
- [x] `Crc16()` - CRC16 checksum ✅ v0.0.1
- [x] `Crc32c()` - CRC32C checksum ✅ v0.0.2
- [x] `Base32Encode()` / `Base32Decode()` - base32 encoding ✅ v0.0.2
- [x] `GetMethodId()` - compute method ID from name ✅ v0.0.2

### Crypto Module (minimal in core)
- [ ] `SafeSign()` / `SafeSignVerify()` - safe signature functions

## Ton.Crypto (@ton/crypto)

**Status**: In progress  
**Completion**: ~60% (Primitives, Ed25519, Mnemonic complete; HD Wallet and Passwords remaining)

### Primitives
- [x] `Sha256.Hash()` - SHA-256 hashing ✅ v0.0.5
- [x] `Sha512.Hash()` - SHA-512 hashing ✅ v0.0.5
- [x] `Pbkdf2Sha512.DeriveKey()` - PBKDF2 with SHA-512 ✅ v0.0.5
- [x] `HmacSha512.Hash()` - HMAC with SHA-512 ✅ v0.0.5
- [x] `SecureRandom.GetBytes()` / `SecureRandom.GetNumber()` - secure random generation ✅ v0.0.6

### NaCl (Ed25519)
- [x] `KeyPair` class - public/private key pair ✅ v0.0.6
- [x] `Ed25519.KeyPairFromSeed()` - derive keypair from seed ✅ v0.0.6
- [x] `Ed25519.KeyPairFromSecretKey()` - derive keypair from secret key ✅ v0.0.6
- [x] `Ed25519.Sign()` / `Ed25519.SignVerify()` - Ed25519 signing ✅ v0.0.6
- [x] `SecretBox.Seal()` / `SecretBox.Open()` - authenticated encryption (XSalsa20-Poly1305) ✅ v0.0.6

### Mnemonic (BIP39)
- [x] `Mnemonic.New()` - generate new mnemonic (12/15/18/21/24 words) ✅ v0.0.7
- [x] `Mnemonic.Validate()` - validate mnemonic ✅ v0.0.7
- [x] `Mnemonic.ToPrivateKey()` - derive private key (TON specific) ✅ v0.0.7
- [x] `Mnemonic.ToWalletKey()` - derive wallet key ✅ v0.0.7
- [x] `Mnemonic.ToSeed()` - convert to BIP39 seed ✅ v0.0.7
- [x] `Mnemonic.ToHDSeed()` - convert to HD wallet seed ✅ v0.0.7
- [x] `Mnemonic.ToEntropy()` - convert to entropy ✅ v0.0.7
- [x] `Mnemonic.FromRandomSeed()` - generate deterministic mnemonic from seed ✅ v0.0.7
- [x] `Mnemonic.BytesToMnemonics()` / `Mnemonic.MnemonicIndexesToBytes()` - conversion utilities ✅ v0.0.7
- [x] `Wordlist.Words` - BIP39 English wordlist (2048 words) ✅ v0.0.7

### Password Generation
- [ ] `NewSecureWords()` - generate secure word passphrase
- [ ] `NewSecurePassphrase()` - generate secure passphrase

### HD Wallet (Hierarchical Deterministic)
- [ ] `HDKeysState` class - HD wallet state
- [ ] `GetED25519MasterKeyFromSeed()` - ED25519 master key
- [ ] `DeriveED25519HardenedKey()` - derive hardened key
- [ ] `DeriveEd25519Path()` - derive key from path
- [ ] `GetSymmetricMasterKeyFromSeed()` - symmetric master key
- [ ] `DeriveSymmetricHardenedKey()` - derive symmetric key
- [ ] `DeriveSymmetricPath()` - derive symmetric path
- [ ] `DeriveMnemonicsPath()` - derive from mnemonic path
- [ ] `DeriveMnemonicHardenedKey()` - derive hardened mnemonic key
- [ ] `GetMnemonicsMasterKeyFromSeed()` - mnemonic master key

## Ton.Client (@ton)

### HTTP API Client
- [ ] `HttpApi` class - low-level HTTP API client
- [ ] `TonClient` class - toncenter API v2 client
- [ ] `TonClient4` class - toncenter API v4 client

### Wallet Contracts
- [ ] `WalletContractV1R1` - Wallet V1 Revision 1
- [ ] `WalletContractV1R2` - Wallet V1 Revision 2
- [ ] `WalletContractV1R3` - Wallet V1 Revision 3
- [ ] `WalletContractV2R1` - Wallet V2 Revision 1
- [ ] `WalletContractV2R2` - Wallet V2 Revision 2
- [ ] `WalletContractV3R1` - Wallet V3 Revision 1
- [ ] `WalletContractV3R2` - Wallet V3 Revision 2
- [ ] `WalletContractV4` - Wallet V4 (with plugins)
- [ ] `WalletContractV5Beta` - Wallet V5 Beta
- [ ] `WalletContractV5R1` - Wallet V5 Revision 1

### Jetton (Fungible Tokens)
- [ ] `JettonMaster` - Jetton master contract
- [ ] `JettonWallet` - Jetton wallet contract

### Multisig
- [ ] `MultisigOrder` - multisig order
- [ ] `MultisigOrderBuilder` - multisig order builder
- [ ] `MultisigWallet` - multisig wallet contract

### Elector
- [ ] `ElectorContract` - validator elector contract

### Config Parser
- [ ] `ConfigParse5()` through `ConfigParse40()` - parse specific config params
- [ ] `ParseFullConfig()` - parse complete config
- [ ] `LoadConfigParamById()` - load specific param
- [ ] Type parsers for Gas, Storage, Messages, Validators, Bridges

### Fee Computation
- [ ] `ComputeExternalMessageFees()` - compute external message fees
- [ ] `ComputeFwdFees()` - compute forward fees
- [ ] `ComputeGasPrices()` - compute gas costs
- [ ] `ComputeMessageForwardFees()` - compute message forward fees
- [ ] `ComputeStorageFees()` - compute storage fees

## Implementation Priority

1. **Phase 1: Ton.Core Foundations** ✅ Completed
   - ✅ Address (class), BitString, BitReader, BitBuilder
   - ✅ Cell, Builder, Slice, CellType, LevelMask
   - ✅ Basic utilities (ToNano, FromNano, CRC)

2. **Phase 2: Ton.Crypto**
   - Primitives (SHA, HMAC, PBKDF2)
   - NaCl/Ed25519
   - Mnemonic (BIP39)

3. **Phase 3: Ton.Core Advanced**
   - Dictionary
   - Tuple
   - TL-B Types
   - Contract interfaces

4. **Phase 4: Ton.Client**
   - HTTP API clients
   - Wallet contracts
   - Jetton contracts

5. **Phase 5: Advanced Features**
   - HD Wallets
   - Exotic cells
   - Multisig
   - Config parsing
   - Fee computation

