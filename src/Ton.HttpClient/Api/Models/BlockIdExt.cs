namespace Ton.HttpClient.Api.Models;

/// <summary>
/// Extended block identifier from Toncenter API.
/// </summary>
public record BlockIdExt
{
    /// <summary>
    /// Workchain ID.
    /// </summary>
    public required int Workchain { get; init; }

    /// <summary>
    /// Shard ID (hex string).
    /// </summary>
    public required string Shard { get; init; }

    /// <summary>
    /// Block sequence number.
    /// </summary>
    public required int Seqno { get; init; }

    /// <summary>
    /// Root hash (base64).
    /// </summary>
    public required string RootHash { get; init; }

    /// <summary>
    /// File hash (base64).
    /// </summary>
    public required string FileHash { get; init; }
}

