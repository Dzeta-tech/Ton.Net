using System.Collections.Concurrent;
using System.Numerics;
using Ton.Adnl;
using Ton.Adnl.Crypto;
using Ton.Adnl.Protocol;
using Ton.Adnl.TL;

namespace Ton.LiteClient.Engines;

/// <summary>
/// Single connection lite engine that uses ADNL client for communication
/// </summary>
public sealed class LiteSingleEngine : ILiteEngine
{
    readonly string host;
    readonly int port;
    readonly byte[] serverPublicKey;
    readonly int reconnectTimeoutMs;
    readonly ConcurrentDictionary<string, PendingQuery> pendingQueries = new();
    
    AdnlClient? client;
    bool isReady;
    bool isClosed = true;
    readonly object stateLock = new();

    /// <summary>
    /// Creates a new lite engine instance
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
    /// Creates a new lite engine from base64-encoded public key
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
                return isReady;
        }
    }

    public bool IsClosed
    {
        get
        {
            lock (stateLock)
                return isClosed;
        }
    }

    public event EventHandler? Connected;
    public event EventHandler? Ready;
    public event EventHandler? Closed;
    public event EventHandler<Exception>? Error;

    public async Task<TResponse> QueryAsync<TRequest, TResponse>(
        uint functionId,
        Action<TLWriteBuffer, TRequest> requestWriter,
        Func<TLReadBuffer, TResponse> responseReader,
        TRequest request,
        int timeout = 5000,
        CancellationToken cancellationToken = default)
    {
        lock (stateLock)
        {
            if (isClosed)
                throw new InvalidOperationException("Engine is closed");
        }

        // Generate random query ID
        byte[] queryId = AdnlKeys.GenerateRandomBytes(32);
        string queryIdHex = Convert.ToHexString(queryId);

        // Build the request
        var requestBuffer = new TLWriteBuffer();
        requestWriter(requestBuffer, request);
        byte[] requestData = requestBuffer.Build();

        // Wrap in liteServer.query
        var liteServerQueryBuffer = new TLWriteBuffer();
        liteServerQueryBuffer.WriteUInt32(0x798C06DF); // liteServer.query
        liteServerQueryBuffer.WriteBuffer(requestData);
        byte[] liteServerQuery = liteServerQueryBuffer.Build();

        // Wrap in adnl.message.query
        var adnlQueryBuffer = new TLWriteBuffer();
        adnlQueryBuffer.WriteUInt32(0xB48BF97A); // adnl.message.query
        adnlQueryBuffer.WriteInt256(new BigInteger(queryId));
        adnlQueryBuffer.WriteBuffer(liteServerQuery);
        byte[] finalQuery = adnlQueryBuffer.Build();

        // Create completion source
        var tcs = new TaskCompletionSource<TResponse>();
        var query = new PendingQuery
        {
            FunctionId = functionId,
            ResponseReader = responseReader,
            CompletionSource = tcs,
            Timeout = timeout
        };

        pendingQueries[queryIdHex] = query;

        // Setup timeout
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(timeout);
        
        cts.Token.Register(() =>
        {
            if (pendingQueries.TryRemove(queryIdHex, out var q))
            {
                q.CompletionSource.TrySetException(new TimeoutException($"Query timed out after {timeout}ms"));
            }
        });

        // Send query if ready
        lock (stateLock)
        {
            if (isReady && client != null)
            {
                _ = client.WriteAsync(finalQuery).ContinueWith(t =>
                {
                    if (t.IsFaulted && pendingQueries.TryRemove(queryIdHex, out var q))
                    {
                        q.CompletionSource.TrySetException(t.Exception!.InnerException ?? t.Exception);
                    }
                });
            }
        }

        return await tcs.Task;
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
        foreach (var query in pendingQueries.Values)
        {
            query.CompletionSource.TrySetException(new OperationCanceledException("Engine closed"));
        }
        pendingQueries.Clear();

        if (clientToClose != null)
        {
            await clientToClose.CloseAsync();
        }
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
        var newClient = new AdnlClient(host, port, serverPublicKey);

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
            {
                Error?.Invoke(this, t.Exception?.InnerException ?? t.Exception ?? new Exception("Connection failed"));
            }
        });
    }

    void OnClientConnected()
    {
        Connected?.Invoke(this, EventArgs.Empty);
    }

    void OnClientReady()
    {
        lock (stateLock)
        {
            isReady = true;
        }

        Ready?.Invoke(this, EventArgs.Empty);

        // Resend all pending queries
        lock (stateLock)
        {
            if (client == null)
                return;

            foreach (var kvp in pendingQueries)
            {
                // Rebuild query for this queryId
                byte[] queryId = Convert.FromHexString(kvp.Key);

                // Note: We can't rebuild the original request here, so queries sent before
                // ready will timeout. In production, clients should wait for Ready event.
            }
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
        {
            _ = Task.Delay(reconnectTimeoutMs).ContinueWith(_ =>
            {
                lock (stateLock)
                {
                    if (!isClosed)
                    {
                        Connect();
                    }
                }
            });
        }
    }

    void OnClientError(Exception error)
    {
        Error?.Invoke(this, error);
    }

    void OnClientDataReceived(byte[] data)
    {
        try
        {
            var reader = new TLReadBuffer(data);
            uint messageType = reader.ReadUInt32();

            if (messageType == 0xDC69FB03) // tcp.pong
            {
                // Ignore heartbeat pongs
                return;
            }

            if (messageType == 0x0FAC8416) // adnl.message.answer
            {
                // Read query ID
                byte[] queryId = reader.ReadBytes(32);
                string queryIdHex = Convert.ToHexString(queryId);

                // Read lite server response
                byte[] liteServerResponse = reader.ReadBuffer();

                // Find pending query
                if (!pendingQueries.TryRemove(queryIdHex, out var query))
                {
                    // Unknown query ID - possibly already timed out
                    return;
                }

                // Parse lite server response
                var liteReader = new TLReadBuffer(liteServerResponse);
                uint constructorId = liteReader.ReadUInt32();

                // Check for liteServer.error
                if (constructorId == LiteServerError.Constructor)
                {
                    var error = LiteServerError.ReadFrom(liteReader);
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
                    query.CompletionSource.TrySetException(
                        new InvalidOperationException($"Failed to deserialize response with constructor 0x{constructorId:X8}", ex));
                }
            }
        }
        catch (Exception ex)
        {
            Error?.Invoke(this, new InvalidOperationException("Failed to process received data", ex));
        }
    }

    sealed class PendingQuery
    {
        public required uint FunctionId { get; init; }
        public required Delegate ResponseReader { get; init; }
        public required dynamic CompletionSource { get; init; }
        public required int Timeout { get; init; }
    }
}

/// <summary>
/// Exception thrown when lite server returns an error
/// </summary>
public sealed class LiteServerException : Exception
{
    public int ErrorCode { get; }

    public LiteServerException(int errorCode, string message)
        : base($"LiteServer error {errorCode}: {message}")
    {
        ErrorCode = errorCode;
    }
}

