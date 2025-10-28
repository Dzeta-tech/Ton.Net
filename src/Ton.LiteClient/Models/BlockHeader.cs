using System.Numerics;

namespace Ton.LiteClient.Models;

/// <summary>
///     Represents a block header with metadata
/// </summary>
public sealed class BlockHeader
{
    /// <summary>
    ///     Block identifier
    /// </summary>
    public required BlockId Id { get; init; }

    /// <summary>
    ///     Global ID of the blockchain
    /// </summary>
    public required int GlobalId { get; init; }

    /// <summary>
    ///     Version of the block format
    /// </summary>
    public required int Version { get; init; }

    /// <summary>
    ///     Flags
    /// </summary>
    public required int Flags { get; init; }

    /// <summary>
    ///     After merge flag
    /// </summary>
    public required bool AfterMerge { get; init; }

    /// <summary>
    ///     After split flag
    /// </summary>
    public required bool AfterSplit { get; init; }

    /// <summary>
    ///     Before split flag
    /// </summary>
    public required bool BeforeSplit { get; init; }

    /// <summary>
    ///     Want merge flag
    /// </summary>
    public required bool WantMerge { get; init; }

    /// <summary>
    ///     Want split flag
    /// </summary>
    public required bool WantSplit { get; init; }

    /// <summary>
    ///     Validator list hash short
    /// </summary>
    public required int ValidatorListHashShort { get; init; }

    /// <summary>
    ///     Catchain seqno
    /// </summary>
    public required int CatchainSeqno { get; init; }

    /// <summary>
    ///     Min ref masterchain seqno
    /// </summary>
    public required int MinRefMcSeqno { get; init; }

    /// <summary>
    ///     Is key block
    /// </summary>
    public required bool IsKeyBlock { get; init; }

    /// <summary>
    ///     Previous key block seqno
    /// </summary>
    public required int PrevKeyBlockSeqno { get; init; }

    /// <summary>
    ///     Unix timestamp when block was generated
    /// </summary>
    public required int GenUtime { get; init; }

    /// <summary>
    ///     Start logical time
    /// </summary>
    public required BigInteger StartLt { get; init; }

    /// <summary>
    ///     End logical time
    /// </summary>
    public required BigInteger EndLt { get; init; }

    /// <summary>
    ///     Returns the generation time as DateTimeOffset
    /// </summary>
    public DateTimeOffset GeneratedAt => DateTimeOffset.FromUnixTimeSeconds(GenUtime);

    public override string ToString()
    {
        return $"BlockHeader(seqno:{Id.Seqno}, time:{GeneratedAt:yyyy-MM-dd HH:mm:ss} UTC, keyBlock:{IsKeyBlock})";
    }
}