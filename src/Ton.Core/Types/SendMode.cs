namespace Ton.Core.Types;

/// <summary>
///     Send mode flags for outbound messages.
///     Source: https://docs.ton.org/develop/smart-contracts/messages#message-modes
/// </summary>
[Flags]
public enum SendMode : byte
{
    /// <summary>
    ///     Ordinary message (default).
    /// </summary>
    SendDefault = 0,

    /// <summary>
    ///     Pay forward fees separately from the message value.
    /// </summary>
    SendPayFwdFeesSeparately = 1,

    /// <summary>
    ///     Ignore any errors arising while processing this message during the action phase.
    /// </summary>
    SendIgnoreErrors = 2,

    /// <summary>
    ///     Bounce the transaction in case of any errors during the action phase.
    ///     Has no effect if SendIgnoreErrors flag is used.
    /// </summary>
    SendBounceIfActionFail = 16,

    /// <summary>
    ///     The current account (contract) will be destroyed if its resulting balance is zero.
    ///     This flag is often used with SendRemainingBalance mode.
    /// </summary>
    SendDestroyIfZero = 32,

    /// <summary>
    ///     Carries all the remaining value of the inbound message in addition to the value initially indicated in the new
    ///     message.
    /// </summary>
    SendRemainingValue = 64,

    /// <summary>
    ///     Carries all the remaining balance of the current smart contract instead of the value originally indicated in the
    ///     message.
    ///     Use with caution as it works with the balance of the entire contract.
    /// </summary>
    SendRemainingBalance = 128
}