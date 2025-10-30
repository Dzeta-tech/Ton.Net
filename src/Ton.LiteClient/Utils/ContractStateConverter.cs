using Ton.Core.Contracts;
using Ton.LiteClient.Models;

namespace Ton.LiteClient.Utils;

/// <summary>
///     Converts LiteClient-specific types to core contract types
/// </summary>
internal static class ContractStateConverter
{
    /// <summary>
    ///     Converts AccountState from LiteClient to ContractState
    /// </summary>
    public static ContractState ToContractState(AccountState accountState)
    {
        ContractState.LastTransaction? last = null;
        if (accountState.LastTransaction != null)
            last = new ContractState.LastTransaction(
                accountState.LastTransaction.Lt,
                accountState.LastTransaction.Hash
            );

        ContractState.AccountStateInfo state = accountState.State switch
        {
            AccountStorageState.Active => new ContractState.AccountStateInfo.Active(
                accountState.Code?.ToBoc(),
                accountState.Data?.ToBoc()
            ),
            AccountStorageState.Frozen => new ContractState.AccountStateInfo.Frozen([]),
            AccountStorageState.Uninitialized => new ContractState.AccountStateInfo.Uninit(),
            _ => new ContractState.AccountStateInfo.Uninit()
        };

        return new ContractState
        {
            Balance = accountState.Balance,
            ExtraCurrency = null, // AccountState doesn't expose extra currencies
            Last = last,
            State = state
        };
    }
}