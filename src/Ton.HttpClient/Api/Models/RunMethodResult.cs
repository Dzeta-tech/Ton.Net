using System.Text.Json.Serialization;

namespace Ton.HttpClient.Api.Models;

/// <summary>
///     Run method result from v4 API.
/// </summary>
public record V4RunMethodResult(
    [property: JsonPropertyName("exitCode")]
    int ExitCode,
    [property: JsonPropertyName("resultRaw")]
    string? ResultRaw,
    [property: JsonPropertyName("block")] BlockRef Block,
    [property: JsonPropertyName("shardBlock")]
    BlockRef ShardBlock
);