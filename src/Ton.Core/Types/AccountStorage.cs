using System.Numerics;
using Ton.Core.Boc;

namespace Ton.Core.Types;

/// <summary>
///     Account storage data.
///     Source:
///     https://github.com/ton-blockchain/ton/blob/24dc184a2ea67f9c47042b4104bbb4d82289fac1/crypto/block/block.tlb#L235
///     account_storage$_ last_trans_lt:uint64 balance:CurrencyCollection state:AccountState
///     = AccountStorage;
/// </summary>
public record AccountStorage(
    BigInteger LastTransLt,
    CurrencyCollection Balance,
    AccountState State)
{
    /// <summary>
    ///     Loads AccountStorage from a Slice.
    /// </summary>
    public static AccountStorage Load(Slice slice)
    {
        return new AccountStorage(
            slice.LoadUintBig(64),
            CurrencyCollection.Load(slice),
            AccountState.Load(slice)
        );
    }

    /// <summary>
    ///     Stores AccountStorage into a Builder.
    /// </summary>
    public void Store(Builder builder)
    {
        builder.StoreUint(LastTransLt, 64);
        Balance.Store(builder);
        State.Store(builder);
    }
}