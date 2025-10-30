using System.Numerics;
using Ton.Core.Tuple;

namespace Ton.LiteClient.Models;

/// <summary>
///     Result from running a smart contract get method
/// </summary>
public sealed class RunMethodResult
{
    /// <summary>
    ///     Exit code from the method execution (0 = success, 1 = alternative success)
    /// </summary>
    public required int ExitCode { get; init; }

    /// <summary>
    ///     Result stack as a TupleReader for easy access
    /// </summary>
    public required TupleReader Stack { get; init; }

    /// <summary>
    ///     Amount of gas used (if available)
    /// </summary>
    public BigInteger? GasUsed { get; init; }

    /// <summary>
    ///     Block where the method was executed
    /// </summary>
    public BlockId? Block { get; init; }

    /// <summary>
    ///     Shard block where the method was executed
    /// </summary>
    public BlockId? ShardBlock { get; init; }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"RunMethodResult(exitCode:{ExitCode}, gasUsed:{GasUsed}, stackSize:{Stack.Remaining})";
    }
}