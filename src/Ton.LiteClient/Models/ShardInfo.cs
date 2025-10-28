namespace Ton.LiteClient.Models;

/// <summary>
/// Represents information about all shards in a given block
/// </summary>
public sealed class ShardInfo
{
    /// <summary>
    /// Block ID for which shard info is provided
    /// </summary>
    public required BlockId Block { get; init; }

    /// <summary>
    /// Collection of all shards with their descriptors
    /// </summary>
    public required ShardCollection Shards { get; init; }

    /// <summary>
    /// Raw proof data from the lite server
    /// </summary>
    public byte[]? Proof { get; init; }

    /// <summary>
    /// Raw shard data from the lite server
    /// </summary>
    public byte[]? Data { get; init; }

    public override string ToString() =>
        $"ShardInfo(block:{Block.Seqno}, shards:{Shards.Count})";
}

