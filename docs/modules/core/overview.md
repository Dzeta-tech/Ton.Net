# Core Module

The `Ton.Core` module provides fundamental TON blockchain data structures: Cells, Addresses, Types, and utilities.

## Features

- ✅ **Cell manipulation** - Build and parse TON cells
- ✅ **BOC serialization** - Convert cells to/from BOC format
- ✅ **Address parsing** - Handle friendly and raw formats
- ✅ **Type definitions** - Account, Transaction, Message, StateInit
- ✅ **Dictionary support** - Work with TON dictionaries (hash maps)
- ✅ **Tuple operations** - For contract get methods

## Cells and BOC

### Building Cells

Cells are built using the `Builder` class:

```csharp
using Ton.Core.Boc;

Builder builder = Builder.BeginCell();

// Store integers
builder.StoreUint(123, 32);          // 32-bit unsigned
builder.StoreInt(-456, 64);          // 64-bit signed
builder.StoreVarUint(1000, 4);       // Variable-length uint

// Store booleans
builder.StoreBit(true);
builder.StoreBit(false);

// Store addresses
Address addr = Address.Parse("EQCD39VS5jcptHL8vMjEXrzGaRcCVYto7HUn4bpAOg8xqB2N");
builder.StoreAddress(addr);

// Store strings
builder.StoreString("Hello TON");    // UTF-8 string
builder.StoreBuffer(bytes);          // Raw bytes

// Store coins (nanoTON)
builder.StoreCoins(1_000_000_000);   // 1 TON in nanotons

// Store references to other cells
Cell ref1 = Builder.BeginCell().StoreUint(1, 8).EndCell();
Cell ref2 = Builder.BeginCell().StoreUint(2, 8).EndCell();
builder.StoreRef(ref1);
builder.StoreRef(ref2);

// Build the cell
Cell cell = builder.EndCell();
```

### Reading Cells

Cells are read using the `Slice` class:

```csharp
Slice slice = cell.BeginParse();

// Load integers
uint value1 = slice.LoadUint(32);
long value2 = slice.LoadInt(64);
BigInteger value3 = slice.LoadVarUint(4);

// Load booleans
bool flag1 = slice.LoadBit();
bool flag2 = slice.LoadBit();

// Load address
Address addr = slice.LoadAddress();

// Load strings and bytes
string text = slice.LoadString(slice.RemainingBits / 8);
byte[] bytes = slice.LoadBuffer(32);

// Load coins
BigInteger coins = slice.LoadCoins();

// Load references
Cell ref1 = slice.LoadRef();
Cell ref2 = slice.LoadRef();

// Check remaining
Console.WriteLine($"Remaining bits: {slice.RemainingBits}");
Console.WriteLine($"Remaining refs: {slice.RemainingRefs}");
```

### Optional Values

```csharp
// Store optional
Builder builder = Builder.BeginCell();
builder.StoreMaybe(address, (b, a) => b.StoreAddress(a));

// Load optional
Slice slice = cell.BeginParse();
Address? addr = slice.LoadMaybe(s => s.LoadAddress());
```

### Either Type

```csharp
// Store either (left or right)
builder.StoreEither(
    isLeft: true,
    left: 123,
    right: "text",
    storeLeft: (b, val) => b.StoreUint(val, 32),
    storeRight: (b, val) => b.StoreString(val)
);

// Load either
var result = slice.LoadEither(
    loadLeft: s => s.LoadUint(32),
    loadRight: s => s.LoadString(s.RemainingBits / 8)
);

if (result.IsLeft)
    Console.WriteLine($"Left value: {result.Left}");
else
    Console.WriteLine($"Right value: {result.Right}");
```

### BOC Serialization

```csharp
// Serialize to BOC
byte[] boc = cell.ToBoc();

// Save to file
File.WriteAllBytes("contract.boc", boc);

// Deserialize from BOC
byte[] bocData = File.ReadAllBytes("contract.boc");
Cell[] cells = Cell.FromBoc(bocData);
Cell rootCell = cells[0];  // First cell is root
```

### Cell Properties

```csharp
Cell cell = builder.EndCell();

// Basic properties
int bitCount = cell.Bits.Length;
int refCount = cell.Refs.Length;

// Hashing (level-based, 0 is most common)
byte[] hash = cell.Hash(level: 0);
Console.WriteLine($"Cell hash: {Convert.ToHexString(hash)}");

// Depth
int depth = cell.Depth(level: 0);

// Type
CellType type = cell.Type;  // Ordinary, MerkleProof, MerkleUpdate, PrunedBranch
bool isExotic = cell.IsExotic;
```

### Exotic Cells

TON has special cell types for proofs and optimization:

```csharp
// MerkleProof cells (used in lite client responses)
Cell proofCell = Cell.FromBoc(headerProof)[0];

if (proofCell.Type == CellType.MerkleProof)
{
    // Extract actual data from proof
    Cell dataCell = proofCell.UnwrapProof();
    Slice data = dataCell.BeginParse();
    // ... parse actual data
}

// Automatic unwrapping for convenience
Cell unwrapped = cell.UnwrapProof();  // Returns self if not a proof
```

## Addresses

### Parsing Addresses

```csharp
using Ton.Core.Addresses;

// Parse any format
Address addr1 = Address.Parse("EQCD39VS5jcptHL8vMjEXrzGaRcCVYto7HUn4bpAOg8xqB2N");
Address addr2 = Address.Parse("0:83dfd552e63729b472fcbcc8c45ebcc6691702558b68ec7527e1ba403a0f31a8");

// Parse specific format
Address friendly = Address.ParseFriendly("EQCD39VS...").Address;
Address raw = Address.ParseRaw("0:83dfd5...");

// Create from components
Address addr = new Address(
    workchain: 0,
    hash: hashBytes  // 32 bytes
);
```

### Address Validation

```csharp
// Check if valid address
bool isFriendly = Address.IsFriendly("EQCD39VS5jcptHL8vMjEXrzGaRcCVYto7HUn4bpAOg8xqB2N");
bool isRaw = Address.IsRaw("0:83dfd552e63729b472fcbcc8c45ebcc6691702558b68ec7527e1ba403a0f31a8");

// Try parse
if (Address.TryParse("somestring", out Address? addr))
{
    Console.WriteLine($"Valid address: {addr}");
}
```

### Address Components

```csharp
Address addr = Address.Parse("EQCD39VS5jcptHL8vMjEXrzGaRcCVYto7HUn4bpAOg8xqB2N");

// Access components
int workchain = addr.Workchain;  // 0 = basechain, -1 = masterchain
byte[] hash = addr.Hash;         // 32 bytes

// Convert to formats
string friendly = addr.ToString(AddressType.Base64);
string raw = addr.ToString(AddressType.Raw);

// With flags
string bounceable = addr.ToString(AddressType.Base64, bounceableTag: true);      // EQ...
string nonBounceable = addr.ToString(AddressType.Base64, bounceableTag: false);  // UQ...
string testnet = addr.ToString(AddressType.Base64, testOnly: true);
```

### Address Comparison

```csharp
Address addr1 = Address.Parse("EQCD39VS5...");
Address addr2 = Address.Parse("0:83dfd552...");

// Equality (ignores format)
bool areEqual = addr1.Equals(addr2);  // true if same workchain and hash

// Operators
bool same = addr1 == addr2;
bool different = addr1 != addr2;
```

## Coins

Working with TON amounts:

```csharp
using Ton.Core.Utils;
using System.Numerics;

// Create from nanotons
BigInteger nanotons = 1_500_000_000;  // 1.5 TON
Coins coins = Coins.FromNano(nanotons);

// Create from TON string
Coins amount1 = Coins.Parse("1.5");      // 1.5 TON
Coins amount2 = Coins.Parse("0.1");      // 0.1 TON

// Convert to string (human readable)
string formatted = coins.ToString();  // "1.5"

// Get nanotons
BigInteger nano = coins.ToNano();

// Arithmetic
Coins sum = amount1 + amount2;
Coins diff = amount1 - amount2;
Coins product = amount1 * 2;
bool greater = amount1 > amount2;
```

## Dictionaries

TON dictionaries are hash maps stored in cells:

```csharp
using Ton.Core.Dict;
using System.Numerics;

// Create empty dictionary
var dict = TonDict.Dictionary<TonDict.DictKeyUint, BigInteger>.Empty();

// Set values
dict.Set(1u, BigInteger.Parse("1000000"));
dict.Set(2u, BigInteger.Parse("2000000"));
dict.Set(100u, BigInteger.Parse("5000000"));

// Get values
BigInteger? value1 = dict.Get(1u);
if (value1 != null)
{
    Console.WriteLine($"Value for key 1: {value1}");
}

// Check if key exists
bool has = dict.Has(2u);

// Delete key
dict.Delete(2u);

// Get all keys
IEnumerable<uint> keys = dict.Keys;

// Get all values
IEnumerable<BigInteger> values = dict.Values;

// Iterate
foreach (var (key, value) in dict)
{
    Console.WriteLine($"{key}: {value}");
}

// Store in cell
Builder builder = Builder.BeginCell();
dict.Store(builder);
Cell cell = builder.EndCell();

// Load from cell
Slice slice = cell.BeginParse();
var loadedDict = TonDict.Dictionary<TonDict.DictKeyUint, BigInteger>.Load(
    slice,
    32,  // key size in bits
    valLoader: s => s.LoadUint(64)
);
```

### Dictionary Key Types

```csharp
// Uint keys
DictKeyUint key1 = new(123);
DictKeyUint key2 = 456u;  // implicit conversion

// Int keys
DictKeyInt key3 = new(-123);

// Address keys
DictKeyAddress key4 = new(address);

// BigInt keys
DictKeyBigInt key5 = new(BigInteger.Parse("12345678901234567890"));

// Buffer keys
DictKeyBuffer key6 = new(bytes);
```

## Tuples

Tuples are used for contract get method parameters and return values:

```csharp
using Ton.Core.Tuple;

// Create tuple
TupleBuilder tb = new();
tb.WriteNumber(123);
tb.WriteString("hello");
tb.WriteAddress(address);
tb.WriteCell(cell);
tb.WriteSlice(slice);
tb.WriteBoolean(true);

Tuple tuple = tb.Build();

// Read tuple
TupleReader tr = tuple.BeginRead();
BigInteger num = tr.ReadNumber();
string str = tr.ReadString();
Address addr = tr.ReadAddress();
Cell c = tr.ReadCell();
Slice s = tr.ReadSlice();
bool flag = tr.ReadBoolean();

// Nested tuples
TupleBuilder inner = new();
inner.WriteNumber(1);
inner.WriteNumber(2);
tb.WriteTuple(inner.Build());
```

## Types

Core blockchain types:

### StateInit

Contract initialization data:

```csharp
using Ton.Core.Types;

StateInit stateInit = new()
{
    Code = codeCell,      // Contract code
    Data = dataCell,      // Initial data
    Libraries = null,     // Optional libraries
};

// Store in cell
Builder builder = Builder.BeginCell();
stateInit.Store(builder);
Cell cell = builder.EndCell();

// Load from cell
Slice slice = cell.BeginParse();
StateInit loaded = StateInit.Load(slice);
```

### Message

Internal/external messages:

```csharp
// Internal message (between contracts)
Message msg = new Message.InternalMessage
{
    Info = new CommonMessageInfoIntRelaxed
    {
        IhrDisabled = true,
        Bounce = true,
        Bounced = false,
        Src = senderAddress,
        Dest = destAddress,
        Value = Coins.FromNano(1_000_000_000),  // 1 TON
        IhrFee = BigInteger.Zero,
        FwdFee = BigInteger.Zero,
        CreatedLt = 0,
        CreatedAt = 0
    },
    Body = bodyCell
};

// External message (from outside)
Message extMsg = new Message.ExternalInMessage
{
    Info = new CommonMessageInfoExternalIn
    {
        Src = ExternalAddress.None,
        Dest = destAddress,
        ImportFee = BigInteger.Zero
    },
    Body = bodyCell
};
```

### Account

Account state representation:

```csharp
Account account = Account.Load(slice);

Console.WriteLine($"Address: {account.Address}");
Console.WriteLine($"Balance: {account.Storage.Balance.Coins}");

if (account.Storage.State is AccountState.Active activeState)
{
    Cell? code = activeState.State?.Code;
    Cell? data = activeState.State?.Data;
    Console.WriteLine("Account is active");
}
```

### Transaction

Transaction data:

```csharp
Transaction tx = Transaction.Load(slice, cell);

Console.WriteLine($"Account: {tx.AccountAddr}");
Console.WriteLine($"LT: {tx.Lt}");
Console.WriteLine($"Hash: {Convert.ToHexString(tx.Hash)}");
Console.WriteLine($"Now: {tx.Now}");

// Transaction description
if (tx.Description is TransactionDescription.Ordinary ord)
{
    // Parse ordinary transaction
    Console.WriteLine($"Destroyed: {ord.Destroyed}");
    // ... more fields
}
```

## Utilities

### Bits

Low-level bit manipulation:

```csharp
using Ton.Core.Boc;

// Create bit string
BitString bits = new(new byte[] { 0b10101010 }, 8);

// Bit builder (dynamic)
BitBuilder bb = new();
bb.WriteBit(true);
bb.WriteBit(false);
bb.WriteUint(123, 8);
BitString result = bb.Build();

// Bit reader
BitReader br = new(bits);
bool bit1 = br.LoadBit();
uint value = br.LoadUint(8);
```

### Hashing

```csharp
using Ton.Crypto.Primitives;

// SHA-256
byte[] hash = Sha256.Hash(data);

// SHA-512
byte[] hash512 = Sha512.Hash(data);

// HMAC-SHA512
byte[] hmac = HmacSha512.Hash(message, key);
```

## Best Practices

### 1. Reuse Builders

```csharp
// ❌ Avoid: Creating many builders
for (int i = 0; i < 100; i++)
{
    Builder b = Builder.BeginCell();
    // ...
}

// ✅ Good: Reuse when possible
Builder builder = Builder.BeginCell();
for (int i = 0; i < 100; i++)
{
    builder.StoreUint((uint)i, 32);
}
```

### 2. Check Remaining Space

```csharp
Builder builder = Builder.BeginCell();

// Check before storing
if (builder.AvailableBits >= 256)
{
    builder.StoreUint(value, 256);
}

if (builder.AvailableRefs >= 1)
{
    builder.StoreRef(cell);
}
```

### 3. Validate Addresses

```csharp
// ✅ Good: Validate user input
string userInput = GetUserInput();
if (Address.TryParse(userInput, out Address? addr))
{
    // Use addr
}
else
{
    Console.WriteLine("Invalid address");
}
```

### 4. Handle BOC Errors

```csharp
try
{
    Cell[] cells = Cell.FromBoc(bocData);
}
catch (ArgumentException ex)
{
    Console.WriteLine($"Invalid BOC: {ex.Message}");
}
```

## See Also

- [LiteClient Module](../liteclient/overview.md)
- [Crypto Module](../crypto/overview.md)
- [Contracts Module](../contracts/overview.md)
