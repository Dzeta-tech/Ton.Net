using Ton.Core.Addresses;
using Ton.Core.Boc;

namespace Ton.Core.Types;

/// <summary>
///     Full account structure.
///     Source:
///     https://github.com/ton-blockchain/ton/blob/24dc184a2ea67f9c47042b4104bbb4d82289fac1/crypto/block/block.tlb#L231
///     account_none$0 = Account;
///     account$1 addr:MsgAddressInt storage_stat:StorageInfo storage:AccountStorage = Account;
/// </summary>
public record Account(
    Address Addr,
    StorageInfo StorageStats,
    AccountStorage Storage)
{
    /// <summary>
    ///     Loads Account from a Slice.
    /// </summary>
    public static Account Load(Slice slice)
    {
        return new Account(
            slice.LoadAddress()!,
            StorageInfo.Load(slice),
            AccountStorage.Load(slice)
        );
    }

    /// <summary>
    ///     Stores Account into a Builder.
    /// </summary>
    public void Store(Builder builder)
    {
        builder.StoreAddress(Addr);
        StorageStats.Store(builder);
        Storage.Store(builder);
    }
}