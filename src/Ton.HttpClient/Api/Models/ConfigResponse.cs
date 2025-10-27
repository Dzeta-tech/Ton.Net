using System.Text.Json.Serialization;

namespace Ton.HttpClient.Api.Models;

/// <summary>
///     Global balance info.
/// </summary>
public record GlobalBalance(
    [property: JsonPropertyName("coins")] string Coins
);

/// <summary>
///     Config details.
/// </summary>
public record ConfigDetails(
    [property: JsonPropertyName("cell")] string Cell,
    [property: JsonPropertyName("address")]
    string Address,
    [property: JsonPropertyName("globalBalance")]
    GlobalBalance GlobalBalance
);

/// <summary>
///     Config response from v4 API.
/// </summary>
public record ConfigResponse(
    [property: JsonPropertyName("config")] ConfigDetails Config
);