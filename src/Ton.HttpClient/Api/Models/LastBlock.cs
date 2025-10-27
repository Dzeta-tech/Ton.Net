using System.Text.Json.Serialization;

namespace Ton.HttpClient.Api.Models;

/// <summary>
///     Last block information for v4 API.
/// </summary>
public record LastBlockInfo(
    [property: JsonPropertyName("seqno")] int Seqno,
    [property: JsonPropertyName("shard")] string Shard,
    [property: JsonPropertyName("workchain")]
    int Workchain,
    [property: JsonPropertyName("fileHash")]
    string FileHash,
    [property: JsonPropertyName("rootHash")]
    string RootHash
);

/// <summary>
///     Init block information for v4 API.
/// </summary>
public record InitBlockInfo(
    [property: JsonPropertyName("fileHash")]
    string FileHash,
    [property: JsonPropertyName("rootHash")]
    string RootHash
);

/// <summary>
///     Last block response from v4 API.
/// </summary>
public record LastBlock(
    [property: JsonPropertyName("last")] LastBlockInfo Last,
    [property: JsonPropertyName("init")] InitBlockInfo Init,
    [property: JsonPropertyName("stateRootHash")]
    string StateRootHash,
    [property: JsonPropertyName("now")] int Now
);