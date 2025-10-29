using Ton.Adnl.Protocol;
using Ton.Adnl.TL;

namespace Ton.LiteClient.Engines;

/// <summary>
///     Interface for TON lite server engine that handles low-level protocol communication
/// </summary>
public interface ILiteEngine : IDisposable
{
    /// <summary>
    ///     Returns true if the engine is ready to process queries
    /// </summary>
    bool IsReady { get; }

    /// <summary>
    ///     Returns true if the engine is closed
    /// </summary>
    bool IsClosed { get; }

    /// <summary>
    ///     Executes a lite server query using a generated request class
    /// </summary>
    /// <typeparam name="TRequest">Request type implementing ILiteRequest</typeparam>
    /// <typeparam name="TResponse">Response type</typeparam>
    /// <param name="request">Request object</param>
    /// <param name="responseReader">Function to read response from TL buffer</param>
    /// <param name="timeout">Query timeout in milliseconds (default: 5000ms)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Deserialized response</returns>
    Task<TResponse> QueryAsync<TRequest, TResponse>(
        TRequest request,
        Func<TLReadBuffer, TResponse> responseReader,
        int timeout = 5000,
        CancellationToken cancellationToken = default)
        where TRequest : ILiteRequest;

    /// <summary>
    ///     Closes the engine and releases resources
    /// </summary>
    Task CloseAsync();

    /// <summary>
    ///     Fired when connection is established (before handshake)
    /// </summary>
    event EventHandler? Connected;

    /// <summary>
    ///     Fired when engine is ready to process queries (after handshake)
    /// </summary>
    event EventHandler? Ready;

    /// <summary>
    ///     Fired when connection is closed
    /// </summary>
    event EventHandler? Closed;

    /// <summary>
    ///     Fired when an error occurs
    /// </summary>
    event EventHandler<Exception>? Error;
}