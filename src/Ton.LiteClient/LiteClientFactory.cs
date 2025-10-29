using System.Net.Http.Json;
using System.Text.Json;
using Ton.LiteClient.Engines;
using Ton.LiteClient.Models;

namespace Ton.LiteClient;

/// <summary>
///     Factory class for creating LiteClient instances with common configurations
/// </summary>
public static class LiteClientFactory
{
    static readonly HttpClient httpClient = new();

    /// <summary>
    ///     Creates a new lite client with a single server connection.
    ///     The client will automatically connect when the first request is made.
    /// </summary>
    /// <param name="host">Server host/IP</param>
    /// <param name="port">Server port</param>
    /// <param name="serverPublicKey">Server's Ed25519 public key (32 bytes)</param>
    /// <param name="reconnectTimeoutMs">Reconnection timeout in milliseconds (default: 10000)</param>
    /// <param name="ownsEngine">Whether the client should dispose the engine when disposed (default: true)</param>
    /// <returns>New LiteClient instance with a LiteSingleEngine</returns>
    public static LiteClient Create(
        string host,
        int port,
        byte[] serverPublicKey,
        int reconnectTimeoutMs = 10000,
        bool ownsEngine = true)
    {
        LiteSingleEngine engine = new(host, port, serverPublicKey, reconnectTimeoutMs);
        return new LiteClient(engine, ownsEngine);
    }

    /// <summary>
    ///     Creates a new lite client with a single server connection (base64 public key).
    ///     The client will automatically connect when the first request is made.
    /// </summary>
    /// <param name="host">Server host/IP</param>
    /// <param name="port">Server port</param>
    /// <param name="serverPublicKeyBase64">Server's Ed25519 public key as base64 string</param>
    /// <param name="reconnectTimeoutMs">Reconnection timeout in milliseconds (default: 10000)</param>
    /// <param name="ownsEngine">Whether the client should dispose the engine when disposed (default: true)</param>
    /// <returns>New LiteClient instance with a LiteSingleEngine</returns>
    public static LiteClient Create(
        string host,
        int port,
        string serverPublicKeyBase64,
        int reconnectTimeoutMs = 10000,
        bool ownsEngine = true)
    {
        byte[] publicKey = Convert.FromBase64String(serverPublicKeyBase64);
        return Create(host, port, publicKey, reconnectTimeoutMs, ownsEngine);
    }

    /// <summary>
    ///     Creates a new lite client from TON network configuration URL.
    ///     If config contains multiple servers, uses round-robin load balancing.
    ///     If only one server, uses single connection.
    /// </summary>
    /// <param name="configUrl">URL to TON network config JSON (e.g., https://ton.org/global-config.json)</param>
    /// <param name="reconnectTimeoutMs">Reconnection timeout in milliseconds (default: 10000)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>New LiteClient instance configured from network config</returns>
    public static async Task<LiteClient> CreateFromUrlAsync(
        string configUrl,
        int reconnectTimeoutMs = 10000,
        CancellationToken cancellationToken = default)
    {
        // Download and parse config
        NetworkConfig config = await httpClient.GetFromJsonAsync<NetworkConfig>(
            configUrl,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true },
            cancellationToken)
            ?? throw new InvalidOperationException("Failed to load network config from URL");

        return CreateFromConfig(config, reconnectTimeoutMs);
    }

    /// <summary>
    ///     Creates a new lite client from TON network configuration.
    ///     If config contains multiple servers, uses round-robin load balancing.
    ///     If only one server, uses single connection.
    /// </summary>
    /// <param name="config">Parsed network configuration</param>
    /// <param name="reconnectTimeoutMs">Reconnection timeout in milliseconds (default: 10000)</param>
    /// <returns>New LiteClient instance configured from network config</returns>
    public static LiteClient CreateFromConfig(
        NetworkConfig config,
        int reconnectTimeoutMs = 10000)
    {
        ArgumentNullException.ThrowIfNull(config);

        if (config.LiteServers == null || config.LiteServers.Count == 0)
            throw new ArgumentException("Network config must contain at least one lite server", nameof(config));

        // If only one server, use single engine
        if (config.LiteServers.Count == 1)
        {
            LiteServerConfig serverConfig = config.LiteServers[0];
            string host = serverConfig.GetIpAddress();
            byte[] publicKey = serverConfig.Id.GetPublicKey();

            LiteSingleEngine engine = new(host, serverConfig.Port, publicKey, reconnectTimeoutMs);
            return new LiteClient(engine, ownsEngine: true);
        }

        // Multiple servers - use round-robin
        ILiteEngine[] engines = config.LiteServers.Select(serverConfig =>
        {
            string host = serverConfig.GetIpAddress();
            byte[] publicKey = serverConfig.Id.GetPublicKey();
            return (ILiteEngine)new LiteSingleEngine(host, serverConfig.Port, publicKey, reconnectTimeoutMs);
        }).ToArray();

        LiteRoundRobinEngine roundRobinEngine = new(engines);
        return new LiteClient(roundRobinEngine, ownsEngine: true);
    }

    /// <summary>
    ///     Creates a new lite client with round-robin load balancing across multiple servers.
    ///     Use this when you want to explicitly use multiple servers for load distribution.
    /// </summary>
    /// <param name="servers">Array of server configurations (host, port, publicKey)</param>
    /// <param name="reconnectTimeoutMs">Reconnection timeout in milliseconds (default: 10000)</param>
    /// <returns>New LiteClient instance with round-robin engine</returns>
    public static LiteClient CreateRoundRobin(
        (string Host, int Port, byte[] PublicKey)[] servers,
        int reconnectTimeoutMs = 10000)
    {
        ArgumentNullException.ThrowIfNull(servers);

        if (servers.Length == 0)
            throw new ArgumentException("Must provide at least one server", nameof(servers));

        ILiteEngine[] engines = servers.Select(s =>
            (ILiteEngine)new LiteSingleEngine(s.Host, s.Port, s.PublicKey, reconnectTimeoutMs)
        ).ToArray();

        LiteRoundRobinEngine roundRobinEngine = new(engines);
        return new LiteClient(roundRobinEngine, ownsEngine: true);
    }

    /// <summary>
    ///     Creates a new lite client with round-robin load balancing across multiple servers (base64 public keys).
    /// </summary>
    /// <param name="servers">Array of server configurations (host, port, publicKeyBase64)</param>
    /// <param name="reconnectTimeoutMs">Reconnection timeout in milliseconds (default: 10000)</param>
    /// <returns>New LiteClient instance with round-robin engine</returns>
    public static LiteClient CreateRoundRobin(
        (string Host, int Port, string PublicKeyBase64)[] servers,
        int reconnectTimeoutMs = 10000)
    {
        ArgumentNullException.ThrowIfNull(servers);

        if (servers.Length == 0)
            throw new ArgumentException("Must provide at least one server", nameof(servers));

        (string Host, int Port, byte[] PublicKey)[] serversWithBytes = servers
            .Select(s => (s.Host, s.Port, Convert.FromBase64String(s.PublicKeyBase64)))
            .ToArray();

        return CreateRoundRobin(serversWithBytes, reconnectTimeoutMs);
    }
}


