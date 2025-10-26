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
    None = 0,

    /// <summary>
    ///     Pay transfer fees separately from the message value.
    /// </summary>
    PayFeesSeparately = 1,

    /// <summary>
    ///     Ignore any errors arising while processing this message during the action phase.
    /// </summary>
    IgnoreErrors = 2,

    /// <summary>
    ///     Current account must be destroyed if its resulting balance is zero.
    /// </summary>
    DestroyIfZero = 32,

    /// <summary>
    ///     Carry all the remaining value of the inbound message in addition to the value initially indicated.
    /// </summary>
    CarryAllRemainingBalance = 64,

    /// <summary>
    ///     Carry all the remaining balance of the current smart contract instead of the value originally indicated.
    /// </summary>
    CarryAllRemainingIncomingValue = 128
}