using Ton.Adnl.Protocol;
using Ton.Adnl.TL;

namespace Ton.LiteClient.Engines;

/// <summary>
///     Round-robin lite engine that distributes queries across multiple engines for load balancing.
///     Matches the behavior of ton-lite-client's LiteRoundRobinEngine.
/// </summary>
public sealed class LiteRoundRobinEngine : ILiteEngine
{
    readonly List<ILiteEngine> allEngines = new();
    readonly List<ILiteEngine> readyEngines = new();
    readonly Lock stateLock = new();
    int counter;
    bool isClosed;

    /// <summary>
    ///     Creates a new round-robin engine
    /// </summary>
    /// <param name="engines">Array of engines to distribute queries across</param>
    /// <exception cref="ArgumentException">If engines array is empty</exception>
    public LiteRoundRobinEngine(params ILiteEngine[] engines)
    {
        ArgumentNullException.ThrowIfNull(engines);

        if (engines.Length == 0)
            throw new ArgumentException("Must provide at least one engine", nameof(engines));

        foreach (ILiteEngine engine in engines) AddSingleEngine(engine);
    }

    /// <summary>
    ///     Gets the number of engines in the round-robin pool
    /// </summary>
    public int EngineCount
    {
        get
        {
            lock (stateLock)
            {
                return allEngines.Count;
            }
        }
    }

    /// <summary>
    ///     Gets the number of ready engines
    /// </summary>
    public int ReadyEngineCount
    {
        get
        {
            lock (stateLock)
            {
                return readyEngines.Count;
            }
        }
    }

    /// <summary>
    ///     Gets the underlying engines (read-only)
    /// </summary>
    public IReadOnlyList<ILiteEngine> Engines
    {
        get
        {
            lock (stateLock)
            {
                return allEngines.ToArray();
            }
        }
    }

    /// <summary>
    ///     Returns true if the engine is not closed (matches JS behavior)
    /// </summary>
    public bool IsReady
    {
        get
        {
            lock (stateLock)
            {
                return !isClosed;
            }
        }
    }

    /// <summary>
    ///     Returns true if the engine is closed
    /// </summary>
    public bool IsClosed
    {
        get
        {
            lock (stateLock)
            {
                return isClosed;
            }
        }
    }

    public event EventHandler? Connected;
    public event EventHandler? Ready;
    public event EventHandler? Closed;
    public event EventHandler<Exception>? Error;

    /// <summary>
    ///     Executes a query using round-robin selection of available engines.
    ///     Implements retry logic matching ton-lite-client behavior.
    /// </summary>
    public async Task<TResponse> QueryAsync<TRequest, TResponse>(
        TRequest request,
        Func<TLReadBuffer, TResponse> responseReader,
        int timeout = 5000,
        CancellationToken cancellationToken = default)
        where TRequest : ILiteRequest
    {
        lock (stateLock)
        {
            if (isClosed)
                throw new InvalidOperationException("Engine is closed");
        }

        int attempts = 0;
        int id;
        int errorsCount = 0;

        lock (stateLock)
        {
            id = counter++ % Math.Max(readyEngines.Count, 1);
        }

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            ILiteEngine? engine = null;
            int readyCount;

            lock (stateLock)
            {
                readyCount = readyEngines.Count;

                if (readyCount == 0 || id >= readyCount || !readyEngines[id].IsReady)
                {
                    // Move to next engine
                    id = (id + 1) % Math.Max(readyCount, 1);
                    attempts++;

                    // Wait if we've tried all engines or no engines are ready
                    if (attempts >= Math.Max(readyCount, 1) || readyCount == 0)
                    {
                        if (attempts > 200)
                            throw new InvalidOperationException("No engines are available");

                        // Don't hold lock during delay
                    }
                    else
                    {
                        continue;
                    }
                }
                else
                {
                    engine = readyEngines[id];
                }
            }

            // Wait outside of lock if we need to
            if (engine == null && (attempts >= Math.Max(readyCount, 1) || readyCount == 0))
            {
                await Task.Delay(100, cancellationToken);
                continue;
            }

            if (engine == null)
                continue;

            try
            {
                TResponse result = await engine.QueryAsync(request, responseReader, timeout, cancellationToken);
                return result;
            }
            catch (TimeoutException)
            {
                // Continue on timeout - try next engine
                lock (stateLock)
                {
                    id = (id + 1) % Math.Max(readyEngines.Count, 1);
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // Move to next engine
                lock (stateLock)
                {
                    id = (id + 1) % Math.Max(readyEngines.Count, 1);
                }

                errorsCount++;

                if (errorsCount > 20)
                    throw;

                // Small delay before retry
                await Task.Delay(100, cancellationToken);
            }
        }
    }

    public async Task CloseAsync()
    {
        ILiteEngine[] enginesToClose;

        lock (stateLock)
        {
            if (isClosed)
                return;

            isClosed = true;
            enginesToClose = allEngines.ToArray();
        }

        // Close all engines in parallel
        await Task.WhenAll(enginesToClose.Select(e => e.CloseAsync()));
    }

    public void Dispose()
    {
        CloseAsync().GetAwaiter().GetResult();

        // Unsubscribe from events and dispose engines
        ILiteEngine[] enginesToDispose;
        lock (stateLock)
        {
            enginesToDispose = allEngines.ToArray();
        }

        foreach (ILiteEngine engine in enginesToDispose)
        {
            engine.Connected -= OnEngineConnected;
            engine.Ready -= OnEngineReady;
            engine.Closed -= OnEngineClosed;
            engine.Error -= OnEngineError;

            engine.Dispose();
        }
    }

    /// <summary>
    ///     Adds a single engine to the round-robin pool
    /// </summary>
    void AddSingleEngine(ILiteEngine engine)
    {
        lock (stateLock)
        {
            if (allEngines.Contains(engine))
                throw new InvalidOperationException("Engine already exists");

            allEngines.Add(engine);
        }

        // Subscribe to events to dynamically manage ready engines list
        engine.Ready += (s, e) =>
        {
            lock (stateLock)
            {
                if (!readyEngines.Contains(engine))
                    readyEngines.Add(engine);
            }
        };

        engine.Closed += (s, e) =>
        {
            lock (stateLock)
            {
                readyEngines.Remove(engine);
            }
        };

        engine.Error += (s, ex) =>
        {
            lock (stateLock)
            {
                readyEngines.Remove(engine);
            }
        };

        // If engine is already ready, add it immediately
        if (engine.IsReady)
            lock (stateLock)
            {
                if (!readyEngines.Contains(engine))
                    readyEngines.Add(engine);
            }

        // Forward events
        engine.Connected += OnEngineConnected;
        engine.Ready += OnEngineReady;
        engine.Closed += OnEngineClosed;
        engine.Error += OnEngineError;
    }

    void OnEngineConnected(object? sender, EventArgs e)
    {
        Connected?.Invoke(this, e);
    }

    void OnEngineReady(object? sender, EventArgs e)
    {
        Ready?.Invoke(this, e);
    }

    void OnEngineClosed(object? sender, EventArgs e)
    {
        // Only fire Closed if all engines are closed
        if (IsClosed)
            Closed?.Invoke(this, e);
    }

    void OnEngineError(object? sender, Exception ex)
    {
        Error?.Invoke(this, ex);
    }
}