using Ton.Core.Boc;
using TonDict = Ton.Core.Dict;

namespace Ton.Core.Types;

/// <summary>
///     Shard account with augmented balance info.
///     Source:
///     https://github.com/ton-blockchain/ton/blob/24dc184a2ea67f9c47042b4104bbb4d82289fac1/crypto/block/block.tlb#L261
///     _ (HashmapAugE 256 ShardAccount DepthBalanceInfo) = ShardAccounts;
/// </summary>
public record ShardAccountRef(
    ShardAccount ShardAccount,
    DepthBalanceInfo DepthBalanceInfo
);

/// <summary>
///     Dictionary value for ShardAccountRef (augmented with balance info).
/// </summary>
internal class ShardAccountRefValue : TonDict.IDictionaryValue<ShardAccountRef>
{
    public ShardAccountRef Parse(Slice slice)
    {
        DepthBalanceInfo depthBalanceInfo = DepthBalanceInfo.Load(slice);
        ShardAccount shardAccount = ShardAccount.Load(slice);

        return new ShardAccountRef(shardAccount, depthBalanceInfo);
    }

    public void Serialize(ShardAccountRef value, Builder builder)
    {
        value.DepthBalanceInfo.Store(builder);
        value.ShardAccount.Store(builder);
    }
}

/// <summary>
///     Extension methods for ShardAccounts dictionary.
/// </summary>
public static class ShardAccountsExtensions
{
    /// <summary>
    ///     Loads ShardAccounts dictionary from a Slice.
    /// </summary>
    public static TonDict.Dictionary<TonDict.DictKeyBigInt, ShardAccountRef> LoadShardAccounts(this Slice slice)
    {
        return slice.LoadDict(
            TonDict.DictionaryKeys.BigUint(256),
            new ShardAccountRefValue()
        ) ?? TonDict.Dictionary<TonDict.DictKeyBigInt, ShardAccountRef>.Empty(
            TonDict.DictionaryKeys.BigUint(256),
            new ShardAccountRefValue()
        );
    }

    /// <summary>
    ///     Stores ShardAccounts dictionary into a Builder.
    /// </summary>
    public static Builder StoreShardAccounts(
        this Builder builder, TonDict.Dictionary<TonDict.DictKeyBigInt, ShardAccountRef> accounts)
    {
        builder.StoreDict(accounts);
        return builder;
    }
}