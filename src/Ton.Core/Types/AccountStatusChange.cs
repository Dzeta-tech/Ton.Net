using Ton.Core.Boc;

namespace Ton.Core.Types;

/// <summary>
///     Account status change enum.
///     Source:
///     https://github.com/ton-blockchain/ton/blob/24dc184a2ea67f9c47042b4104bbb4d82289fac1/crypto/block/block.tlb#L275
///     acst_unchanged$0 = AccStatusChange;  // x -> x
///     acst_frozen$10 = AccStatusChange;    // init -> frozen
///     acst_deleted$11 = AccStatusChange;   // frozen -> deleted
/// </summary>
public enum AccountStatusChange
{
    Unchanged,
    Frozen,
    Deleted
}

/// <summary>
///     Extension methods for AccountStatusChange.
/// </summary>
public static class AccountStatusChangeExtensions
{
    /// <summary>
    ///     Loads AccountStatusChange from a Slice.
    /// </summary>
    public static AccountStatusChange LoadAccountStatusChange(this Slice slice)
    {
        if (!slice.LoadBit())
            return AccountStatusChange.Unchanged;

        return slice.LoadBit()
            ? AccountStatusChange.Deleted
            : AccountStatusChange.Frozen;
    }

    /// <summary>
    ///     Stores AccountStatusChange into a Builder.
    /// </summary>
    public static Builder StoreAccountStatusChange(this Builder builder, AccountStatusChange change)
    {
        switch (change)
        {
            case AccountStatusChange.Unchanged:
                builder.StoreBit(false);
                break;
            case AccountStatusChange.Frozen:
                builder.StoreBit(true);
                builder.StoreBit(false);
                break;
            case AccountStatusChange.Deleted:
                builder.StoreBit(true);
                builder.StoreBit(true);
                break;
            default:
                throw new InvalidOperationException($"Invalid account status change: {change}");
        }

        return builder;
    }
}