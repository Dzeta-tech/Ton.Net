using System.Numerics;
using Ton.Core.Addresses;
using Ton.Core.Boc;
using Ton.Core.Types;
using TonDict = Ton.Core.Dict;

namespace Ton.Core.Contracts;

/// <summary>
///     Arguments for sending a message.
/// </summary>
public record SenderArguments
{
    /// <summary>
    ///     Amount of nanotons to send.
    /// </summary>
    public required BigInteger Value { get; init; }

    /// <summary>
    ///     Destination address.
    /// </summary>
    public required Address To { get; init; }

    /// <summary>
    ///     Extra currencies to send (optional).
    /// </summary>
    public TonDict.Dictionary<TonDict.DictKeyUint, BigInteger>? ExtraCurrency { get; init; }

    /// <summary>
    ///     Send mode flags (optional, defaults to PAY_GAS_SEPARATELY).
    /// </summary>
    public SendMode? SendMode { get; init; }

    /// <summary>
    ///     Whether the message should bounce on failure (optional, defaults to true).
    /// </summary>
    public bool? Bounce { get; init; }

    /// <summary>
    ///     State initialization for deploying contracts (optional).
    /// </summary>
    public StateInit? Init { get; init; }

    /// <summary>
    ///     Message body (optional).
    /// </summary>
    public Cell? Body { get; init; }
}

/// <summary>
///     Interface for sending messages to the blockchain.
///     Implemented by wallets and other message senders.
/// </summary>
public interface ISender
{
    /// <summary>
    ///     Sender's address (optional).
    ///     May be null for external message senders.
    /// </summary>
    Address? Address { get; }

    /// <summary>
    ///     Send a message to the blockchain.
    /// </summary>
    Task SendAsync(SenderArguments args);
}