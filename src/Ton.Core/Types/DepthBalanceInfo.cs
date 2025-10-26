using Ton.Core.Boc;

namespace Ton.Core.Types;

/// <summary>
///     Depth balance information.
///     Source:
///     https://github.com/ton-blockchain/ton/blob/24dc184a2ea67f9c47042b4104bbb4d82289fac1/crypto/block/block.tlb#L259
///     depth_balance$_ split_depth:(#&lt;= 30) balance:CurrencyCollection = DepthBalanceInfo;
/// </summary>
public record DepthBalanceInfo(int SplitDepth, CurrencyCollection Balance)
{
    /// <summary>
    ///     Loads DepthBalanceInfo from a Slice.
    /// </summary>
    public static DepthBalanceInfo Load(Slice slice)
    {
        return new DepthBalanceInfo(
            (int)slice.LoadUint(5),
            CurrencyCollection.Load(slice)
        );
    }

    /// <summary>
    ///     Stores DepthBalanceInfo into a Builder.
    /// </summary>
    public void Store(Builder builder)
    {
        builder.StoreUint(SplitDepth, 5);
        Balance.Store(builder);
    }
}