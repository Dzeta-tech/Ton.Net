using System.Numerics;
using TonDict = Ton.Core.Dict;

namespace Ton.Core.Contracts;

/// <summary>
///     Represents the current state of a contract on the blockchain.
/// </summary>
public record ContractState
{
    /// <summary>
    ///     Contract balance in nanotons.
    /// </summary>
    public required BigInteger Balance { get; init; }

    /// <summary>
    ///     Extra currencies held by the contract (if any).
    /// </summary>
    public TonDict.Dictionary<TonDict.DictKeyUint, BigInteger>? ExtraCurrency { get; init; }

    /// <summary>
    ///     Last transaction information (logical time and hash).
    ///     Null if no transactions have been executed.
    /// </summary>
    public LastTransaction? Last { get; init; }

    /// <summary>
    ///     Current account state (uninit, active, or frozen).
    /// </summary>
    public required AccountStateInfo State { get; init; }

    /// <summary>
    ///     Last transaction information.
    /// </summary>
    public record LastTransaction(BigInteger Lt, byte[] Hash);

    /// <summary>
    ///     Account state information.
    /// </summary>
    public abstract record AccountStateInfo
    {
        /// <summary>
        ///     Account is uninitialized (not deployed yet).
        /// </summary>
        public record Uninit : AccountStateInfo;

        /// <summary>
        ///     Account is active with code and data.
        /// </summary>
        public record Active(byte[]? Code, byte[]? Data) : AccountStateInfo;

        /// <summary>
        ///     Account is frozen (usually due to low balance).
        /// </summary>
        public record Frozen(byte[] StateHash) : AccountStateInfo;
    }
}