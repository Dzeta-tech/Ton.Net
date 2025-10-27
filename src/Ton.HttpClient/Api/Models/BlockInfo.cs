using System.Text.Json.Serialization;

namespace Ton.HttpClient.Api.Models;

/// <summary>
///     Transaction info in a block.
/// </summary>
public record BlockTransaction(
    [property: JsonPropertyName("account")]
    string Account,
    [property: JsonPropertyName("hash")] string Hash,
    [property: JsonPropertyName("lt")] string Lt
);

/// <summary>
///     Shard info in a block.
/// </summary>
public record ShardInfo(
    [property: JsonPropertyName("workchain")]
    int Workchain,
    [property: JsonPropertyName("seqno")] int Seqno,
    [property: JsonPropertyName("shard")] string Shard,
    [property: JsonPropertyName("rootHash")]
    string RootHash,
    [property: JsonPropertyName("fileHash")]
    string FileHash,
    [property: JsonPropertyName("transactions")]
    List<BlockTransaction> Transactions
);

/// <summary>
///     Block details.
/// </summary>
public record BlockDetails(
    [property: JsonPropertyName("shards")] List<ShardInfo> Shards
);

/// <summary>
///     Block response from v4 API.
/// </summary>
public record Block(
    [property: JsonPropertyName("exist")] bool Exist,
    [property: JsonPropertyName("block")] BlockDetails? BlockData
);