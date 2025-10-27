using System.Text.Json.Serialization;

namespace Ton.HttpClient.Api.Models;

/// <summary>
/// Block transactions response.
/// </summary>
public record BlockTransactions
{
    /// <summary>
    /// List of transactions in the block.
    /// </summary>
    [JsonPropertyName("transactions")]
    public required List<ShardTransaction> Transactions { get; init; }

    /// <summary>
    /// Whether the list is incomplete (more transactions exist).
    /// </summary>
    [JsonPropertyName("incomplete")]
    public required bool Incomplete { get; init; }
}

/// <summary>
/// Transaction reference in a shard block.
/// </summary>
public record ShardTransaction
{
    /// <summary>
    /// Account address (raw format).
    /// </summary>
    [JsonPropertyName("account")]
    public required string Account { get; init; }

    /// <summary>
    /// Logical time.
    /// </summary>
    [JsonPropertyName("lt")]
    public required string Lt { get; init; }

    /// <summary>
    /// Transaction hash (base64).
    /// </summary>
    [JsonPropertyName("hash")]
    public required string Hash { get; init; }
}

