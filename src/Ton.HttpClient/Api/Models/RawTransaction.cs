using System.Text.Json.Serialization;

namespace Ton.HttpClient.Api.Models;

/// <summary>
///     Raw transaction data from API.
/// </summary>
public record RawTransaction
{
    /// <summary>
    ///     Transaction data (base64 BOC).
    /// </summary>
    [JsonPropertyName("data")]
    public required string Data { get; init; }

    /// <summary>
    ///     Transaction ID.
    /// </summary>
    [JsonPropertyName("transaction_id")]
    public required TransactionId TransactionId { get; init; }

    /// <summary>
    ///     Unix timestamp.
    /// </summary>
    [JsonPropertyName("utime")]
    public required long Utime { get; init; }

    /// <summary>
    ///     Fee (as string).
    /// </summary>
    [JsonPropertyName("fee")]
    public required string Fee { get; init; }

    /// <summary>
    ///     Storage fee (as string).
    /// </summary>
    [JsonPropertyName("storage_fee")]
    public required string StorageFee { get; init; }

    /// <summary>
    ///     Other fee (as string).
    /// </summary>
    [JsonPropertyName("other_fee")]
    public required string OtherFee { get; init; }

    /// <summary>
    ///     In message data.
    /// </summary>
    [JsonPropertyName("in_msg")]
    public RawMessage? InMsg { get; init; }

    /// <summary>
    ///     Out messages data.
    /// </summary>
    [JsonPropertyName("out_msgs")]
    public required List<RawMessage> OutMsgs { get; init; }
}

/// <summary>
///     Raw message data from API.
/// </summary>
public record RawMessage
{
    /// <summary>
    ///     Source address.
    /// </summary>
    [JsonPropertyName("source")]
    public required string Source { get; init; }

    /// <summary>
    ///     Destination address.
    /// </summary>
    [JsonPropertyName("destination")]
    public required string Destination { get; init; }

    /// <summary>
    ///     Value (as string).
    /// </summary>
    [JsonPropertyName("value")]
    public required string Value { get; init; }

    /// <summary>
    ///     Forward fee (as string).
    /// </summary>
    [JsonPropertyName("fwd_fee")]
    public required string FwdFee { get; init; }

    /// <summary>
    ///     Message data (base64 BOC).
    /// </summary>
    [JsonPropertyName("message")]
    public string? Message { get; init; }
}