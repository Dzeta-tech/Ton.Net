namespace Ton.HttpClient;

/// <summary>
/// Parameters for TonClient initialization.
/// </summary>
public record TonClientParameters
{
    /// <summary>
    /// API endpoint URL.
    /// </summary>
    public required string Endpoint { get; init; }

    /// <summary>
    /// HTTP request timeout in milliseconds (default: 5000ms).
    /// </summary>
    public int Timeout { get; init; } = 5000;

    /// <summary>
    /// Optional API key for authenticated requests.
    /// </summary>
    public string? ApiKey { get; init; }
}

/// <summary>
/// Parameters for TonClient4 initialization.
/// </summary>
public record TonClient4Parameters
{
    /// <summary>
    /// API endpoint URL.
    /// </summary>
    public required string Endpoint { get; init; }

    /// <summary>
    /// HTTP request timeout in milliseconds (default: 5000ms).
    /// </summary>
    public int Timeout { get; init; } = 5000;
}

