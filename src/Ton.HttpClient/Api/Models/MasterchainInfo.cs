using System.Text.Json.Serialization;

namespace Ton.HttpClient.Api.Models;

/// <summary>
/// Masterchain information.
/// </summary>
public record MasterchainInfo
{
    /// <summary>
    /// Last masterchain block.
    /// </summary>
    [JsonPropertyName("last")]
    public required BlockIdExt Last { get; init; }

    /// <summary>
    /// State root hash.
    /// </summary>
    [JsonPropertyName("state_root_hash")]
    public required string StateRootHash { get; init; }

    /// <summary>
    /// Init block.
    /// </summary>
    [JsonPropertyName("init")]
    public required BlockIdExt Init { get; init; }
}

