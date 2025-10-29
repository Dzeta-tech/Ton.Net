using System.Collections.Concurrent;
using System.Numerics;
using Ton.Adnl;
using Ton.Adnl.Crypto;
using Ton.Adnl.Protocol;
using Ton.Adnl.TL;

namespace Ton.LiteClient.Engines;

/// <summary>
///     Single connection lite engine that uses ADNL client for communication
/// </summary>
public sealed class LiteSingleEngine : ILiteEngine
{
    readonly string host;
    readonly ConcurrentDictionary<string, PendingQuery> pendingQueries = new();
    readonly int port;
    readonly int reconnectTimeoutMs;
    readonly byte[] serverPublicKey;
    readonly Lock stateLock = new();

    AdnlClient? client;
    bool isClosed = true;
    bool isReady;

    /// <summary>
    ///     Creates a new lite engine instance
    /// </summary>
    /// <param name="host">Server host/IP</param>
    /// <param name="port">Server port</param>
    /// <param name="serverPublicKey">Server's Ed25519 public key (32 bytes)</param>
    /// <param name="reconnectTimeoutMs">Reconnection timeout in milliseconds (default: 10000)</param>
    public LiteSingleEngine(string host, int port, byte[] serverPublicKey, int reconnectTimeoutMs = 10000)
    {
        ArgumentNullException.ThrowIfNull(host);
        ArgumentNullException.ThrowIfNull(serverPublicKey);

        if (serverPublicKey.Length != 32)
            throw new ArgumentException("Server public key must be 32 bytes", nameof(serverPublicKey));

        this.host = host;
        this.port = port;
        this.serverPublicKey = serverPublicKey;
        this.reconnectTimeoutMs = reconnectTimeoutMs;

        // Start connection
        Connect();
    }

    /// <summary>
    ///     Creates a new lite engine from base64-encoded public key
    /// </summary>
    public LiteSingleEngine(string host, int port, string serverPublicKeyBase64, int reconnectTimeoutMs = 10000)
        : this(host, port, Convert.FromBase64String(serverPublicKeyBase64), reconnectTimeoutMs)
    {
    }

    public bool IsReady
    {
        get
        {
            lock (stateLock)
            {
                return isReady;
            }
        }
    }

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
    ///     Executes a query using a generated request class
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

        // Wait for connection to be ready (with timeout)
        await WaitForReadyAsync(timeout, cancellationToken);

        // Generate random query ID
        byte[] queryId = AdnlKeys.GenerateRandomBytes(32);
        string queryIdHex = Convert.ToHexString(queryId);

        // Build the request (request already includes constructor ID)
        TLWriteBuffer requestBuffer = new();
        request.WriteTo(requestBuffer);
        byte[] requestData = requestBuffer.Build();

        // Wrap in liteServer.query
        TLWriteBuffer liteServerQueryBuffer = new();
        liteServerQueryBuffer.WriteUInt32(0x798C06DF); // liteServer.query
        liteServerQueryBuffer.WriteBuffer(requestData);
        byte[] liteServerQuery = liteServerQueryBuffer.Build();

        // Wrap in adnl.message.query
        TLWriteBuffer adnlQueryBuffer = new();
        adnlQueryBuffer.WriteUInt32(0xB48BF97A); // adnl.message.query
        adnlQueryBuffer.WriteInt256(new BigInteger(queryId));
        adnlQueryBuffer.WriteBuffer(liteServerQuery);
        byte[] finalQuery = adnlQueryBuffer.Build();

        // Create completion source (using object to avoid generic type issues with dynamic storage)
        TaskCompletionSource<object> tcs = new();

        // Store the response reader with proper function ID extraction
        // We'll extract the constructor ID from the actual response
        PendingQuery query = new()
        {
            FunctionId = 0, // Not needed anymore - we match by query ID
            ResponseReader = responseReader,
            CompletionSource = tcs,
            Timeout = timeout,
            QueryData = finalQuery
        };

        pendingQueries[queryIdHex] = query;

        // Setup timeout
        using CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(timeout);

        cts.Token.Register(() =>
        {
            if (pendingQueries.TryRemove(queryIdHex, out PendingQuery? q))
                q.CompletionSource.TrySetException(new TimeoutException($"Query timed out after {timeout}ms"));
        });

        // Send query
        lock (stateLock)
        {
            if (isReady && client != null)
                _ = client.WriteAsync(finalQuery, cts.Token).ContinueWith(t =>
                {
                    if (t.IsFaulted && pendingQueries.TryRemove(queryIdHex, out PendingQuery? q))
                        q.CompletionSource.TrySetException(t.Exception?.InnerException ??
                                                           new Exception("Failed to send query"));
                }, cts.Token);
        }

        // Wait for response
        object result = await tcs.Task;
        return (TResponse)result;
    }

    public async Task CloseAsync()
    {
        AdnlClient? clientToClose;

        lock (stateLock)
        {
            if (isClosed)
                return;

            isClosed = true;
            isReady = false;
            clientToClose = client;
            client = null;
        }

        // Cancel all pending queries
        foreach (PendingQuery query in pendingQueries.Values)
            query.CompletionSource.TrySetException(new OperationCanceledException("Engine closed"));
        pendingQueries.Clear();

        if (clientToClose != null) await clientToClose.CloseAsync();
    }

    public void Dispose()
    {
        CloseAsync().GetAwaiter().GetResult();
    }

    void Connect()
    {
        lock (stateLock)
        {
            if (!isClosed)
                return;

            isClosed = false;
        }

        // Create new client
        AdnlClient newClient = new(host, port, serverPublicKey);

        // Subscribe to events
        newClient.Connected += OnClientConnected;
        newClient.Ready += OnClientReady;
        newClient.Closed += OnClientClosed;
        newClient.Error += OnClientError;
        newClient.DataReceived += OnClientDataReceived;

        lock (stateLock)
        {
            client = newClient;
        }

        // Start connection (don't await - let it run async)
        _ = newClient.ConnectAsync().ContinueWith(t =>
        {
            if (t.IsFaulted)
                Error?.Invoke(this, t.Exception?.InnerException ?? t.Exception ?? new Exception("Connection failed"));
        });
    }

    void OnClientConnected()
    {
        Connected?.Invoke(this, EventArgs.Empty);
    }

    void OnClientReady()
    {
        AdnlClient? currentClient;
        lock (stateLock)
        {
            isReady = true;
            currentClient = client;
        }

        Ready?.Invoke(this, EventArgs.Empty);

        // Resend all pending queries after reconnection
        if (currentClient != null)
            foreach (KeyValuePair<string, PendingQuery> kvp in pendingQueries)
            {
                PendingQuery query = kvp.Value;
                _ = currentClient.WriteAsync(query.QueryData).ContinueWith(t =>
                {
                    if (t.IsFaulted && pendingQueries.TryRemove(kvp.Key, out PendingQuery? q))
                        q.CompletionSource.TrySetException(t.Exception?.InnerException ??
                                                           new Exception("Failed to resend query after reconnection"));
                });
            }
    }

    void OnClientClosed()
    {
        bool shouldReconnect;
        lock (stateLock)
        {
            shouldReconnect = !isClosed;
            isReady = false;
            client = null;
        }

        Closed?.Invoke(this, EventArgs.Empty);

        // Schedule reconnection
        if (shouldReconnect)
            _ = Task.Delay(reconnectTimeoutMs).ContinueWith(_ =>
            {
                lock (stateLock)
                {
                    if (!isClosed) Connect();
                }
            });
    }

    void OnClientError(Exception error)
    {
        Error?.Invoke(this, error);
    }

    void OnClientDataReceived(byte[] data)
    {
        try
        {
            TLReadBuffer reader = new(data);
            uint messageType = reader.ReadUInt32();

            if (messageType == 0xDC69FB03) // tcp.pong
                // Ignore heartbeat pongs
                return;

            if (messageType == 0x0FAC8416) // adnl.message.answer
            {
                // Read query ID
                byte[] queryId = reader.ReadBytes(32);
                string queryIdHex = Convert.ToHexString(queryId);

                // Read lite server response
                byte[] liteServerResponse = reader.ReadBuffer();

                // Find pending query
                if (!pendingQueries.TryRemove(queryIdHex, out PendingQuery? query))
                    // Unknown query ID - possibly already timed out
                    return;

                // Parse lite server response
                TLReadBuffer liteReader = new(liteServerResponse);
                uint constructorId = liteReader.ReadUInt32();

                // Check for liteServer.error
                if (constructorId == LiteServerError.Constructor)
                {
                    LiteServerError? error = LiteServerError.ReadFrom(liteReader);
                    query.CompletionSource.TrySetException(
                        new LiteServerException(error.Code, error.Message));
                    return;
                }

                // Deserialize response
                try
                {
                    // Reset position to start (including constructor)
                    liteReader = new TLReadBuffer(liteServerResponse);
                    constructorId = liteReader.ReadUInt32(); // Read constructor again

                    object? response = query.ResponseReader.DynamicInvoke(liteReader);
                    query.CompletionSource.TrySetResult(response!);
                }
                catch (Exception ex)
                {
                    string errorDetails = $"Failed to deserialize response with constructor 0x{constructorId:X8}. " +
                                          $"Inner exception: {ex.Message}";
                    if (ex.InnerException != null)
                        errorDetails += $" | Inner: {ex.InnerException.Message}";

                    query.CompletionSource.TrySetException(
                        new InvalidOperationException(errorDetails, ex));
                }
            }
        }
        catch (Exception ex)
        {
            Error?.Invoke(this, new InvalidOperationException("Failed to process received data", ex));
        }
    }

    /// <summary>
    ///     Waits for the engine to be ready for queries
    /// </summary>
    async Task WaitForReadyAsync(int timeout, CancellationToken cancellationToken)
    {
        // If already ready, return immediately
        lock (stateLock)
        {
            if (isReady)
                return;

            if (isClosed)
                throw new InvalidOperationException("Engine is closed");
        }

        // Wait for ready event
        TaskCompletionSource readyTask = new();

        EventHandler? readyHandler = null;
        EventHandler<Exception>? errorHandler = null;

        readyHandler = (s, e) => readyTask.TrySetResult();
        errorHandler = (s, ex) => readyTask.TrySetException(ex);

        Ready += readyHandler;
        Error += errorHandler;

        try
        {
            // Check again after subscribing (race condition prevention)
            lock (stateLock)
            {
                if (isReady)
                {
                    readyTask.TrySetResult();
                    return;
                }
            }

            using CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(timeout);

            Task delayTask = Task.Delay(-1, cts.Token);
            Task completedTask = await Task.WhenAny(readyTask.Task, delayTask);

            if (completedTask == delayTask)
                throw new TimeoutException($"Connection not ready within {timeout}ms");

            // Re-throw any exception from the ready task
            await readyTask.Task;
        }
        finally
        {
            Ready -= readyHandler;
            Error -= errorHandler;
        }
    }

    sealed class PendingQuery
    {
        public required uint FunctionId { get; init; }
        public required Delegate ResponseReader { get; init; }
        public required TaskCompletionSource<object> CompletionSource { get; init; }
        public required int Timeout { get; init; }
        public required byte[] QueryData { get; init; }
    }
}

/// <summary>
///     Exception thrown when lite server returns an error
/// </summary>
public sealed class LiteServerException(int errorCode, string message)
    : Exception($"LiteServer error {errorCode}: {message}")
{
    public int ErrorCode { get; } = errorCode;
}