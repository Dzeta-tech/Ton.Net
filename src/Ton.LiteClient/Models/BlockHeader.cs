namespace Ton.LiteClient.Models;

/// <summary>
///     Represents a block header response from lite server.
///     Contains the block ID and the raw header proof data (typically a MerkleProof cell in BOC format).
/// </summary>
public sealed class BlockHeader
{
    /// <summary>
    ///     Block identifier
    /// </summary>
    public required BlockId Id { get; init; }

    /// <summary>
    ///     Mode flags from the response
    /// </summary>
    public required uint Mode { get; init; }

    /// <summary>
    ///     Raw header proof data in BOC format.
    ///     This is typically a MerkleProof cell that can be parsed using Cell.FromBoc() and UnwrapProof().
    /// </summary>
    public required byte[] HeaderProof { get; init; }

    /// <summary>
    ///     Returns a string representation of the block header
    /// </summary>
    public override string ToString()
    {
        return $"BlockHeader(seqno:{Id.Seqno}, mode:{Mode}, proofSize:{HeaderProof.Length} bytes)";
    }
}