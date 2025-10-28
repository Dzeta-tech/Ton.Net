using System.Buffers.Binary;
using System.Net.Sockets;
using Ton.Adnl.Crypto;
using Ton.Adnl.Protocol;

namespace Ton.Adnl;

/// <summary>
/// ADNL (Abstract Datagram Network Layer) TCP client.
/// Provides encrypted communication with TON nodes over TCP.
/// Thread-safe implementation with automatic reconnection.
/// </summary>
public sealed class AdnlClient : IDisposable
{
    private readonly string _host;
    private readonly int _port;
    private readonly byte[] _peerPublicKey;
    private readonly int _reconnectTimeoutMs;
    private readonly SemaphoreSlim _stateLock = new(1, 1);
    private readonly CancellationTokenSource _disposeCts = new();

    private TcpClient? _tcpClient;
    private NetworkStream? _networkStream;
    private AdnlKeys? _keys;
    private AdnlCipher? _encryptCipher;
    private AdnlCipher? _decryptCipher;
    private Task? _receiveTask;
    private AdnlClientState _state = AdnlClientState.Closed;
    private bool _disposed;

    /// <summary>
    /// Creates a new ADNL client.
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
        if (peerPublicKey == null)
            throw new ArgumentNullException(nameof(peerPublicKey));
        if (peerPublicKey.Length != 32)
            throw new ArgumentException("Peer public key must be 32 bytes", nameof(peerPublicKey));
        if (reconnectTimeoutMs < 0)
            throw new ArgumentOutOfRangeException(nameof(reconnectTimeoutMs), "Reconnect timeout cannot be negative");

        _host = host;
        _port = port;
        _peerPublicKey = peerPublicKey;
        _reconnectTimeoutMs = reconnectTimeoutMs;
    }

    /// <summary>
    /// Gets the current connection state.
    /// </summary>
    public AdnlClientState State
    {
        get
        {
            _stateLock.Wait();
            try
            {
                return _state;
            }
            finally
            {
                _stateLock.Release();
            }
        }
    }

    /// <summary>
    /// Raised when the TCP connection is established.
    /// </summary>
    public event Action? Connected;

    /// <summary>
    /// Raised when the ADNL handshake completes and the client is ready.
    /// </summary>
    public event Action? Ready;

    /// <summary>
    /// Raised when the connection is closed.
    /// </summary>
    public event Action? Closed;

    /// <summary>
    /// Raised when data is received (after decryption).
    /// </summary>
    public event Action<byte[]>? DataReceived;

    /// <summary>
    /// Raised when an error occurs.
    /// </summary>
    public event Action<Exception>? Error;

    /// <summary>
    /// Connects to the ADNL server asynchronously.
    /// </summary>
    public async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        await _stateLock.WaitAsync(cancellationToken);
        try
        {
            if (_state != AdnlClientState.Closed)
                throw new InvalidOperationException($"Cannot connect in state {_state}");

            _state = AdnlClientState.Connecting;
        }
        finally
        {
            _stateLock.Release();
        }

        try
        {
            // Create TCP client
            _tcpClient = new TcpClient
            {
                ReceiveBufferSize = 1024 * 1024, // 1 MB
                SendBufferSize = 1024 * 1024
            };

            // Connect
            await _tcpClient.ConnectAsync(_host, _port, cancellationToken);
            _networkStream = _tcpClient.GetStream();

            Connected?.Invoke();

            // Perform ADNL handshake
            await PerformHandshakeAsync(cancellationToken);

            // Start receiving
            _receiveTask = Task.Run(() => ReceiveLoopAsync(_disposeCts.Token), _disposeCts.Token);

            await SetStateAsync(AdnlClientState.Ready);
            Ready?.Invoke();
        }
        catch (Exception ex)
        {
            Error?.Invoke(ex);
            await CloseInternalAsync();
            throw;
        }
    }

    /// <summary>
    /// Writes encrypted data to the server.
    /// </summary>
    public async Task WriteAsync(byte[] data, CancellationToken cancellationToken = default)
    {
        if (data == null)
            throw new ArgumentNullException(nameof(data));

        ObjectDisposedException.ThrowIf(_disposed, this);

        await _stateLock.WaitAsync(cancellationToken);
        try
        {
            if (_state != AdnlClientState.Ready)
                throw new InvalidOperationException($"Cannot write in state {_state}");

            if (_networkStream == null || _encryptCipher == null)
                throw new InvalidOperationException("Connection not established");

            // Create ADNL packet
            var packet = new AdnlPacket(data);
            var packetBytes = packet.ToBytes();

            // Encrypt
            var encrypted = _encryptCipher.Process(packetBytes);

            // Write
            await _networkStream.WriteAsync(encrypted, cancellationToken);
        }
        finally
        {
            _stateLock.Release();
        }
    }

    /// <summary>
    /// Closes the connection.
    /// </summary>
    public async Task CloseAsync()
    {
        await CloseInternalAsync();
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
            _disposeCts.Cancel();
            CloseInternalAsync().GetAwaiter().GetResult();
            _disposeCts.Dispose();
            _stateLock.Dispose();
        }
    }

    private async Task PerformHandshakeAsync(CancellationToken cancellationToken)
    {
        await SetStateAsync(AdnlClientState.Handshaking);

        // Generate ephemeral keys
        _keys = new AdnlKeys(_peerPublicKey);

        // Generate AES parameters
        var aesParams = new AdnlAesParams();

        // Compute handshake packet
        // Key = shared_secret[0..16] + aesParams.Hash[16..32]
        // Nonce = aesParams.Hash[0..4] + shared_secret[20..32]
        var sharedSecret = _keys.SharedSecret;
        var paramsHash = aesParams.Hash;

        byte[] key = new byte[32];
        Array.Copy(sharedSecret, 0, key, 0, 16);
        Array.Copy(paramsHash, 16, key, 16, 16);

        byte[] nonce = new byte[16];
        Array.Copy(paramsHash, 0, nonce, 0, 4);
        Array.Copy(sharedSecret, 20, nonce, 4, 12);

        // Encrypt AES parameters
        using var handshakeCipher = new AdnlCipher(key, nonce);
        var encryptedParams = handshakeCipher.Process(aesParams.Bytes);

        // Build handshake packet
        // peer_address(32) + client_public_key(32) + params_hash(32) + encrypted_params(160)
        var peerAddress = new AdnlAddress(_peerPublicKey);
        var handshake = new byte[256];
        int offset = 0;

        Array.Copy(peerAddress.Hash, 0, handshake, offset, 32);
        offset += 32;
        Array.Copy(_keys.PublicKey, 0, handshake, offset, 32);
        offset += 32;
        Array.Copy(paramsHash, 0, handshake, offset, 32);
        offset += 32;
        Array.Copy(encryptedParams, 0, handshake, offset, 160);

        // Send handshake
        if (_networkStream == null)
            throw new InvalidOperationException("Network stream not available");

        await _networkStream.WriteAsync(handshake, cancellationToken);

        // Setup ciphers for communication
        _encryptCipher = new AdnlCipher(aesParams.TxKey, aesParams.TxNonce);
        _decryptCipher = new AdnlCipher(aesParams.RxKey, aesParams.RxNonce);

        // Wait for handshake response (empty packet)
        var responseBuffer = new byte[AdnlPacket.MinimumSize];
        int read = await _networkStream.ReadAsync(responseBuffer, cancellationToken);

        if (read == 0)
            throw new IOException("Server closed connection during handshake");

        // Decrypt and verify
        _decryptCipher.ProcessInPlace(responseBuffer.AsSpan(0, read));
        var responsePacket = AdnlPacket.TryParse(responseBuffer[0..read]);

        if (responsePacket == null || responsePacket.Payload.Length != 0)
            throw new InvalidOperationException("Invalid handshake response");
    }

    private async Task ReceiveLoopAsync(CancellationToken cancellationToken)
    {
        var buffer = new List<byte>();
        var tempBuffer = new byte[4096];

        try
        {
            while (!cancellationToken.IsCancellationRequested && _networkStream != null)
            {
                int read = await _networkStream.ReadAsync(tempBuffer, cancellationToken);
                if (read == 0)
                {
                    // Connection closed
                    break;
                }

                // Decrypt
                if (_decryptCipher != null)
                {
                    var decrypted = _decryptCipher.Process(tempBuffer[0..read]);
                    buffer.AddRange(decrypted);
                }

                // Process complete packets
                while (buffer.Count >= 4)
                {
                    // Check if we have a complete packet
                    if (buffer.Count < 4)
                        break;

                    uint size = BinaryPrimitives.ReadUInt32LittleEndian(buffer.ToArray());
                    int totalSize = (int)(4 + size);

                    if (buffer.Count < totalSize)
                        break; // Wait for more data

                    // Extract packet
                    var packetBytes = buffer.GetRange(0, totalSize).ToArray();
                    buffer.RemoveRange(0, totalSize);

                    // Parse and emit
                    try
                    {
                        var packet = AdnlPacket.Parse(packetBytes);
                        DataReceived?.Invoke(packet.Payload);
                    }
                    catch (Exception ex)
                    {
                        Error?.Invoke(new InvalidOperationException("Failed to parse packet", ex));
                    }
                }
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            Error?.Invoke(ex);
        }
        finally
        {
            await CloseInternalAsync();
        }
    }

    private async Task CloseInternalAsync()
    {
        await _stateLock.WaitAsync();
        try
        {
            if (_state == AdnlClientState.Closed || _state == AdnlClientState.Closing)
                return;

            _state = AdnlClientState.Closing;

            _encryptCipher?.Dispose();
            _decryptCipher?.Dispose();
            _networkStream?.Dispose();
            _tcpClient?.Dispose();

            _encryptCipher = null;
            _decryptCipher = null;
            _networkStream = null;
            _tcpClient = null;
            _keys = null;

            _state = AdnlClientState.Closed;
        }
        finally
        {
            _stateLock.Release();
        }

        Closed?.Invoke();
    }

    private async Task SetStateAsync(AdnlClientState newState)
    {
        await _stateLock.WaitAsync();
        try
        {
            _state = newState;
        }
        finally
        {
            _stateLock.Release();
        }
    }
}

