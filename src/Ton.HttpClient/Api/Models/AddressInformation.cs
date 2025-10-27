using System.Text.Json.Serialization;

namespace Ton.HttpClient.Api.Models;

/// <summary>
///     Address information from Toncenter API.
/// </summary>
public record AddressInformation
{
    /// <summary>
    ///     Account balance in nanotons (as string to handle large numbers).
    /// </summary>
    [JsonPropertyName("balance")]
    public required string Balance { get; init; }

    /// <summary>
    ///     Account state: "active", "uninitialized", or "frozen".
    /// </summary>
    [JsonPropertyName("state")]
    public required string State { get; init; }

    /// <summary>
    ///     Account data (base64 encoded BOC).
    /// </summary>
    [JsonPropertyName("data")]
    public required string Data { get; init; }

    /// <summary>
    ///     Account code (base64 encoded BOC).
    /// </summary>
    [JsonPropertyName("code")]
    public required string Code { get; init; }

    /// <summary>
    ///     Last transaction ID.
    /// </summary>
    [JsonPropertyName("last_transaction_id")]
    public required TransactionId LastTransactionId { get; init; }

    /// <summary>
    ///     Block ID where this state was observed.
    /// </summary>
    [JsonPropertyName("block_id")]
    public required BlockIdExt BlockId { get; init; }

    /// <summary>
    ///     Sync time (unix timestamp).
    /// </summary>
    [JsonPropertyName("sync_utime")]
    public required long SyncUtime { get; init; }

    /// <summary>
    ///     Extra currencies (jettons, etc.).
    /// </summary>
    [JsonPropertyName("extra_currencies")]
    public List<ExtraCurrencyInfo>? ExtraCurrencies { get; init; }
}

/// <summary>
///     Transaction ID reference.
/// </summary>
public record TransactionId
{
    /// <summary>
    ///     Logical time.
    /// </summary>
    [JsonPropertyName("lt")]
    public required string Lt { get; init; }

    /// <summary>
    ///     Transaction hash (base64).
    /// </summary>
    [JsonPropertyName("hash")]
    public required string Hash { get; init; }
}

/// <summary>
///     Extra currency information.
/// </summary>
public record ExtraCurrencyInfo
{
    /// <summary>
    ///     Currency ID.
    /// </summary>
    [JsonPropertyName("id")]
    public required int Id { get; init; }

    /// <summary>
    ///     Amount (as string).
    /// </summary>
    [JsonPropertyName("amount")]
    public required string Amount { get; init; }
}