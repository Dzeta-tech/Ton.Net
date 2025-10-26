using System.Numerics;
using Ton.Core.Addresses;
using Ton.Core.Boc;
using Ton.Core.Tuple;
using Ton.Core.Types;
using TonDict = Ton.Core.Dict;

namespace Ton.Core.Contracts;

/// <summary>
///     Result from a get method call.
/// </summary>
public record ContractGetMethodResult
{
    /// <summary>
    ///     Result stack as a TupleReader for easy access.
    /// </summary>
    public required TupleReader Stack { get; init; }

    /// <summary>
    ///     Amount of gas used (if available).
    /// </summary>
    public BigInteger? GasUsed { get; init; }

    /// <summary>
    ///     Execution logs (if available).
    /// </summary>
    public string? Logs { get; init; }
}

/// <summary>
///     Arguments for sending internal messages.
/// </summary>
public record InternalMessageArgs
{
    /// <summary>
    ///     Amount of nanotons to send.
    /// </summary>
    public required BigInteger Value { get; init; }

    /// <summary>
    ///     Extra currencies to send (optional).
    /// </summary>
    public TonDict.Dictionary<TonDict.DictKeyUint, BigInteger>? ExtraCurrency { get; init; }

    /// <summary>
    ///     Whether the message should bounce on failure (optional).
    /// </summary>
    public bool? Bounce { get; init; }

    /// <summary>
    ///     Send mode flags (optional).
    /// </summary>
    public SendMode? SendMode { get; init; }

    /// <summary>
    ///     Message body (optional).
    /// </summary>
    public Cell? Body { get; init; }
}

/// <summary>
///     Provider for contract interactions.
///     Abstracts the underlying communication with the blockchain.
/// </summary>
public interface IContractProvider
{
    /// <summary>
    ///     Get the current state of the contract.
    /// </summary>
    Task<ContractState> GetStateAsync();

    /// <summary>
    ///     Call a get method by name.
    /// </summary>
    /// <param name="name">Method name (e.g., "get_balance")</param>
    /// <param name="args">Method arguments as tuple items</param>
    Task<ContractGetMethodResult> GetAsync(string name, TupleItem[] args);

    /// <summary>
    ///     Call a get method by ID.
    /// </summary>
    /// <param name="methodId">Method ID (computed from name)</param>
    /// <param name="args">Method arguments as tuple items</param>
    Task<ContractGetMethodResult> GetAsync(int methodId, TupleItem[] args);

    /// <summary>
    ///     Send an external message to the contract.
    ///     Used for wallet transfers and other external operations.
    /// </summary>
    Task ExternalAsync(Cell message);

    /// <summary>
    ///     Send an internal message via a sender (typically a wallet).
    /// </summary>
    /// <param name="via">Sender that will send the message</param>
    /// <param name="args">Message arguments</param>
    Task InternalAsync(ISender via, InternalMessageArgs args);

    /// <summary>
    ///     Open another contract using this provider.
    /// </summary>
    OpenedContract<T> Open<T>(T contract) where T : IContract;

    /// <summary>
    ///     Get transactions for an address.
    /// </summary>
    /// <param name="address">Address to query</param>
    /// <param name="lt">Logical time to start from</param>
    /// <param name="hash">Transaction hash to start from</param>
    /// <param name="limit">Maximum number of transactions to return</param>
    Task<Transaction[]> GetTransactionsAsync(Address address, BigInteger lt, byte[] hash, int? limit = null);
}