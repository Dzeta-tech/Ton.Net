# Contract Implementation Plan

> **Goal:** Implement contract system from @ton/core to enable contract interactions

## Architecture Overview

The contract system has a clean separation of concerns:

```
┌─────────────────────────────────────────────────────────────┐
│                    CONTRACT SYSTEM                          │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  ┌──────────────┐         ┌──────────────────┐            │
│  │   Contract   │         │ ContractProvider │            │
│  │  (Interface) │────────▶│   (Interface)    │            │
│  └──────────────┘         └──────────────────┘            │
│   - address               - getState()                     │
│   - init (optional)       - get(method)                    │
│   - abi (optional)        - external(msg)                  │
│                           - internal(via, args)            │
│                           - getTransactions()              │
│                                                             │
│  ┌──────────────┐         ┌──────────────────┐            │
│  │    Sender    │         │  ContractState   │            │
│  │  (Interface) │         │    (Record)      │            │
│  └──────────────┘         └──────────────────┘            │
│   - address?              - balance                        │
│   - send(args)            - extracurrency                  │
│                           - last (lt, hash)                │
│                           - state (uninit/active/frozen)   │
│                                                             │
│  ┌──────────────────────────────────────────┐             │
│  │      OpenContract<T> Helper              │             │
│  │  (C#: Extension methods or proxy)        │             │
│  └──────────────────────────────────────────┘             │
│   - Auto-injects ContractProvider into methods            │
│   - Pattern: methods starting with Get/Send/Is            │
│                                                             │
│  ┌──────────────┐                                          │
│  │ ComputeError │                                          │
│  │   (Class)    │                                          │
│  └──────────────┘                                          │
│   - exitCode                                               │
│   - debugLogs                                              │
│   - logs                                                   │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

## Phase 1: Core Interfaces (Day 1)

### 1.1 Contract Interface
**File:** `src/Ton.Core/Contract/Contract.cs`

```csharp
namespace Ton.Core.Contract;

/// <summary>
/// Base contract interface
/// </summary>
public interface IContract
{
    /// <summary>
    /// Contract address
    /// </summary>
    Address Address { get; }
    
    /// <summary>
    /// Optional state initialization
    /// </summary>
    StateInit? Init { get; }
    
    /// <summary>
    /// Optional contract ABI
    /// </summary>
    ContractABI? ABI { get; }
}
```

**Why:**
- Simple interface that all contracts implement
- Provides address and optional init/ABI
- No dependencies on external systems

### 1.2 ContractState Record
**File:** `src/Ton.Core/Contract/ContractState.cs`

```csharp
namespace Ton.Core.Contract;

/// <summary>
/// Contract state representation
/// </summary>
public record ContractState
{
    /// <summary>
    /// Contract balance in nanotons
    /// </summary>
    public BigInteger Balance { get; init; }
    
    /// <summary>
    /// Extra currencies (if any)
    /// </summary>
    public ExtraCurrency? ExtraCurrency { get; init; }
    
    /// <summary>
    /// Last transaction info
    /// </summary>
    public LastTransaction? Last { get; init; }
    
    /// <summary>
    /// Account state
    /// </summary>
    public AccountStateInfo State { get; init; }
    
    public record LastTransaction(BigInteger Lt, byte[] Hash);
    
    public abstract record AccountStateInfo
    {
        public record Uninit : AccountStateInfo;
        public record Active(byte[]? Code, byte[]? Data) : AccountStateInfo;
        public record Frozen(byte[] StateHash) : AccountStateInfo;
    }
}
```

**Why:**
- Represents current state of a contract on-chain
- Matches JS SDK structure exactly
- Uses C# record types for immutability

### 1.3 Sender Interface
**File:** `src/Ton.Core/Contract/Sender.cs`

```csharp
namespace Ton.Core.Contract;

/// <summary>
/// Arguments for sending messages
/// </summary>
public record SenderArguments
{
    public required BigInteger Value { get; init; }
    public required Address To { get; init; }
    public ExtraCurrency? ExtraCurrency { get; init; }
    public SendMode? SendMode { get; init; }
    public bool? Bounce { get; init; }
    public StateInit? Init { get; init; }
    public Cell? Body { get; init; }
}

/// <summary>
/// Interface for sending messages
/// </summary>
public interface ISender
{
    /// <summary>
    /// Sender address (optional)
    /// </summary>
    Address? Address { get; }
    
    /// <summary>
    /// Send a message
    /// </summary>
    Task SendAsync(SenderArguments args);
}
```

**Why:**
- Abstracts message sending
- Used by wallets to send messages
- Clean separation from provider logic

## Phase 2: ContractProvider (Day 2)

### 2.1 ContractProvider Interface
**File:** `src/Ton.Core/Contract/ContractProvider.cs`

```csharp
namespace Ton.Core.Contract;

/// <summary>
/// Result from get method call
/// </summary>
public record ContractGetMethodResult
{
    public required TupleReader Stack { get; init; }
    public BigInteger? GasUsed { get; init; }
    public string? Logs { get; init; }
}

/// <summary>
/// Arguments for internal messages
/// </summary>
public record InternalMessageArgs
{
    public required BigInteger Value { get; init; }
    public ExtraCurrency? ExtraCurrency { get; init; }
    public bool? Bounce { get; init; }
    public SendMode? SendMode { get; init; }
    public Cell? Body { get; init; }
}

/// <summary>
/// Provider for contract interactions
/// </summary>
public interface IContractProvider
{
    /// <summary>
    /// Get current contract state
    /// </summary>
    Task<ContractState> GetStateAsync();
    
    /// <summary>
    /// Call a get method
    /// </summary>
    Task<ContractGetMethodResult> GetAsync(string name, TupleItem[] args);
    
    /// <summary>
    /// Call a get method by ID
    /// </summary>
    Task<ContractGetMethodResult> GetAsync(int methodId, TupleItem[] args);
    
    /// <summary>
    /// Send external message
    /// </summary>
    Task ExternalAsync(Cell message);
    
    /// <summary>
    /// Send internal message via sender
    /// </summary>
    Task InternalAsync(ISender via, InternalMessageArgs args);
    
    /// <summary>
    /// Open another contract using this provider
    /// </summary>
    OpenedContract<T> Open<T>(T contract) where T : IContract;
    
    /// <summary>
    /// Get transactions for an address
    /// </summary>
    Task<Transaction[]> GetTransactionsAsync(Address address, BigInteger lt, byte[] hash, int? limit = null);
}
```

**Why:**
- Core abstraction for interacting with contracts
- Will be implemented by TonClient later
- Clean async/await pattern for C#

### 2.2 OpenedContract Helper
**File:** `src/Ton.Core/Contract/OpenedContract.cs`

**C# Approach:** Since C# doesn't have TypeScript's dynamic proxy magic, we'll use:
1. **Option A (Simpler):** Extension methods pattern
2. **Option B (Advanced):** Dynamic proxy with DispatchProxy

**Recommended: Option A - Extension Methods**

```csharp
namespace Ton.Core.Contract;

/// <summary>
/// Wraps a contract with its provider for convenient method calls
/// </summary>
public class OpenedContract<T> where T : IContract
{
    public T Contract { get; }
    public IContractProvider Provider { get; }
    
    public OpenedContract(T contract, IContractProvider provider)
    {
        Contract = contract;
        Provider = provider;
    }
}

/// <summary>
/// Extension methods for convenient contract interaction
/// </summary>
public static class ContractExtensions
{
    /// <summary>
    /// Open a contract with a provider
    /// </summary>
    public static OpenedContract<T> Open<T>(this IContractProvider provider, T contract) 
        where T : IContract
    {
        return new OpenedContract<T>(contract, provider);
    }
}
```

**Usage:**
```csharp
// JS: let wallet = client.open(WalletV4.create(...))
// C#: var wallet = client.Open(WalletV4.Create(...))

// Then wallet contracts implement methods that take the provider:
public async Task<BigInteger> GetSeqno(OpenedContract<WalletV4> contract)
{
    var state = await contract.Provider.GetStateAsync();
    // ...
}
```

**Why:**
- Clean C# idiom
- Type-safe
- Easy to understand
- No reflection magic needed

## Phase 3: Supporting Types (Day 3)

### 3.1 ComputeError
**File:** `src/Ton.Core/Contract/ComputeError.cs`

```csharp
namespace Ton.Core.Contract;

/// <summary>
/// Error during contract compute phase
/// </summary>
public class ComputeError : Exception
{
    public int ExitCode { get; }
    public string? DebugLogs { get; }
    public string? Logs { get; }
    
    public ComputeError(string message, int exitCode, string? debugLogs = null, string? logs = null)
        : base(message)
    {
        ExitCode = exitCode;
        DebugLogs = debugLogs;
        Logs = logs;
    }
}
```

### 3.2 ContractABI
**File:** `src/Ton.Core/Contract/ContractABI.cs`

```csharp
namespace Ton.Core.Contract;

public record ABIError(string Message);

public abstract record ABITypeRef
{
    public record Simple(
        string Type, 
        bool? Optional = null, 
        object? Format = null
    ) : ABITypeRef;
    
    public record Dict(
        string Key,
        string Value,
        object? Format = null,
        object? KeyFormat = null,
        object? ValueFormat = null
    ) : ABITypeRef;
}

public record ABIField(string Name, ABITypeRef Type);
public record ABIType(string Name, int? Header, ABIField[] Fields);
public record ABIArgument(string Name, ABITypeRef Type);
public record ABIGetter(
    string Name, 
    int? MethodId = null, 
    ABIArgument[]? Arguments = null, 
    ABITypeRef? ReturnType = null
);

public abstract record ABIReceiverMessage
{
    public record Typed(string Type) : ABIReceiverMessage;
    public record Any : ABIReceiverMessage;
    public record Empty : ABIReceiverMessage;
    public record Text(string? Text = null) : ABIReceiverMessage;
}

public record ABIReceiver(
    string Receiver, // "internal" or "external"
    ABIReceiverMessage Message
);

public record ContractABI
{
    public string? Name { get; init; }
    public ABIType[]? Types { get; init; }
    public Dictionary<int, ABIError>? Errors { get; init; }
    public ABIGetter[]? Getters { get; init; }
    public ABIReceiver[]? Receivers { get; init; }
}
```

**Why:**
- Represents contract ABI (for tools/generators)
- Not critical for basic usage
- Can be extended later

## Phase 4: Testing Strategy

### 4.1 Mock Provider
**File:** `tests/Ton.Core.Tests/Contract/MockContractProvider.cs`

```csharp
public class MockContractProvider : IContractProvider
{
    private ContractState _state;
    private Dictionary<string, ContractGetMethodResult> _methods = new();
    
    public void SetState(ContractState state) => _state = state;
    public void SetMethodResult(string name, ContractGetMethodResult result) 
        => _methods[name] = result;
    
    public Task<ContractState> GetStateAsync() => Task.FromResult(_state);
    
    public Task<ContractGetMethodResult> GetAsync(string name, TupleItem[] args)
    {
        if (_methods.TryGetValue(name, out var result))
            return Task.FromResult(result);
        throw new NotImplementedException($"Method {name} not mocked");
    }
    
    // ... implement other methods
}
```

### 4.2 Unit Tests
**File:** `tests/Ton.Core.Tests/Contract/ContractTests.cs`

```csharp
[Test]
public async Task Test_ContractState_Parsing()
{
    var state = new ContractState
    {
        Balance = 1000000000,
        State = new ContractState.AccountStateInfo.Active(
            Code: new byte[] { 1, 2, 3 },
            Data: new byte[] { 4, 5, 6 }
        )
    };
    
    Assert.That(state.Balance, Is.EqualTo(1000000000));
    Assert.That(state.State, Is.InstanceOf<ContractState.AccountStateInfo.Active>());
}

[Test]
public async Task Test_OpenedContract_Integration()
{
    var mockProvider = new MockContractProvider();
    var contract = new TestContract(Address.Parse("..."));
    
    var opened = mockProvider.Open(contract);
    
    Assert.That(opened.Contract, Is.EqualTo(contract));
    Assert.That(opened.Provider, Is.EqualTo(mockProvider));
}
```

## Implementation Steps

### Step 1: Create Directory Structure
```bash
mkdir -p src/Ton.Core/Contract
mkdir -p tests/Ton.Core.Tests/Contract
```

### Step 2: Implement Core Types (in order)
1. ✅ `Contract.cs` - Interface (5 min)
2. ✅ `ContractState.cs` - Record (10 min)
3. ✅ `Sender.cs` - Interface + SenderArguments (10 min)
4. ✅ `ContractProvider.cs` - Interface (15 min)
5. ✅ `OpenedContract.cs` - Helper (15 min)
6. ✅ `ComputeError.cs` - Exception (5 min)
7. ✅ `ContractABI.cs` - Records (20 min)

**Total: ~1.5 hours**

### Step 3: Add Tests
1. Unit tests for each type
2. Integration test with mock provider
3. Test OpenedContract pattern

**Total: ~1 hour**

### Step 4: Update API Inventory
Mark contract module as complete in `API_INVENTORY.md`

## Next Steps After Contract Module

Once contracts are done, the natural progression is:

1. **HTTP Client** - Implement `IContractProvider` with real API calls
2. **Wallet Contracts** - Use contract interfaces to implement wallets
3. **Jetton Contracts** - Token support
4. **Advanced Features** - Multisig, elector, etc.

## Key Design Decisions

### C# vs JavaScript Differences

| Aspect | JavaScript | C# Implementation |
|--------|-----------|-------------------|
| **Proxy magic** | Uses Proxy for method injection | Use extension methods or DispatchProxy |
| **Maybe types** | `Maybe<T>` is just `T | null` | Use `T?` nullable reference types |
| **Async** | Promise-based | Task-based (async/await) |
| **Interfaces** | Structural typing | Explicit interfaces |
| **Records** | Object literals | Record types or classes |

### Why This Approach Works

1. **Clean separation** - Interfaces don't depend on implementation
2. **Testable** - Mock providers for unit tests
3. **Type-safe** - Full C# type system benefits
4. **Extensible** - Easy to add new contract types
5. **Idiomatic C#** - Follows C# patterns, not direct JS port

## Timeline

- **Day 1:** Core interfaces + ContractState ✅
- **Day 2:** ContractProvider + OpenedContract ✅
- **Day 3:** Supporting types + tests ✅
- **Total:** 3 days for complete contract foundation

After this, we can implement HTTP client and wallets on top of this solid foundation! 🚀

