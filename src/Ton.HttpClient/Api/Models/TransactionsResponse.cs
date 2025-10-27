using System.Text.Json.Serialization;

namespace Ton.HttpClient.Api.Models;

/// <summary>
///     Transactions response from v4 API.
/// </summary>
public record TransactionsResponse(
    [property: JsonPropertyName("blocks")] List<BlockRef> Blocks,
    [property: JsonPropertyName("boc")] string Boc
);