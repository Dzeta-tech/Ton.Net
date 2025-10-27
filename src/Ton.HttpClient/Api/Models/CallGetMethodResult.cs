using System.Text.Json.Serialization;

namespace Ton.HttpClient.Api.Models;

/// <summary>
///     Result from calling a get method on a contract.
/// </summary>
public record CallGetMethodResult
{
    /// <summary>
    ///     Gas used during execution.
    /// </summary>
    [JsonPropertyName("gas_used")]
    public required int GasUsed { get; init; }

    /// <summary>
    ///     Exit code (0 = success).
    /// </summary>
    [JsonPropertyName("exit_code")]
    public required int ExitCode { get; init; }

    /// <summary>
    ///     TVM stack result (raw JSON array).
    /// </summary>
    [JsonPropertyName("stack")]
    public required List<object> Stack { get; init; }
}