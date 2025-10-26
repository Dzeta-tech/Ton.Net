namespace Ton.Core.Types;

/// <summary>
///     Reserve mode for action_reserve_currency.
///     Source:
///     https://github.com/ton-blockchain/ton/blob/24dc184a2ea67f9c47042b4104bbb4d82289fac1/crypto/block/block.tlb#L192
/// </summary>
public enum ReserveMode : byte
{
    /// <summary>
    ///     Reserve exactly this amount.
    /// </summary>
    ThisAmount = 0,

    /// <summary>
    ///     Leave this amount, reserve the rest.
    /// </summary>
    LeaveThisAmount = 1,

    /// <summary>
    ///     Reserve at most this amount.
    /// </summary>
    AtMostThisAmount = 2,

    /// <summary>
    ///     Leave at most this amount.
    /// </summary>
    LeaveMaxThisAmount = 3,

    /// <summary>
    ///     Reserve before_balance + this amount.
    /// </summary>
    BeforeBalancePlusThisAmount = 4,

    /// <summary>
    ///     Leave before_balance + this amount.
    /// </summary>
    LeaveBeforeBalancePlusThisAmount = 5,

    /// <summary>
    ///     Reserve before_balance - this amount.
    /// </summary>
    BeforeBalanceMinusThisAmount = 12,

    /// <summary>
    ///     Leave before_balance - this amount.
    /// </summary>
    LeaveBeforeBalanceMinusThisAmount = 13
}