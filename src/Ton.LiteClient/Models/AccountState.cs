using System.Numerics;
using Ton.Core.Addresses;
using Ton.Core.Boc;

namespace Ton.LiteClient.Models;

/// <summary>
/// Represents the state of an account in TON blockchain
/// </summary>
public sealed class AccountState
{
    /// <summary>
    /// Account address
    /// </summary>
    public required Address Address { get; init; }

    /// <summary>
    /// Account balance in nanotons
    /// </summary>
    public required BigInteger Balance { get; init; }

    /// <summary>
    /// Last transaction information (if any)
    /// </summary>
    public TransactionId? LastTransaction { get; init; }

    /// <summary>
    /// Account storage state (Active, Frozen, Uninitialized, NonExist)
    /// </summary>
    public required AccountStorageState State { get; init; }

    /// <summary>
    /// Account data (contract storage) if available
    /// </summary>
    public Cell? Data { get; init; }

    /// <summary>
    /// Account code (smart contract code) if available
    /// </summary>
    public Cell? Code { get; init; }

    /// <summary>
    /// Block in which this state was observed
    /// </summary>
    public required BlockId Block { get; init; }

    /// <summary>
    /// Shard block in which this account resides
    /// </summary>
    public required BlockId ShardBlock { get; init; }

    /// <summary>
    /// Returns true if the account exists and is initialized
    /// </summary>
    public bool IsActive => State == AccountStorageState.Active;

    /// <summary>
    /// Returns true if the account has contract code
    /// </summary>
    public bool IsContract => Code != null;

    /// <summary>
    /// Balance in TON (formatted as decimal)
    /// </summary>
    public decimal BalanceInTon => (decimal)Balance / 1_000_000_000m;

    public override string ToString() =>
        $"Account({Address}, balance:{BalanceInTon:F4} TON, state:{State})";
}

/// <summary>
/// Represents account storage state
/// </summary>
public enum AccountStorageState
{
    /// <summary>
    /// Account is active with initialized contract
    /// </summary>
    Active,

    /// <summary>
    /// Account is frozen (suspended)
    /// </summary>
    Frozen,

    /// <summary>
    /// Account exists but is not initialized
    /// </summary>
    Uninitialized,

    /// <summary>
    /// Account does not exist
    /// </summary>
    NonExist
}

