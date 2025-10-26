# TON.NET API Inventory

This document tracks the implementation status of all APIs from the JavaScript SDKs.

## Ton.Core (@ton/core)

### Address Module
- [x] `Address` class with `Parse()`, `ToString()`, `Equals()` ✅ v0.0.1
- [ ] `ExternalAddress` class
- [ ] `ADNLAddress` class
- [ ] `ContractAddress()` function

### BOC (Bag of Cells) Module
- [ ] `BitString` class - immutable bit string
- [ ] `BitReader` class - sequential bit reading
- [ ] `BitBuilder` class - sequential bit writing
- [ ] `Builder` class - cell builder with `BeginCell()`
- [ ] `Slice` class - cell reader
- [ ] `Cell` class - fundamental data structure
- [ ] `CellType` enum
- [ ] `Writable` interface

### Dictionary Module
- [ ] `Dictionary<TKey, TValue>` class
- [ ] `DictionaryKey<T>` interface
- [ ] `DictionaryValue<T>` interface
- [ ] Built-in key types (Int, Uint, Address, Buffer, BigInt)

### Exotic Cells
- [ ] `ExoticMerkleProof` - Merkle proof cells
- [ ] `ExoticMerkleUpdate` - Merkle update cells
- [ ] `ExoticPruned` - Pruned branch cells
- [ ] `GenerateMerkleProof()` function
- [ ] `GenerateMerkleUpdate()` function

### Tuple Module
- [ ] `Tuple` type
- [ ] `TupleItem` types (Null, Int, NaN, Cell, Slice, Builder)
- [ ] `TupleReader` class
- [ ] `TupleBuilder` class
- [ ] `ParseTuple()` / `SerializeTuple()` functions

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

### Primitives
- [ ] `Sha256()` / `Sha256Sync()` - SHA-256 hashing
- [ ] `Sha512()` / `Sha512Sync()` - SHA-512 hashing
- [ ] `Pbkdf2Sha512()` - PBKDF2 with SHA-512
- [ ] `HmacSha512()` - HMAC with SHA-512
- [ ] `GetSecureRandomBytes()` / `GetSecureRandomWords()` / `GetSecureRandomNumber()` - secure random generation

### NaCl (Ed25519)
- [ ] `KeyPair` struct - public/private key pair
- [ ] `KeyPairFromSeed()` - derive keypair from seed
- [ ] `KeyPairFromSecretKey()` - derive keypair from secret key
- [ ] `Sign()` / `SignVerify()` - Ed25519 signing
- [ ] `SealBox()` / `OpenBox()` - authenticated encryption

### Mnemonic (BIP39)
- [ ] `MnemonicNew()` - generate new mnemonic (12/15/18/21/24 words)
- [ ] `MnemonicValidate()` - validate mnemonic
- [ ] `MnemonicToPrivateKey()` - derive private key (TON specific)
- [ ] `MnemonicToWalletKey()` - derive wallet key
- [ ] `MnemonicToSeed()` - convert to BIP39 seed
- [ ] `MnemonicToHDSeed()` - convert to HD wallet seed
- [ ] `MnemonicWordList` - BIP39 word list

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

1. **Phase 1: Ton.Core Foundations**
   - Address, BitString, BitReader, BitBuilder
   - Cell, Builder, Slice
   - Basic utilities (ToNano, FromNano, CRC)

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

