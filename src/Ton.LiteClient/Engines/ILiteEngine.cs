namespace Ton.LiteClient.Engines;

/// <summary>
/// Interface for TON lite server engine that handles low-level protocol communication
/// </summary>
public interface ILiteEngine : IDisposable
{
    /// <summary>
    /// Executes a lite server query with automatic serialization/deserialization
    /// </summary>
    /// <typeparam name="TRequest">Request type</typeparam>
    /// <typeparam name="TResponse">Response type</typeparam>
    /// <param name="functionId">TL function constructor ID</param>
    /// <param name="requestWriter">Action to write request to TL buffer</param>
    /// <param name="responseReader">Function to read response from TL buffer</param>
    /// <param name="timeout">Query timeout in milliseconds (default: 5000ms)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Deserialized response</returns>
    Task<TResponse> QueryAsync<TRequest, TResponse>(
        uint functionId,
        Action<Ton.Adnl.TL.TLWriteBuffer, TRequest> requestWriter,
        Func<Ton.Adnl.TL.TLReadBuffer, TResponse> responseReader,
        TRequest request,
        int timeout = 5000,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns true if the engine is ready to process queries
    /// </summary>
    bool IsReady { get; }

    /// <summary>
    /// Returns true if the engine is closed
    /// </summary>
    bool IsClosed { get; }

    /// <summary>
    /// Closes the engine and releases resources
    /// </summary>
    Task CloseAsync();

    /// <summary>
    /// Fired when connection is established (before handshake)
    /// </summary>
    event EventHandler? Connected;

    /// <summary>
    /// Fired when engine is ready to process queries (after handshake)
    /// </summary>
    event EventHandler? Ready;

    /// <summary>
    /// Fired when connection is closed
    /// </summary>
    event EventHandler? Closed;

    /// <summary>
    /// Fired when an error occurs
    /// </summary>
    event EventHandler<Exception>? Error;
}

