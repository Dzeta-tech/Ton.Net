using Ton.Core.Boc;

namespace Ton.Core.Types;

/// <summary>
///     Account status in TON blockchain.
///     Source:
///     https://github.com/ton-blockchain/ton/blob/24dc184a2ea67f9c47042b4104bbb4d82289fac1/crypto/block/block.tlb#L243
///     acc_state_uninit$00 = AccountStatus;
///     acc_state_frozen$01 = AccountStatus;
///     acc_state_active$10 = AccountStatus;
///     acc_state_nonexist$11 = AccountStatus;
/// </summary>
public enum AccountStatus
{
    Uninitialized = 0x00,
    Frozen = 0x01,
    Active = 0x02,
    NonExisting = 0x03
}

/// <summary>
///     Extension methods for AccountStatus.
/// </summary>
public static class AccountStatusExtensions
{
    /// <summary>
    ///     Loads AccountStatus from a Slice.
    /// </summary>
    public static AccountStatus LoadAccountStatus(this Slice slice)
    {
        int status = (int)slice.LoadUint(2);
        return status switch
        {
            0x00 => AccountStatus.Uninitialized,
            0x01 => AccountStatus.Frozen,
            0x02 => AccountStatus.Active,
            0x03 => AccountStatus.NonExisting,
            _ => throw new InvalidOperationException($"Invalid account status: {status}")
        };
    }

    /// <summary>
    ///     Stores AccountStatus into a Builder.
    /// </summary>
    public static Builder StoreAccountStatus(this Builder builder, AccountStatus status)
    {
        return builder.StoreUint((int)status, 2);
    }
}