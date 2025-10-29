using System.Threading.RateLimiting;
using Ton.Adnl.Protocol;
using Ton.Adnl.TL;

namespace Ton.LiteClient.Engines;

/// <summary>
///     Rate-limited lite engine wrapper that throttles queries to the underlying engine.
///     Useful for public servers that have rate limits or to prevent overwhelming a single server.
///     Uses token bucket algorithm for smooth rate limiting.
/// </summary>
public sealed class RateLimitedLiteEngine : ILiteEngine
{
    readonly TokenBucketRateLimiter rateLimiter;

    /// <summary>
    ///     Creates a new rate-limited engine
    /// </summary>
    /// <param name="innerEngine">The underlying engine to wrap</param>
    /// <param name="requestsPerSecond">Maximum number of requests per second (default: 10)</param>
    public RateLimitedLiteEngine(ILiteEngine innerEngine, int requestsPerSecond = 10)
    {
        ArgumentNullException.ThrowIfNull(innerEngine);

        if (requestsPerSecond <= 0)
            throw new ArgumentException("Requests per second must be positive", nameof(requestsPerSecond));

        InnerEngine = innerEngine;

        // Use token bucket rate limiter with burst capacity equal to permits per second
        rateLimiter = new TokenBucketRateLimiter(new TokenBucketRateLimiterOptions
        {
            TokenLimit = requestsPerSecond,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = 100,
            ReplenishmentPeriod = TimeSpan.FromSeconds(1),
            TokensPerPeriod = requestsPerSecond,
            AutoReplenishment = true
        });

        // Subscribe to inner engine events and forward them
        innerEngine.Connected += (s, e) => Connected?.Invoke(this, e);
        innerEngine.Ready += (s, e) => Ready?.Invoke(this, e);
        innerEngine.Closed += (s, e) => Closed?.Invoke(this, e);
        innerEngine.Error += (s, e) => Error?.Invoke(this, e);
    }

    /// <summary>
    ///     Gets the underlying engine
    /// </summary>
    public ILiteEngine InnerEngine { get; }

    /// <inheritdoc />
    public bool IsReady => InnerEngine.IsReady;

    /// <inheritdoc />
    public bool IsClosed => InnerEngine.IsClosed;

    /// <inheritdoc />
    public event EventHandler? Connected;

    /// <inheritdoc />
    public event EventHandler? Ready;

    /// <inheritdoc />
    public event EventHandler? Closed;

    /// <inheritdoc />
    public event EventHandler<Exception>? Error;

    /// <summary>
    ///     Executes a query with rate limiting
    /// </summary>
    public async Task<TResponse> QueryAsync<TRequest, TResponse>(
        TRequest request,
        Func<TLReadBuffer, TResponse> responseReader,
        CancellationToken cancellationToken = default)
        where TRequest : ILiteRequest
    {
        // Acquire rate limit token (will wait if needed)
        using RateLimitLease lease = await rateLimiter.AcquireAsync(1, cancellationToken);

        if (!lease.IsAcquired) throw new InvalidOperationException("Failed to acquire rate limit token");

        // Execute the query
        return await InnerEngine.QueryAsync(request, responseReader, cancellationToken);
    }

    /// <inheritdoc />
    public async Task CloseAsync()
    {
        await InnerEngine.CloseAsync();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        rateLimiter.Dispose();
        InnerEngine.Dispose();
    }
}