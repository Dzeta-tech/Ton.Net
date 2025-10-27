using System.Text.Json.Serialization;

namespace Ton.HttpClient.Api.Models;

/// <summary>
///     Response containing shard information.
/// </summary>
public record ShardResponse
{
    /// <summary>
    ///     List of shards.
    /// </summary>
    [JsonPropertyName("shards")]
    public required List<BlockIdExt> Shards { get; init; }
}