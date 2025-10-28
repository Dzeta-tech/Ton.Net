# Tact Code Generator

Automatically generates C# contract wrappers from Tact compiler output.

## Usage

```bash
cd tools/TactCodegen
dotnet run -- /path/to/Contract_Name.pkg
```

Or if you only have the ABI:

```bash
dotnet run -- /path/to/Contract_Name.abi
```

This will generate a `Contract_Name.cs` file in the same directory. **Use .pkg files when possible** as they include the contract code BOC automatically.

## What it generates

For each Tact contract, it generates:

1. **Message/Struct Types** - C# records with:
   - Properties for all fields
   - `Store(Builder)` method for serialization
   - Static `Load(Slice)` method for deserialization
   - OpCode constants for messages

2. **Contract Class** - Implementing `IContract` with:
   - `Create()` static factory method (from data fields)
   - `Send{MessageType}Async()` methods for each receiver
   - `Get{Name}Async()` methods for each getter
   - `Address`, `Init`, and `ABI` properties

## Example

Given a Tact contract:

```tact
message ProxyMessage {
    to: Address;
    value: Int;
    body: Cell?;
}

contract Proxy(owner: Address, invoiceId: Int as uint256) {
    receive(message: ProxyMessage) {
        // ...
    }
}
```

It generates:

```csharp
public record ProxyMessage(
    Address To,
    BigInteger Value,
    Cell? Body
)
{
    public const uint OpCode = 0x00B59D21;
    
    public void Store(Builder builder) { /* ... */ }
    public static ProxyMessage Load(Slice slice) { /* ... */ }
}

public class Proxy : IContract
{
    public static readonly Cell Code = /* ... */;
    
    public static Proxy Create(Address owner, BigInteger invoiceId) { /* ... */ }
    public async Task SendProxyMessageAsync(IContractProvider provider, ISender sender, ProxyMessage message, BigInteger value, bool bounce = true) { /* ... */ }
}
```

## Workflow

1. Compile your Tact contract: `npx @tact-lang/compiler`
2. Generate C# wrapper: `dotnet run -- build/Contract/Contract_Contract.pkg`
3. Add the generated `.cs` file to your project
4. Use it with Ton.NET SDK!

```csharp
using Generated.Contracts;
using Ton.HttpClient;

// Create contract instance
var proxy = Proxy.Create(ownerAddress, invoiceId);

// Open contract with provider
var client = new TonClient(new TonClientParameters { Endpoint = "..." });
var opened = client.Open(proxy);

// Send message
var message = new ProxyMessage(
    To: destinationAddress,
    Value: amount,
    Body: null,
    Bounce: true,
    Code: null,
    Data: null
);

await opened.Contract.SendProxyMessageAsync(
    opened.Provider,
    sender,
    message,
    value: 1_000_000_000 // 1 TON
);
```

## Features

✅ Extracts contract code BOC from `.pkg` file  
✅ Generates type-safe message/struct records  
✅ Full integration with Ton.NET SDK types  
✅ Automatic serialization/deserialization  
✅ Contract deployment helpers  

## TODOs

- [ ] Generate proper ABI metadata
- [ ] Support tuple serialization for get method arguments
- [ ] Handle complex types (maps, arrays)
- [ ] Add XML documentation comments from TL-B schemas

