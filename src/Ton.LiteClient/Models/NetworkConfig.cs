using System.Text.Json.Serialization;

namespace Ton.LiteClient.Models;

/// <summary>
///     Represents TON network configuration (global config)
/// </summary>
public sealed class NetworkConfig
{
    /// <summary>
    ///     List of lite servers available in the network
    /// </summary>
    [JsonPropertyName("liteservers")]
    public List<LiteServerConfig> LiteServers { get; set; } = new();

    /// <summary>
    ///     Validator configuration
    /// </summary>
    [JsonPropertyName("validator")]
    public ValidatorConfig? Validator { get; set; }
}

/// <summary>
///     Configuration for a single lite server
/// </summary>
public sealed class LiteServerConfig
{
    /// <summary>
    ///     Server IP address (IPv4 as integer)
    /// </summary>
    [JsonPropertyName("ip")]
    public long Ip { get; set; }

    /// <summary>
    ///     Server port
    /// </summary>
    [JsonPropertyName("port")]
    public int Port { get; set; }

    /// <summary>
    ///     Server identity (contains public key)
    /// </summary>
    [JsonPropertyName("id")]
    public ServerIdentity Id { get; set; } = new();

    /// <summary>
    ///     Converts the IP integer to a readable IP address string
    /// </summary>
    public string GetIpAddress()
    {
        // IP is stored as a signed 32-bit integer in network byte order
        uint unsignedIp = unchecked((uint)Ip);
        byte[] bytes = BitConverter.GetBytes(unsignedIp);

        // Convert from network byte order (big-endian) to host byte order if needed
        if (BitConverter.IsLittleEndian)
            Array.Reverse(bytes);

        return $"{bytes[0]}.{bytes[1]}.{bytes[2]}.{bytes[3]}";
    }
}

/// <summary>
///     Server identity containing public key information
/// </summary>
public sealed class ServerIdentity
{
    /// <summary>
    ///     Key type (e.g., "ed25519")
    /// </summary>
    [JsonPropertyName("@type")]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    ///     Public key as base64 string
    /// </summary>
    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;

    /// <summary>
    ///     Gets the public key as a byte array
    /// </summary>
    public byte[] GetPublicKey()
    {
        return Convert.FromBase64String(Key);
    }
}

/// <summary>
///     Validator configuration (not used by lite clients, included for completeness)
/// </summary>
public sealed class ValidatorConfig
{
    /// <summary>
    ///     Configuration type
    /// </summary>
    [JsonPropertyName("@type")] public string Type { get; set; } = string.Empty;

    /// <summary>
    ///     Zero state information
    /// </summary>
    [JsonPropertyName("zero_state")] public ZeroState? ZeroState { get; set; }
}

/// <summary>
///     Zero state configuration
/// </summary>
public sealed class ZeroState
{
    /// <summary>
    ///     Workchain ID
    /// </summary>
    [JsonPropertyName("workchain")] public int Workchain { get; set; }

    /// <summary>
    ///     Shard identifier
    /// </summary>
    [JsonPropertyName("shard")] public long Shard { get; set; }

    /// <summary>
    ///     Sequence number
    /// </summary>
    [JsonPropertyName("seqno")] public int Seqno { get; set; }

    /// <summary>
    ///     Root hash (hex string)
    /// </summary>
    [JsonPropertyName("root_hash")] public string RootHash { get; set; } = string.Empty;

    /// <summary>
    ///     File hash (hex string)
    /// </summary>
    [JsonPropertyName("file_hash")] public string FileHash { get; set; } = string.Empty;
}