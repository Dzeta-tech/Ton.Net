using System.Text.Json.Serialization;

namespace Ton.HttpClient.Api.Models;

/// <summary>
///     Extended block identifier from Toncenter API.
/// </summary>
public record BlockIdExt
{
    /// <summary>
    ///     Workchain ID.
    /// </summary>
    [JsonPropertyName("workchain")]
    public required int Workchain { get; init; }

    /// <summary>
    ///     Shard ID (hex string).
    /// </summary>
    [JsonPropertyName("shard")]
    public required string Shard { get; init; }

    /// <summary>
    ///     Block sequence number.
    /// </summary>
    [JsonPropertyName("seqno")]
    public required int Seqno { get; init; }

    /// <summary>
    ///     Root hash (base64).
    /// </summary>
    [JsonPropertyName("root_hash")]
    public required string RootHash { get; init; }

    /// <summary>
    ///     File hash (base64).
    /// </summary>
    [JsonPropertyName("file_hash")]
    public required string FileHash { get; init; }
}