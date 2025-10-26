# TL-B Types Implementation Plan

## Priority 1: Core Types (Essential for Wallets & DApps)

### 1.1 Foundation (Week 1, Days 1-2) ✅ COMPLETE
- [x] `SendMode.ts` - Enum (simple)
- [x] `StateInit.ts` - Contract initialization
- [x] `TickTock.ts` - Tick-tock flag (dependency)
- [x] `SimpleLibrary.ts` - Library structure (dependency)
- [x] `CommonMessageInfo.ts` - Message headers (3 variants)
- [x] `CurrencyCollection.ts` - TON + extra currencies
- [x] `ExternalAddress.ts` - External address type
- [x] `Message.ts` - Messages (internal/external)
- [x] `CommonMessageInfoRelaxed.ts` - Relaxed message headers
- [x] `MessageRelaxed.ts` - Relaxed messages

**Why first:** Required for ANY contract interaction (sending messages, deploying contracts).

### 1.2 Account Types (Week 1, Days 3-4) ✅ COMPLETE
- [x] `AccountStatus.ts` - Enum (simple)
- [x] `AccountStorage.ts` - Account storage data
- [x] `AccountState.ts` - Account state
- [x] `StorageInfo.ts` - Storage statistics
- [x] `StorageUsed.ts` - Storage usage
- [x] `StorageExtraInfo.ts` - Extra storage info
- [x] `Account.ts` - Full account structure
- [x] `DepthBalanceInfo.ts` - Balance info

**Why second:** Needed to read account data, check balances, get contract state.

### 1.3 Currency (Week 1, Day 5)
- [ ] `ExtraCurrency.ts` - Non-TON currencies (jettons)

**Why third:** Handle TON coins + jettons in transactions.

---

## Priority 2: Transaction Types (Essential for Block Explorers)

### 2.1 Core Transaction (Week 2, Days 1-2)
- [ ] `Transaction.ts` - Main transaction structure
- [ ] `TransactionDescription.ts` - Transaction details (7 variants)
- [ ] `HashUpdate.ts` - State hash updates
- [ ] `AccountStatusChange.ts` - Status transitions

**Why fourth:** Parse transaction history, analyze blockchain data.

### 2.2 Transaction Phases (Week 2, Day 3)
- [ ] `TransactionStoragePhase.ts` - Storage fees
- [ ] `TransactionCreditPhase.ts` - Credit phase
- [ ] `TransactionComputePhase.ts` - Compute/gas phase
- [ ] `TransactionActionPhase.ts` - Action phase
- [ ] `TransactionBouncePhase.ts` - Bounce handling
- [ ] `ComputeSkipReason.ts` - Why compute skipped (enum)

**Why fifth:** Detailed transaction analysis, gas calculation, error debugging.

---

## Priority 3: Advanced Types (For Validators & Advanced Features)

### 3.1 Block/Shard Structures (Week 2, Days 4-5)
- [ ] `ShardAccount.ts` - Account in shard
- [ ] `ShardAccounts.ts` - Multiple accounts
- [ ] `ShardIdent.ts` - Shard identifier
- [ ] `ShardStateUnsplit.ts` - Shard state
- [ ] `SplitMergeInfo.ts` - Shard split/merge
- [ ] `MasterchainStateExtra.ts` - Masterchain state

**Why later:** Only needed for validators, shard analysis, advanced indexing.

### 3.2 Libraries & References (Week 3, Day 1)
- [ ] `SimpleLibrary.ts` - Contract libraries
- [ ] `LibRef.ts` - Library references

**Why later:** Rare use case, mostly for complex contracts.

### 3.3 Special Contracts (Week 3, Day 2)
- [ ] `TickTock.ts` - Special contract tick/tock
- [ ] `ReserveMode.ts` - Reserve mode flags (enum)
- [ ] `StorageExtraInfo.ts` - Extra storage metadata

**Why later:** Only for special system contracts.

### 3.4 Output Messages (Week 3, Day 3)
- [ ] `OutList.ts` - Transaction output messages

**Why later:** Advanced transaction analysis.

---

## Implementation Order Summary

### Week 1: Core Wallet Functionality (Priority 1)
```
Day 1-2: StateInit, Message, CommonMessageInfo (6 types)
Day 3-4: Account types (7 types)
Day 5:   Currency types (2 types)
Total:   15 types → Can send messages, deploy contracts, check balances
```

### Week 2: Transactions & Analytics (Priority 2)
```
Day 1-2: Transaction core (4 types)
Day 3:   Transaction phases (6 types)
Day 4-5: Block/shard types (6 types)
Total:   16 types → Full transaction parsing, block exploration
```

### Week 3: Advanced Features (Priority 3)
```
Day 1:   Libraries (2 types)
Day 2:   Special contracts (3 types)
Day 3:   Output messages (1 type)
Total:   6 types → Complete implementation
```

---

## Total: 37 Types

**Breakdown:**
- **Simple** (enums/flags): 4 types (10%) - ~1 hour each
- **Medium** (structs): 20 types (54%) - ~2-3 hours each
- **Complex** (unions/refs): 13 types (35%) - ~4-6 hours each

**Estimated Time:**
- Priority 1 (Core): 5 days
- Priority 2 (Transactions): 5 days
- Priority 3 (Advanced): 3 days
- **Total: ~13 days** (2.5 weeks with testing)

---

## Dependencies Graph

```
StateInit
    └── TickTock
    └── SimpleLibrary

Message
    └── CommonMessageInfo
    └── StateInit

Account
    └── AccountStorage
    └── AccountStatus
    └── StorageInfo → StorageUsed

Transaction
    └── Message
    └── AccountStatus
    └── AccountStatusChange
    └── HashUpdate
    └── CurrencyCollection → ExtraCurrency
    └── TransactionDescription
        └── TransactionStoragePhase
        └── TransactionCreditPhase
        └── TransactionComputePhase → ComputeSkipReason
        └── TransactionActionPhase
        └── TransactionBouncePhase
        └── SplitMergeInfo

ShardStateUnsplit
    └── ShardAccounts → ShardAccount
    └── MasterchainStateExtra
```

---

## Next Steps

1. ✅ Start with **SendMode** (already in API inventory)
2. ✅ Implement **StateInit** (most fundamental)
3. ✅ Implement **CommonMessageInfo** + **Message**
4. ✅ Continue down the priority list...

Each type gets:
- C# class/record with properties
- `Load(Slice)` static method
- `Store()` static method returning `Action<Builder>`
- Full XML documentation
- NUnit tests with JS SDK test vectors

