using System.Numerics;
using Ton.Adnl.Protocol;
using Ton.Core.Addresses;
using Ton.Core.Boc;
using Ton.Core.Dict;
using Ton.Core.Types;

namespace Ton.LiteClient.Models;

/// <summary>
///     Represents the state of an account in TON blockchain
/// </summary>
public sealed class AccountState
{
    /// <summary>
    ///     Account address
    /// </summary>
    public required Address Address { get; init; }

    /// <summary>
    ///     Account balance in nanotons
    /// </summary>
    public required BigInteger Balance { get; init; }

    /// <summary>
    ///     Last transaction information (if any)
    /// </summary>
    public TransactionId? LastTransaction { get; init; }

    /// <summary>
    ///     Account storage state (Active, Frozen, Uninitialized, NonExist)
    /// </summary>
    public required AccountStorageState State { get; init; }

    /// <summary>
    ///     Account data (contract storage) if available
    /// </summary>
    public Cell? Data { get; init; }

    /// <summary>
    ///     Account code (smart contract code) if available
    /// </summary>
    public Cell? Code { get; init; }

    /// <summary>
    ///     Block in which this state was observed
    /// </summary>
    public required BlockId Block { get; init; }

    /// <summary>
    ///     Shard block in which this account resides
    /// </summary>
    public required BlockId ShardBlock { get; init; }

    /// <summary>
    ///     Returns true if the account exists and is initialized
    /// </summary>
    public bool IsActive => State == AccountStorageState.Active;

    /// <summary>
    ///     Returns true if the account has contract code
    /// </summary>
    public bool IsContract => Code != null;

    /// <summary>
    ///     Balance in TON (formatted as decimal)
    /// </summary>
    public decimal BalanceInTon => (decimal)Balance / 1_000_000_000m;

    /// <summary>
    ///     Creates AccountState from ADNL protocol's LiteServerAccountState
    /// </summary>
    public static AccountState FromAdnl(LiteServerAccountState adnlState, Address address)
    {
        // Parse account from state BOC
        if (adnlState.State.Length == 0)
            // Account doesn't exist
            return new AccountState
            {
                Address = address,
                Balance = BigInteger.Zero,
                State = AccountStorageState.NonExist,
                Block = BlockId.FromAdnl(adnlState.Id),
                ShardBlock = BlockId.FromAdnl(adnlState.Shardblk)
            };

        Cell stateCell = Cell.FromBoc(adnlState.State)[0];
        Slice stateSlice = stateCell.BeginParse();

        // Check if account exists (first bit = 1 means exists)
        if (!stateSlice.LoadBit())
            // Account doesn't exist
            return new AccountState
            {
                Address = address,
                Balance = BigInteger.Zero,
                State = AccountStorageState.NonExist,
                Block = BlockId.FromAdnl(adnlState.Id),
                ShardBlock = BlockId.FromAdnl(adnlState.Shardblk)
            };

        // Load Account structure
        Account account = Account.Load(stateSlice);

        // Determine account storage state
        AccountStorageState storageState = account.Storage.State switch
        {
            Core.Types.AccountState.Active _ => AccountStorageState.Active,
            Core.Types.AccountState.Frozen _ => AccountStorageState.Frozen,
            Core.Types.AccountState.Uninit _ => AccountStorageState.Uninitialized,
            _ => AccountStorageState.NonExist
        };

        // Extract code and data cells
        Cell? code = null;
        Cell? data = null;

        if (account.Storage.State is Core.Types.AccountState.Active activeState)
        {
            code = activeState.State?.Code;
            data = activeState.State?.Data;
        }

        // Get last transaction info from proof (if available)
        TransactionId? lastTx = null;
        if (adnlState.Proof.Length > 0)
            try
            {
                Cell[] proofCells = Cell.FromBoc(adnlState.Proof);
                if (proofCells.Length > 1)
                {
                    // Parse shard state from proof to get last transaction
                    Cell? shardStateCell = proofCells[1].Refs.Length > 0 ? proofCells[1].Refs[0] : null;
                    if (shardStateCell != null)
                    {
                        ShardStateUnsplit shardState = ShardStateUnsplit.Load(shardStateCell.BeginParse());
                        if (shardState.Accounts != null)
                        {
                            BigInteger accountHash = new(address.Hash, true, true);
                            ShardAccountRef? shardAccountRef = shardState.Accounts.Get(new DictKeyBigInt(accountHash));
                            if (shardAccountRef != null)
                            {
                                ShardAccount shardAccount = shardAccountRef.ShardAccount;
                                lastTx = new TransactionId(
                                    shardAccount.LastTransactionLt,
                                    shardAccount.LastTransactionHash.ToByteArray(true, true));
                            }
                        }
                    }
                }
            }
            catch
            {
                // If proof parsing fails, continue without last tx info
            }

        return new AccountState
        {
            Address = address,
            Balance = account.Storage.Balance.Coins,
            LastTransaction = lastTx,
            State = storageState,
            Code = code,
            Data = data,
            Block = BlockId.FromAdnl(adnlState.Id),
            ShardBlock = BlockId.FromAdnl(adnlState.Shardblk)
        };
    }

    public override string ToString()
    {
        return $"Account({Address}, balance:{BalanceInTon:F4} TON, state:{State})";
    }
}

/// <summary>
///     Represents account storage state
/// </summary>
public enum AccountStorageState
{
    /// <summary>
    ///     Account is active with initialized contract
    /// </summary>
    Active,

    /// <summary>
    ///     Account is frozen (suspended)
    /// </summary>
    Frozen,

    /// <summary>
    ///     Account exists but is not initialized
    /// </summary>
    Uninitialized,

    /// <summary>
    ///     Account does not exist
    /// </summary>
    NonExist
}