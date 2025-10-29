# Crypto Module

The `Ton.Crypto` module provides cryptographic primitives for TON blockchain: Ed25519 signatures, mnemonic phrases, and hashing functions.

## Features

- ✅ **BIP39 Mnemonics** - TON-specific mnemonic generation and validation
- ✅ **Ed25519** - Signing and verification
- ✅ **Hashing** - SHA-256, SHA-512, HMAC-SHA512
- ✅ **Key Derivation** - PBKDF2-SHA512
- ✅ **Secure Random** - Cryptographically secure RNG

## Mnemonics

TON uses BIP39-compatible mnemonics with TON-specific validation.

### Generate Mnemonic

```csharp
using Ton.Crypto.Mnemonic;

// Generate 24-word mnemonic (recommended)
string[] mnemonic = Mnemonic.New(24);

// Generate 12-word mnemonic
string[] shortMnemonic = Mnemonic.New(12);

// With password protection
string[] protectedMnemonic = Mnemonic.New(24, password: "my-secure-password");

// Display to user
Console.WriteLine(string.Join(" ", mnemonic));
```

### Validate Mnemonic

```csharp
string[] mnemonic = GetMnemonicFromUser();

// Validate without password
if (Mnemonic.Validate(mnemonic))
{
    Console.WriteLine("✓ Valid mnemonic");
}
else
{
    Console.WriteLine("✗ Invalid mnemonic");
}

// Validate with password
if (Mnemonic.Validate(mnemonic, password: "my-secure-password"))
{
    Console.WriteLine("✓ Valid mnemonic with password");
}
```

### Derive Keys

```csharp
using Ton.Crypto.Ed25519;

// Get wallet key pair (for signing transactions)
KeyPair walletKeys = Mnemonic.ToWalletKey(mnemonic);
byte[] publicKey = walletKeys.PublicKey;   // 32 bytes
byte[] secretKey = walletKeys.SecretKey;   // 64 bytes (private + public)

// Get private key (master key)
KeyPair privateKey = Mnemonic.ToPrivateKey(mnemonic);

// Get HD seed (for hierarchical deterministic wallets)
byte[] hdSeed = Mnemonic.ToHdSeed(mnemonic);  // 64 bytes
```

### Mnemonic to Address

```csharp
using Ton.Contracts.Wallets;
using Ton.Core.Addresses;

// Generate mnemonic
string[] mnemonic = Mnemonic.New(24);

// Get wallet keys
KeyPair keys = Mnemonic.ToWalletKey(mnemonic);

// Create wallet contract
WalletV4R2 wallet = new(keys.PublicKey);
Address address = wallet.Address;

Console.WriteLine($"Mnemonic: {string.Join(" ", mnemonic)}");
Console.WriteLine($"Address: {address}");
```

### Mnemonic from Seed

Create deterministic mnemonic from seed:

```csharp
byte[] randomSeed = new byte[32];
using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
rng.GetBytes(randomSeed);

string[] mnemonic = Mnemonic.FromRandomSeed(randomSeed, 24);
```

### Convert to Entropy

```csharp
// Get 64-byte entropy from mnemonic
byte[] entropy = Mnemonic.ToEntropy(mnemonic);

// With password
byte[] entropyWithPassword = Mnemonic.ToEntropy(mnemonic, password: "secret");
```

### Convert to Seed

```csharp
// Get seed for specific purpose
byte[] tonSeed = Mnemonic.ToSeed(mnemonic, "TON default seed");
byte[] hdSeed = Mnemonic.ToSeed(mnemonic, "TON HD Keys seed");

// With password
byte[] protectedSeed = Mnemonic.ToSeed(
    mnemonic, 
    "TON default seed", 
    password: "secret"
);
```

## Ed25519

Digital signatures using Ed25519.

### Generate Key Pair

```csharp
using Ton.Crypto.Ed25519;

// Generate random key pair
KeyPair keys = Ed25519.GenerateKeyPair();

byte[] publicKey = keys.PublicKey;   // 32 bytes
byte[] secretKey = keys.SecretKey;   // 64 bytes
```

### Key Pair from Seed

```csharp
// Deterministic key pair from 32-byte seed
byte[] seed = new byte[32];
// ... fill seed ...

KeyPair keys = Ed25519.KeyPairFromSeed(seed);
```

### Sign Message

```csharp
KeyPair keys = Ed25519.GenerateKeyPair();
byte[] message = Encoding.UTF8.GetBytes("Hello TON");

// Sign
byte[] signature = Ed25519.Sign(message, keys.SecretKey);
// signature is 64 bytes

Console.WriteLine($"Signature: {Convert.ToBase64String(signature)}");
```

### Verify Signature

```csharp
byte[] message = Encoding.UTF8.GetBytes("Hello TON");
byte[] signature = ...; // 64 bytes
byte[] publicKey = ...; // 32 bytes

// Verify
bool isValid = Ed25519.Verify(signature, message, publicKey);

if (isValid)
{
    Console.WriteLine("✓ Signature valid");
}
else
{
    Console.WriteLine("✗ Signature invalid");
}
```

### Extract Public Key

```csharp
byte[] secretKey = ...; // 64 bytes

// Extract public key from secret key
byte[] publicKey = Ed25519.ExtractPublicKey(secretKey);  // 32 bytes
```

## Hashing

Cryptographic hash functions.

### SHA-256

```csharp
using Ton.Crypto.Primitives;

byte[] data = Encoding.UTF8.GetBytes("Hello TON");

// Compute hash
byte[] hash = Sha256.Hash(data);  // 32 bytes

Console.WriteLine($"SHA-256: {Convert.ToHexString(hash)}");
```

### SHA-512

```csharp
byte[] data = Encoding.UTF8.GetBytes("Hello TON");

// Compute hash
byte[] hash = Sha512.Hash(data);  // 64 bytes

Console.WriteLine($"SHA-512: {Convert.ToHexString(hash)}");
```

### HMAC-SHA512

```csharp
byte[] message = Encoding.UTF8.GetBytes("Hello TON");
byte[] key = Encoding.UTF8.GetBytes("secret-key");

// Compute HMAC
byte[] hmac = HmacSha512.Hash(message, key);  // 64 bytes

Console.WriteLine($"HMAC-SHA512: {Convert.ToHexString(hmac)}");
```

### PBKDF2-SHA512

Key derivation function:

```csharp
byte[] password = Encoding.UTF8.GetBytes("my-password");
byte[] salt = Encoding.UTF8.GetBytes("salt");
int iterations = 100000;
int keyLength = 64;

// Derive key
byte[] derivedKey = Pbkdf2Sha512.DeriveKey(password, salt, iterations, keyLength);

Console.WriteLine($"Derived key: {Convert.ToHexString(derivedKey)}");
```

## Secure Random

Cryptographically secure random number generation:

```csharp
using Ton.Crypto.Primitives;

// Random integer in range [min, max)
int randomNum = SecureRandom.GetNumber(0, 100);

// Random bytes
byte[] randomBytes = new byte[32];
SecureRandom.GetBytes(randomBytes);
```

## NaCl SecretBox

Authenticated encryption:

```csharp
using Ton.Crypto.Primitives;

byte[] message = Encoding.UTF8.GetBytes("Secret message");
byte[] key = new byte[32];    // 256-bit key
byte[] nonce = new byte[24];  // 192-bit nonce

// Fill key and nonce
SecureRandom.GetBytes(key);
SecureRandom.GetBytes(nonce);

// Encrypt
byte[] ciphertext = SecretBox.Seal(message, nonce, key);

// Decrypt
byte[] decrypted = SecretBox.Open(ciphertext, nonce, key);

string result = Encoding.UTF8.GetString(decrypted);
Console.WriteLine($"Decrypted: {result}");
```

## Practical Examples

### Create and Sign Transaction

```csharp
using Ton.Crypto.Mnemonic;
using Ton.Crypto.Ed25519;
using Ton.Crypto.Primitives;
using Ton.Core.Boc;

// Get keys from mnemonic
string[] mnemonic = GetMnemonicFromUser();
KeyPair keys = Mnemonic.ToWalletKey(mnemonic);

// Create transaction data
Builder builder = Builder.BeginCell();
builder.StoreUint(0, 32);  // seqno
// ... more transaction data

Cell cell = builder.EndCell();
byte[] cellHash = cell.Hash(0);

// Sign transaction hash
byte[] signature = Ed25519.Sign(cellHash, keys.SecretKey);

// Build signed message
Builder signedBuilder = Builder.BeginCell();
signedBuilder.StoreBuffer(signature);
signedBuilder.StoreSlice(cell.BeginParse());
Cell signedCell = signedBuilder.EndCell();
```

### Verify Contract Signature

```csharp
// Extract signature and data from cell
Slice slice = cell.BeginParse();
byte[] signature = slice.LoadBuffer(64);
Cell dataCell = slice.LoadRef();

// Get message hash
byte[] messageHash = dataCell.Hash(0);

// Verify
bool isValid = Ed25519.Verify(signature, messageHash, publicKey);
```

### Password-Protected Wallet

```csharp
// Create wallet with password
string password = GetPasswordFromUser();
string[] mnemonic = Mnemonic.New(24, password: password);

// Later: recover wallet
string[] recoveredMnemonic = GetMnemonicFromUser();
string recoveredPassword = GetPasswordFromUser();

if (!Mnemonic.Validate(recoveredMnemonic, password: recoveredPassword))
{
    Console.WriteLine("Invalid mnemonic or password");
    return;
}

KeyPair keys = Mnemonic.ToWalletKey(recoveredMnemonic, password: recoveredPassword);
```

### Key Derivation Path (Custom)

```csharp
// Get HD seed
byte[] hdSeed = Mnemonic.ToHdSeed(mnemonic);

// Derive child key using PBKDF2
byte[] childSeed = Pbkdf2Sha512.DeriveKey(
    hdSeed,
    Encoding.UTF8.GetBytes("m/44'/607'/0'/0/0"),  // BIP44 path
    1,
    32
);

KeyPair childKeys = Ed25519.KeyPairFromSeed(childSeed);
```

## Security Best Practices

### 1. Never Expose Private Keys

```csharp
// ✅ Good: Keep keys in memory only
KeyPair keys = Mnemonic.ToWalletKey(mnemonic);
// Use keys
Array.Clear(keys.SecretKey, 0, keys.SecretKey.Length);  // Clear after use

// ❌ Never: Log or save private keys
Console.WriteLine($"Private key: {Convert.ToBase64String(keys.SecretKey)}");  // DON'T!
File.WriteAllText("key.txt", Convert.ToBase64String(keys.SecretKey));  // DON'T!
```

### 2. Validate User Input

```csharp
// ✅ Good: Always validate mnemonics
string[] userMnemonic = GetMnemonicFromUser();
if (!Mnemonic.Validate(userMnemonic))
{
    throw new ArgumentException("Invalid mnemonic");
}
```

### 3. Use Secure Storage

```csharp
// ✅ Good: Use system keychain/credential manager
// Example with .NET Data Protection API
using System.Security.Cryptography;
using Microsoft.AspNetCore.DataProtection;

IDataProtector protector = dataProtectionProvider.CreateProtector("MnemonicStorage");
string encryptedMnemonic = protector.Protect(string.Join(" ", mnemonic));
// Store encryptedMnemonic

// Later: decrypt
string decryptedMnemonic = protector.Unprotect(encryptedMnemonic);
```

### 4. Clear Sensitive Data

```csharp
// ✅ Good: Clear sensitive data from memory
byte[] secretKey = keys.SecretKey;
try
{
    // Use secret key
    byte[] signature = Ed25519.Sign(message, secretKey);
}
finally
{
    // Clear from memory
    Array.Clear(secretKey, 0, secretKey.Length);
}
```

### 5. Use Strong Passwords

```csharp
// ✅ Good: Enforce password requirements
string password = GetPasswordFromUser();

if (password.Length < 12)
    throw new ArgumentException("Password must be at least 12 characters");

if (!HasUpperCase(password) || !HasNumber(password))
    throw new ArgumentException("Password must contain uppercase and numbers");

string[] mnemonic = Mnemonic.New(24, password: password);
```

### 6. Verify Signatures

```csharp
// ✅ Good: Always verify signatures before trusting data
if (!Ed25519.Verify(signature, message, publicKey))
{
    throw new SecurityException("Invalid signature");
}
// Only use message after verification
```

## Common Patterns

### Mnemonic Import/Export

```csharp
// Export (display to user)
string mnemonicString = string.Join(" ", mnemonic);
Console.WriteLine($"Write down your recovery phrase:");
Console.WriteLine(mnemonicString);

// Import (from user input)
string userInput = Console.ReadLine();
string[] importedMnemonic = userInput!.Split(' ', StringSplitOptions.RemoveEmptyEntries);

if (!Mnemonic.Validate(importedMnemonic))
{
    Console.WriteLine("Invalid mnemonic");
    return;
}
```

### Key Rotation

```csharp
// Generate new keys
string[] newMnemonic = Mnemonic.New(24);
KeyPair newKeys = Mnemonic.ToWalletKey(newMnemonic);

// Create new wallet contract
WalletV4R2 newWallet = new(newKeys.PublicKey);

// Transfer funds from old wallet to new wallet
// ... (see Contracts module)
```

## See Also

- [Getting Started](../../getting-started/installation.md)
- [Key Concepts](../../getting-started/key-concepts.md)
- [Contracts Module](../contracts/overview.md)
