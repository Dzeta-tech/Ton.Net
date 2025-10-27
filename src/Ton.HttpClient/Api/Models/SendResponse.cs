using System.Text.Json.Serialization;

namespace Ton.HttpClient.Api.Models;

/// <summary>
///     Send message response from v4 API.
/// </summary>
public record SendResponse(
    [property: JsonPropertyName("status")] int Status
);