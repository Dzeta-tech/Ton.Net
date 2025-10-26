using System.Numerics;
using Ton.Core.Boc;

namespace Ton.Core.Types;

/// <summary>
///     Shard account structure.
///     Source:
///     https://github.com/ton-blockchain/ton/blob/24dc184a2ea67f9c47042b4104bbb4d82289fac1/crypto/block/block.tlb#L256
///     account_descr$_ account:^Account last_trans_hash:bits256
///     last_trans_lt:uint64 = ShardAccount;
/// </summary>
public record ShardAccount(
    Account? Account,
    BigInteger LastTransactionHash,
    ulong LastTransactionLt
)
{
    /// <summary>
    ///     Loads ShardAccount from a Slice.
    /// </summary>
    public static ShardAccount Load(Slice slice)
    {
        Cell accountRef = slice.LoadRef();
        Account? account = null;

        if (!accountRef.IsExotic)
        {
            Slice accountSlice = accountRef.BeginParse();
            if (accountSlice.LoadBit()) account = Account.Load(accountSlice);
        }

        return new ShardAccount(
            account,
            slice.LoadUintBig(256),
            (ulong)slice.LoadUint(64)
        );
    }

    /// <summary>
    ///     Stores ShardAccount into a Builder.
    /// </summary>
    public void Store(Builder builder)
    {
        Builder accountBuilder = Builder.BeginCell();
        if (Account != null)
        {
            accountBuilder.StoreBit(true);
            Account.Store(accountBuilder);
        }
        else
        {
            accountBuilder.StoreBit(false);
        }

        builder.StoreRef(accountBuilder.EndCell());

        builder.StoreUint(LastTransactionHash, 256);
        builder.StoreUint(LastTransactionLt, 64);
    }
}