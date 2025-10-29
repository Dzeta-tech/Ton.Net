# Key Concepts

This page explains core TON blockchain concepts and how they're represented in Ton.NET.

## Addresses

TON addresses identify accounts on the blockchain.

### Address Formats

- **Friendly** (Base64): User-friendly format with checksum
  - Example: `EQCD39VS5jcptHL8vMjEXrzGaRcCVYto7HUn4bpAOg8xqB2N`
  - Contains: bounceable flag, testnet flag, workchain, hash, checksum
  
- **Raw** (Workchain:Hash): Technical format
  - Example: `0:83dfd552e63729b472fcbcc8c45ebcc6691702558b68ec7527e1ba403a0f31a8`
  - Format: `{workchain}:{hash_in_hex}`

```csharp
using Ton.Core.Addresses;

// Parse any format
Address addr = Address.Parse("EQCD39VS5jcptHL8vMjEXrzGaRcCVYto7HUn4bpAOg8xqB2N");

// Access components
int workchain = addr.Workchain;  // 0 = basechain, -1 = masterchain
byte[] hash = addr.Hash;         // 32 bytes

// Convert formats
string friendly = addr.ToString(AddressType.Base64, bounceableTag: true);
string raw = addr.ToString(AddressType.Raw);
```

### Bounceable vs Non-Bounceable

- **Bounceable** (`EQ...`): For smart contracts - returns funds if transaction fails
- **Non-Bounceable** (`UQ...`): For wallets - keeps funds even if no code

```csharp
// Parse with metadata
var (isBounceable, isTestOnly, address) = Address.ParseFriendly("EQCD...");

// Create bounceable/non-bounceable strings
string bounceable = address.ToString(AddressType.Base64, bounceableTag: true);
string nonBounceable = address.ToString(AddressType.Base64, bounceableTag: false);
```

## Cells and BOC

Cells are the fundamental data structure in TON. Everything (contracts, messages, transactions) is stored as cells.

### Cell Structure

- **Up to 1023 bits** of data
- **Up to 4 references** to other cells
- Forms a **Directed Acyclic Graph (DAG)**

```csharp
using Ton.Core.Boc;

// Build a cell
Builder builder = Builder.BeginCell();
builder.StoreUint(123, 32);         // Store 32-bit integer
builder.StoreAddress(address);       // Store address
builder.StoreBit(true);             // Store boolean

Cell cell = builder.EndCell();
```

### Reading Cells

```csharp
Slice slice = cell.BeginParse();

uint value = slice.LoadUint(32);
Address addr = slice.LoadAddress();
bool flag = slice.LoadBit();
```

### BOC (Bag of Cells)

BOC is the serialization format for cells used in TON:

```csharp
// Serialize to BOC
byte[] boc = cell.ToBoc();

// Deserialize from BOC
Cell[] cells = Cell.FromBoc(boc);
Cell rootCell = cells[0];
```

## Blocks and Shards

TON uses sharding for scalability.

### Workchains

- **Masterchain** (workchain -1): Coordinates other chains, stores config
- **Basechain** (workchain 0): Main chain for user contracts

### BlockId

Uniquely identifies a block:

```csharp
BlockId blockId = new BlockId(
    workchain: -1,
    shard: -9223372036854775808,  // full shard
    seqno: 1000000,
    rootHash: [...],
    fileHash: [...]
);
```

## Lite Client Architecture

### Engines

Engines handle low-level ADNL protocol communication:

- **LiteSingleEngine**: Connects to one server
- **LiteRoundRobinEngine**: Load balances across multiple servers

```csharp
// Single server
LiteClient client = LiteClientFactory.Create("65.109.14.188", 14432, publicKey);

// Multiple servers (automatic from config)
LiteClient client = await LiteClientFactory.CreateFromUrlAsync(
    "https://ton.org/global-config.json"
);
```

### Connection Behavior

- **Automatic connection**: Connects on first request
- **Automatic reconnection**: Reconnects if connection drops
- **No manual management**: Just call methods

## Account States

TON accounts have four states:

### NonExist
Account doesn't exist on blockchain yet.

### Uninitialized
Address exists but no code deployed:
- Has balance
- Can receive TON
- Cannot execute code

### Active
Fully functional contract:
- Has code and data
- Can process messages
- Can execute get methods

### Frozen
Contract is frozen (rare):
- Cannot process messages
- Balance locked

```csharp
AccountState state = await client.GetAccountStateAsync(address, block);

switch (state.State)
{
    case AccountStorageState.NonExist:
        Console.WriteLine("Account does not exist");
        break;
    case AccountStorageState.Uninitialized:
        Console.WriteLine($"Uninitialized account with {state.BalanceInTon} TON");
        break;
    case AccountStorageState.Active:
        Console.WriteLine("Active contract");
        Console.WriteLine($"Code hash: {Convert.ToHexString(state.Code!.Hash(0))}");
        break;
    case AccountStorageState.Frozen:
        Console.WriteLine("Frozen account");
        break;
}
```

## Transactions and Logical Time

### Logical Time (LT)

- **Monotonically increasing** counter
- **Unique per account** transaction ordering
- **Not wall-clock time**

```csharp
if (state.LastTransaction != null)
{
    Console.WriteLine($"Last TX LT: {state.LastTransaction.Lt}");
    Console.WriteLine($"Last TX Hash: {Convert.ToHexString(state.LastTransaction.Hash)}");
}
```

### Unix Time (utime)

- **Wall-clock timestamp** of block creation
- **Not precise** - approximate time
- **Used for time-based lookups**

```csharp
// Find block at specific time
BlockId block = await client.LookupBlockByUtimeAsync(-1, shard, unixTimestamp);
```

## Message Types

TON has three message types:

### Internal Messages
Between contracts on-chain:
- Can carry TON
- Can carry data/code
- Can bounce back if fails

### External Messages
From outside world to blockchain:
- Typically wallet operations
- No automatic reply
- Must pay for gas

### External Out Messages
From blockchain to outside:
- Events/logs
- Cannot be processed on-chain

## Mnemonics and Keys

### BIP39 Mnemonics

```csharp
using Ton.Crypto.Mnemonic;
using Ton.Crypto.Ed25519;

// Generate new mnemonic
string[] mnemonic = Mnemonic.New(24);  // 24 words

// Validate mnemonic
bool isValid = Mnemonic.Validate(mnemonic);

// Derive key pair
KeyPair keys = Mnemonic.ToWalletKey(mnemonic);
byte[] publicKey = keys.PublicKey;   // 32 bytes
byte[] secretKey = keys.SecretKey;   // 64 bytes
```

### Key Security

- **Never** store private keys in code
- **Never** commit mnemonics to source control
- **Use** environment variables or secure storage
- **Derive** addresses from public keys only

## Next Steps

Continue learning with module-specific guides:

- [LiteClient Module](../modules/liteclient/overview.md) - Blockchain queries
- [Core Module](../modules/core/overview.md) - Cells, addresses, types
- [Crypto Module](../modules/crypto/overview.md) - Cryptography operations
- [Contracts Module](../modules/contracts/overview.md) - Wallet operations
