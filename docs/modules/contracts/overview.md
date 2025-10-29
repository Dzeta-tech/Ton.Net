# Contracts Module

The `Ton.Contracts` module provides wallet contract implementations for sending and receiving TON.

## Features

- ✅ **Wallet V5R1** - Latest wallet version with plugins and actions
- ✅ **Type-safe API** - Strongly typed message builders
- ✅ **Multiple operations** - Batch multiple transfers in one transaction
- ✅ **Plugin support** - Extensible architecture (V5 only)

## Wallet Types

TON supports multiple wallet versions. Each version has different features and gas costs.

### WalletV5R1 (Recommended)

Latest wallet with plugins and actions support:
- **Plugin system** for extensibility
- **Actions** for complex operations
- **Optimized gas usage**
- **Future-proof**

```csharp
using Ton.Contracts.Wallets.V5;
using Ton.Crypto.Mnemonic;

// Generate keys
string[] mnemonic = Mnemonic.New(24);
KeyPair keys = Mnemonic.ToWalletKey(mnemonic);

// Create wallet
WalletV5R1 wallet = new(keys.PublicKey);
Address address = wallet.Address;

Console.WriteLine($"Wallet address: {address}");
```

## Creating a Wallet

### From New Mnemonic

```csharp
using Ton.Contracts.Wallets.V5;
using Ton.Crypto.Mnemonic;
using Ton.Crypto.Ed25519;

// Generate mnemonic
string[] mnemonic = Mnemonic.New(24);

// Derive keys
KeyPair keys = Mnemonic.ToWalletKey(mnemonic);

// Create wallet contract
WalletV5R1 wallet = new(
    publicKey: keys.PublicKey,
    workchain: 0,  // 0 = basechain (cheaper), -1 = masterchain
    walletId: null  // null = default (matches TON Connect standard)
);

Address address = wallet.Address;

Console.WriteLine($"Mnemonic: {string.Join(" ", mnemonic)}");
Console.WriteLine($"Address: {address}");
Console.WriteLine($"Public key: {Convert.ToBase64String(keys.PublicKey)}");
```

### From Existing Mnemonic

```csharp
// User provides mnemonic
string[] mnemonic = GetMnemonicFromUser();

// Validate
if (!Mnemonic.Validate(mnemonic))
{
    throw new ArgumentException("Invalid mnemonic");
}

// Restore wallet
KeyPair keys = Mnemonic.ToWalletKey(mnemonic);
WalletV5R1 wallet = new(keys.PublicKey);
Address address = wallet.Address;
```

### Custom Wallet ID

```csharp
// Network-specific wallet ID
int customWalletId = WalletV5R1WalletId.Create(
    networkGlobalId: -239,  // -239 = mainnet, -3 = testnet
    workchain: 0,
    subwalletNumber: 0,
    version: 0
);

WalletV5R1 wallet = new(keys.PublicKey, walletId: customWalletId);
```

## Checking Balance

```csharp
using Ton.LiteClient;

// Get client
LiteClient client = await LiteClientFactory.CreateFromUrlAsync(
    "https://ton.org/global-config.json"
);

// Get latest block
MasterchainInfo info = await client.GetMasterchainInfoAsync();

// Check balance
AccountState state = await client.GetAccountStateAsync(wallet.Address, info.Last);

Console.WriteLine($"Balance: {state.BalanceInTon:F4} TON");
Console.WriteLine($"State: {state.State}");
Console.WriteLine($"Is deployed: {state.IsActive}");
```

## Deploying a Wallet

Before you can send transactions, the wallet contract must be deployed (initialized) on-chain.

```csharp
using Ton.Core.Boc;
using Ton.Core.Types;

// Check if already deployed
AccountState state = await client.GetAccountStateAsync(wallet.Address, info.Last);

if (!state.IsActive)
{
    Console.WriteLine("Wallet not deployed. Deploy by sending any transaction.");
    // Wallet will auto-deploy on first outgoing transaction
}

// Get deploy message (also used for first transaction)
// Note: Wallet V5 automatically deploys when you send first transaction
// You don't need a separate deploy step
```

## Sending Transactions

### Simple Transfer

```csharp
// Destination
Address destination = Address.Parse("EQCD39VS5jcptHL8vMjEXrzGaRcCVYto7HUn4bpAOg8xqB2N");
decimal amount = 0.5m;  // 0.5 TON

// Get current seqno
AccountState state = await client.GetAccountStateAsync(wallet.Address, info.Last);
int seqno = 0;  // 0 for first transaction (undeployed wallet)

if (state.IsActive && state.Data != null)
{
    // Parse seqno from wallet data
    Slice dataSlice = state.Data.BeginParse();
    seqno = (int)dataSlice.LoadUint(32);  // First 32 bits is seqno
}

// Create transfer message
Cell body = wallet.CreateTransferBody(
    privateKey: keys.SecretKey,
    walletId: wallet.WalletId,
    validUntil: DateTimeOffset.UtcNow.AddMinutes(5).ToUnixTimeSeconds(),
    seqno: seqno,
    actions: new[]
    {
        WalletV5OutActions.SendMessage(
            mode: SendMode.PayGasSeparately | SendMode.IgnoreErrors,
            message: new Message.InternalMessage
            {
                Info = new CommonMessageInfoIntRelaxed
                {
                    IhrDisabled = true,
                    Bounce = true,
                    Bounced = false,
                    Src = wallet.Address,
                    Dest = destination,
                    Value = Coins.FromNano((long)(amount * 1_000_000_000)),
                    IhrFee = BigInteger.Zero,
                    FwdFee = BigInteger.Zero,
                    CreatedLt = 0,
                    CreatedAt = 0
                },
                Body = Builder.BeginCell().EndCell()  // Empty body
            }
        )
    }
);

// Create external message
Message externalMessage = new Message.ExternalInMessage
{
    Info = new CommonMessageInfoExternalIn
    {
        Src = ExternalAddress.None,
        Dest = wallet.Address,
        ImportFee = BigInteger.Zero
    },
    Body = body
};

// Serialize and send
Builder messageBuilder = Builder.BeginCell();
externalMessage.Store(messageBuilder);
Cell messageCell = messageBuilder.EndCell();
byte[] boc = messageCell.ToBoc();

// Send via HTTP client or lite client
// (Implementation depends on which client you're using)
```

### Transfer with Comment

```csharp
// Create comment cell
string comment = "Payment for services";
Cell commentCell = Builder.BeginCell()
    .StoreUint(0, 32)  // Text comment opcode
    .StoreString(comment)
    .EndCell();

// Use in message body
var action = WalletV5OutActions.SendMessage(
    mode: SendMode.PayGasSeparately | SendMode.IgnoreErrors,
    message: new Message.InternalMessage
    {
        Info = new CommonMessageInfoIntRelaxed
        {
            IhrDisabled = true,
            Bounce = true,
            Bounced = false,
            Src = wallet.Address,
            Dest = destination,
            Value = Coins.FromNano((long)(amount * 1_000_000_000)),
            IhrFee = BigInteger.Zero,
            FwdFee = BigInteger.Zero,
            CreatedLt = 0,
            CreatedAt = 0
        },
        Body = commentCell
    }
);
```

### Multiple Transfers (Batch)

```csharp
var actions = new[]
{
    // Transfer 1
    WalletV5OutActions.SendMessage(
        mode: SendMode.PayGasSeparately | SendMode.IgnoreErrors,
        message: new Message.InternalMessage
        {
            Info = new CommonMessageInfoIntRelaxed
            {
                IhrDisabled = true,
                Bounce = true,
                Bounced = false,
                Src = wallet.Address,
                Dest = recipient1,
                Value = Coins.FromNano(1_000_000_000),  // 1 TON
                IhrFee = BigInteger.Zero,
                FwdFee = BigInteger.Zero,
                CreatedLt = 0,
                CreatedAt = 0
            },
            Body = Builder.BeginCell().EndCell()
        }
    ),
    
    // Transfer 2
    WalletV5OutActions.SendMessage(
        mode: SendMode.PayGasSeparately | SendMode.IgnoreErrors,
        message: new Message.InternalMessage
        {
            Info = new CommonMessageInfoIntRelaxed
            {
                IhrDisabled = true,
                Bounce = true,
                Bounced = false,
                Src = wallet.Address,
                Dest = recipient2,
                Value = Coins.FromNano(500_000_000),  // 0.5 TON
                IhrFee = BigInteger.Zero,
                FwdFee = BigInteger.Zero,
                CreatedLt = 0,
                CreatedAt = 0
            },
            Body = Builder.BeginCell().EndCell()
        }
    )
};

Cell body = wallet.CreateTransferBody(
    privateKey: keys.SecretKey,
    walletId: wallet.WalletId,
    validUntil: DateTimeOffset.UtcNow.AddMinutes(5).ToUnixTimeSeconds(),
    seqno: seqno,
    actions: actions
);
```

## Send Modes

Control how TON handles the transaction:

```csharp
// Common send modes
SendMode mode = SendMode.PayGasSeparately | SendMode.IgnoreErrors;

// Available modes:
// - None: Basic send
// - PayGasSeparately: Pay transfer fees separately from message value
// - IgnoreErrors: Don't revert transaction if this message fails
// - DestroyIfZero: Destroy account if balance becomes zero
// - CarryAllBalance: Send all remaining balance
// - CarryAllBalanceDestroyIfZero: Send all and destroy account
```

## Wallet ID

Wallet ID prevents replay attacks across different networks:

```csharp
// Default wallet ID (recommended)
int defaultId = wallet.WalletId;

// Custom wallet ID
int customId = WalletV5R1WalletId.Create(
    networkGlobalId: -239,  // Mainnet
    workchain: 0,
    subwalletNumber: 0,
    version: 0
);

// Multiple subwallets from same keys
WalletV5R1 wallet1 = new(keys.PublicKey, walletId: WalletV5R1WalletId.Create(
    networkGlobalId: -239, workchain: 0, subwalletNumber: 0, version: 0
));
WalletV5R1 wallet2 = new(keys.PublicKey, walletId: WalletV5R1WalletId.Create(
    networkGlobalId: -239, workchain: 0, subwalletNumber: 1, version: 0
));

// Different addresses, same keys
Console.WriteLine($"Wallet 1: {wallet1.Address}");
Console.WriteLine($"Wallet 2: {wallet2.Address}");
```

## Transaction Lifecycle

1. **Create transaction** with seqno and actions
2. **Sign** with private key
3. **Serialize** to BOC
4. **Send** to blockchain
5. **Wait** for confirmation
6. **Increment seqno** for next transaction

```csharp
// 1. Get current seqno
int seqno = GetCurrentSeqno(wallet.Address);

// 2. Create and sign
Cell body = wallet.CreateTransferBody(
    privateKey: keys.SecretKey,
    walletId: wallet.WalletId,
    validUntil: DateTimeOffset.UtcNow.AddMinutes(5).ToUnixTimeSeconds(),
    seqno: seqno,
    actions: actions
);

// 3. Create external message
Message externalMessage = new Message.ExternalInMessage
{
    Info = new CommonMessageInfoExternalIn
    {
        Src = ExternalAddress.None,
        Dest = wallet.Address,
        ImportFee = BigInteger.Zero
    },
    Body = body
};

// 4. Serialize
Builder messageBuilder = Builder.BeginCell();
externalMessage.Store(messageBuilder);
Cell messageCell = messageBuilder.EndCell();
byte[] boc = messageCell.ToBoc();

// 5. Send (implementation depends on client)
await SendBoc(boc);

// 6. Wait for confirmation (poll seqno or use transaction monitoring)
await WaitForSeqnoIncrease(wallet.Address, seqno);
```

## Best Practices

### 1. Always Validate Addresses

```csharp
// ✅ Good
string userInput = GetAddressFromUser();
if (!Address.TryParse(userInput, out Address? destination))
{
    throw new ArgumentException("Invalid address");
}
```

### 2. Check Balance Before Sending

```csharp
// ✅ Good
AccountState state = await client.GetAccountStateAsync(wallet.Address, block);
decimal balance = state.BalanceInTon;
decimal amountToSend = 1.0m;
decimal estimatedFee = 0.01m;

if (balance < amountToSend + estimatedFee)
{
    throw new InvalidOperationException("Insufficient balance");
}
```

### 3. Use Appropriate Timeout

```csharp
// ✅ Good: 5 minutes is reasonable
long validUntil = DateTimeOffset.UtcNow.AddMinutes(5).ToUnixTimeSeconds();

// ❌ Too short (may expire before confirmation)
long tooShort = DateTimeOffset.UtcNow.AddSeconds(30).ToUnixTimeSeconds();

// ❌ Too long (security risk if private key compromised)
long tooLong = DateTimeOffset.UtcNow.AddDays(1).ToUnixTimeSeconds();
```

### 4. Handle Seqno Correctly

```csharp
// ✅ Good: Get fresh seqno for each transaction
int seqno = await GetCurrentSeqno(wallet.Address);

// ❌ Bad: Reusing old seqno
// int seqno = cachedSeqno;  // DON'T!
```

### 5. Set Bounce Flag Appropriately

```csharp
// ✅ For contracts (bounce back if fails)
Bounce = true

// ✅ For wallets (keep TON even if no code)
Bounce = false
```

### 6. Secure Private Keys

```csharp
// ✅ Good: Clear sensitive data
try
{
    Cell body = wallet.CreateTransferBody(keys.SecretKey, ...);
    // ... send transaction
}
finally
{
    Array.Clear(keys.SecretKey, 0, keys.SecretKey.Length);
}
```

## Common Patterns

### Get Current Seqno

```csharp
async Task<int> GetCurrentSeqno(Address walletAddress)
{
    MasterchainInfo info = await client.GetMasterchainInfoAsync();
    AccountState state = await client.GetAccountStateAsync(walletAddress, info.Last);
    
    if (!state.IsActive || state.Data == null)
    {
        return 0;  // Undeployed wallet
    }
    
    Slice slice = state.Data.BeginParse();
    return (int)slice.LoadUint(32);
}
```

### Wait for Transaction Confirmation

```csharp
async Task WaitForSeqnoIncrease(Address address, int oldSeqno, int maxAttempts = 30)
{
    for (int i = 0; i < maxAttempts; i++)
    {
        await Task.Delay(2000);  // Wait 2 seconds
        
        int currentSeqno = await GetCurrentSeqno(address);
        if (currentSeqno > oldSeqno)
        {
            Console.WriteLine("✓ Transaction confirmed");
            return;
        }
    }
    
    throw new TimeoutException("Transaction not confirmed");
}
```

## See Also

- [LiteClient Module](../liteclient/overview.md)
- [Crypto Module](../crypto/overview.md)
- [Core Module](../core/overview.md)
