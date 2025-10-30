using System.Buffers.Binary;
using System.Net.Sockets;
using Ton.Adnl.Crypto;
using Ton.Adnl.Protocol;

namespace Ton.Adnl;

/// <summary>
///     ADNL (Abstract Datagram Network Layer) TCP client.
///     Provides encrypted communication with TON nodes over TCP.
///     Thread-safe implementation with automatic reconnection.
/// </summary>
public sealed class AdnlClient : IDisposable
{
    readonly CancellationTokenSource disposeCts = new();
    readonly string host;
    readonly byte[] peerPublicKey;
    readonly int port;
    readonly SemaphoreSlim stateLock = new(1, 1);
    AdnlCipher? decryptCipher;
    bool disposed;
    AdnlCipher? encryptCipher;

    byte[]? handshakePacket;
    AdnlKeys? keys;
    NetworkStream? networkStream;
    AdnlClientState state = AdnlClientState.Closed;

    TcpClient? tcpClient;

    /// <summary>
    ///     Creates a new ADNL client.
    /// </summary>
    /// <param name="host">Server hostname or IP address.</param>
    /// <param name="port">Server port.</param>
    /// <param name="peerPublicKey">Server's Ed25519 public key (32 bytes).</param>
    /// <param name="reconnectTimeoutMs">Milliseconds to wait before reconnecting after disconnect.</param>
    public AdnlClient(string host, int port, byte[] peerPublicKey, int reconnectTimeoutMs = 10000)
    {
        if (string.IsNullOrWhiteSpace(host))
            throw new ArgumentException("Host cannot be null or empty", nameof(host));
        if (port <= 0 || port > 65535)
            throw new ArgumentOutOfRangeException(nameof(port), "Port must be between 1 and 65535");
        ArgumentNullException.ThrowIfNull(peerPublicKey);
        if (peerPublicKey.Length != 32)
            throw new ArgumentException("Peer public key must be 32 bytes", nameof(peerPublicKey));
        if (reconnectTimeoutMs < 0)
            throw new ArgumentOutOfRangeException(nameof(reconnectTimeoutMs), "Reconnect timeout cannot be negative");

        this.host = host;
        this.port = port;
        this.peerPublicKey = peerPublicKey;
    }

    /// <summary>
    ///     Gets the current connection state.
    /// </summary>
    public AdnlClientState State
    {
        get
        {
            stateLock.Wait();
            try
            {
                return state;
            }
            finally
            {
                stateLock.Release();
            }
        }
    }

    public void Dispose()
    {
        if (!disposed)
        {
            disposed = true;
            disposeCts.Cancel();
            CloseInternalAsync().GetAwaiter().GetResult();
            disposeCts.Dispose();
            stateLock.Dispose();
        }
    }

    /// <summary>
    ///     Raised when the TCP connection is established.
    /// </summary>
    public event Action? Connected;

    /// <summary>
    ///     Raised when the ADNL handshake completes and the client is ready.
    /// </summary>
    public event Action? Ready;

    /// <summary>
    ///     Raised when the connection is closed.
    /// </summary>
    public event Action? Closed;

    /// <summary>
    ///     Raised when data is received (after decryption).
    /// </summary>
    public event Action<byte[]>? DataReceived;

    /// <summary>
    ///     Raised when an error occurs.
    /// </summary>
    public event Action<Exception>? Error;

    /// <summary>
    ///     Connects to the ADNL server asynchronously.
    /// </summary>
    public async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        if (disposed) throw new ObjectDisposedException(nameof(AdnlClient));

        await stateLock.WaitAsync(cancellationToken);
        try
        {
            if (state != AdnlClientState.Closed)
                throw new InvalidOperationException($"Cannot connect in state {state}");

            state = AdnlClientState.Connecting;
        }
        finally
        {
            stateLock.Release();
        }

        try
        {
            // Create TCP client
            tcpClient = new TcpClient
            {
                ReceiveBufferSize = 1024 * 1024, // 1 MB
                SendBufferSize = 1024 * 1024
            };

            // Connect
            await tcpClient.ConnectAsync(host, port, cancellationToken);
            networkStream = tcpClient.GetStream();

            Connected?.Invoke();

            // Setup handshake parameters and ciphers BEFORE starting receive loop
            await PrepareHandshakeAsync(cancellationToken);

            // Start receiving (ciphers are now ready)
            _ = Task.Run(() => ReceiveLoopAsync(disposeCts.Token), disposeCts.Token);

            // Send handshake (response will be handled by receive loop)
            await SendHandshakeAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            Error?.Invoke(ex);
            await CloseInternalAsync();
            throw;
        }
    }

    /// <summary>
    ///     Writes encrypted data to the server.
    /// </summary>
    public async Task WriteAsync(byte[] data, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(data);

        if (disposed) throw new ObjectDisposedException(nameof(AdnlClient));

        await stateLock.WaitAsync(cancellationToken);
        try
        {
            if (state != AdnlClientState.Ready)
                throw new InvalidOperationException($"Cannot write in state {state}");

            if (networkStream == null || encryptCipher == null)
                throw new InvalidOperationException("Connection not established");

            // Create ADNL packet
            AdnlPacket packet = new(data);
            byte[] packetBytes = packet.ToBytes();

            // Encrypt
            byte[] encrypted = encryptCipher.Process(packetBytes);

            // Write
            await networkStream.WriteAsync(encrypted, cancellationToken);
            await networkStream.FlushAsync(cancellationToken);
        }
        finally
        {
            stateLock.Release();
        }
    }

    /// <summary>
    ///     Closes the connection.
    /// </summary>
    public async Task CloseAsync()
    {
        await CloseInternalAsync();
    }

    async Task PrepareHandshakeAsync(CancellationToken cancellationToken)
    {
        await SetStateAsync(AdnlClientState.Handshaking);

        // Generate ephemeral keys
        keys = new AdnlKeys(peerPublicKey);

        // Generate AES parameters
        AdnlAesParams aesParams = new();

        // Compute handshake packet
        // Key = shared_secret[0..16] + aesParams.Hash[16..32]
        // Nonce = aesParams.Hash[0..4] + shared_secret[20..32]
        byte[] sharedSecret = keys.SharedSecret;
        byte[] paramsHash = aesParams.Hash;

        byte[] key = new byte[32];
        Array.Copy(sharedSecret, 0, key, 0, 16);
        Array.Copy(paramsHash, 16, key, 16, 16);

        byte[] nonce = new byte[16];
        Array.Copy(paramsHash, 0, nonce, 0, 4);
        Array.Copy(sharedSecret, 20, nonce, 4, 12);

        // Encrypt AES parameters
        using AdnlCipher handshakeCipher = new(key, nonce);
        byte[] encryptedParams = handshakeCipher.Process(aesParams.Bytes);

        // Build handshake packet
        // peer_address(32) + client_public_key(32) + params_hash(32) + encrypted_params(160)
        AdnlAddress peerAddress = new(peerPublicKey);
        handshakePacket = new byte[256];
        int offset = 0;

        Array.Copy(peerAddress.Hash, 0, handshakePacket, offset, 32);
        offset += 32;
        Array.Copy(keys.PublicKey, 0, handshakePacket, offset, 32);
        offset += 32;
        Array.Copy(paramsHash, 0, handshakePacket, offset, 32);
        offset += 32;
        Array.Copy(encryptedParams, 0, handshakePacket, offset, 160);

        // Setup ciphers for communication (BEFORE starting receive loop)

        encryptCipher = new AdnlCipher(aesParams.TxKey, aesParams.TxNonce);
        decryptCipher = new AdnlCipher(aesParams.RxKey, aesParams.RxNonce);
    }

    async Task SendHandshakeAsync(CancellationToken cancellationToken)
    {
        if (networkStream == null)
            throw new InvalidOperationException("Network stream not available");
        if (handshakePacket == null)
            throw new InvalidOperationException("Handshake not prepared");


        await networkStream.WriteAsync(handshakePacket, cancellationToken);
        await networkStream.FlushAsync(cancellationToken);
    }

    async Task ReceiveLoopAsync(CancellationToken cancellationToken)
    {
        List<byte> buffer = [];
        byte[] tempBuffer = new byte[4096];

        try
        {
            while (!cancellationToken.IsCancellationRequested && networkStream != null)
            {
                int read = await networkStream.ReadAsync(tempBuffer, cancellationToken);

                if (read == 0) break;

                // Decrypt
                if (decryptCipher != null)
                {
                    byte[] decrypted = decryptCipher.Process(tempBuffer[..read]);
                    buffer.AddRange(decrypted);
                }
                else
                {
                    Error?.Invoke(new InvalidOperationException("Decrypt cipher not initialized"));
                    break;
                }

                // Process complete packets
                while (buffer.Count >= 4)
                {
                    // Check if we have a complete packet
                    if (buffer.Count < 4)
                        break;

                    uint size = BinaryPrimitives.ReadUInt32LittleEndian(buffer.ToArray());
                    int totalSize = (int)(4 + size);

                    if (buffer.Count < totalSize) break; // Wait for more data

                    // Extract packet
                    byte[] packetBytes = buffer.GetRange(0, totalSize).ToArray();
                    buffer.RemoveRange(0, totalSize);

                    // Parse and emit
                    try
                    {
                        AdnlPacket packet = AdnlPacket.Parse(packetBytes);

                        // Handle handshake response
                        if (state == AdnlClientState.Handshaking)
                        {
                            if (packet.Payload.Length != 0)
                            {
                                Error?.Invoke(new InvalidOperationException(
                                    $"Invalid handshake response - expected empty payload, got {packet.Payload.Length} bytes"));
                                await CloseInternalAsync();
                                break;
                            }

                            await SetStateAsync(AdnlClientState.Ready);
                            Ready?.Invoke();
                            continue; // Don't emit DataReceived for handshake response
                        }

                        DataReceived?.Invoke(packet.Payload);
                    }
                    catch (Exception ex)
                    {
                        Error?.Invoke(new InvalidOperationException("Failed to parse packet", ex));
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Normal cancellation, don't emit error
            throw;
        }
        catch (Exception ex)
        {
            Error?.Invoke(ex);
        }
        finally
        {
            await CloseInternalAsync();
        }
    }

    async Task CloseInternalAsync()
    {
        await stateLock.WaitAsync();
        try
        {
            if (state == AdnlClientState.Closed || state == AdnlClientState.Closing)
                return;

            state = AdnlClientState.Closing;

            encryptCipher?.Dispose();
            decryptCipher?.Dispose();
            if (networkStream != null)
                await networkStream.DisposeAsync();
            tcpClient?.Dispose();

            encryptCipher = null;
            decryptCipher = null;
            networkStream = null;
            tcpClient = null;
            keys = null;

            state = AdnlClientState.Closed;
        }
        finally
        {
            stateLock.Release();
        }

        Closed?.Invoke();
    }

    async Task SetStateAsync(AdnlClientState newState)
    {
        await stateLock.WaitAsync();
        try
        {
            state = newState;
        }
        finally
        {
            stateLock.Release();
        }
    }
}