using System.Buffers.Binary;
using Ton.Adnl.Crypto;
using Ton.Crypto.Primitives;

namespace Ton.Adnl.Protocol;

/// <summary>
/// ADNL packet structure for framing data over the wire.
/// 
/// Packet format:
/// - 4 bytes: size (little-endian, excluding the size field itself)
/// - 32 bytes: nonce (random)
/// - N bytes: payload
/// - 32 bytes: checksum (SHA-256 of nonce + payload)
/// 
/// Minimum packet size: 68 bytes (4 + 32 + 0 + 32)
/// </summary>
public sealed class AdnlPacket
{
    /// <summary>
    /// Minimum packet size in bytes (size + nonce + hash).
    /// </summary>
    public const int MinimumSize = 68; // 4 (size) + 32 (nonce) + 32 (hash)

    private readonly byte[] _nonce;
    private readonly byte[] _payload;
    private byte[]? _cachedHash;
    private byte[]? _cachedData;

    /// <summary>
    /// Creates a new ADNL packet with the specified payload.
    /// </summary>
    /// <param name="payload">The payload data to include in the packet.</param>
    /// <param name="nonce">Optional 32-byte nonce (random if not provided).</param>
    public AdnlPacket(byte[] payload, byte[]? nonce = null)
    {
        if (payload == null)
            throw new ArgumentNullException(nameof(payload));

        if (nonce != null && nonce.Length != 32)
            throw new ArgumentException("Nonce must be 32 bytes", nameof(nonce));

        _payload = payload;
        _nonce = nonce ?? AdnlKeys.GenerateRandomBytes(32);
    }

    /// <summary>
    /// Gets the packet payload.
    /// </summary>
    public byte[] Payload => _payload;

    /// <summary>
    /// Gets the packet nonce.
    /// </summary>
    public byte[] Nonce => _nonce;

    /// <summary>
    /// Gets the total packet length (including all fields).
    /// </summary>
    public int Length => 4 + 32 + _payload.Length + 32;

    /// <summary>
    /// Computes the SHA-256 hash of (nonce + payload).
    /// </summary>
    private byte[] ComputeHash()
    {
        if (_cachedHash != null)
            return _cachedHash;

        // Hash = SHA256(nonce || payload)
        byte[] combined = new byte[32 + _payload.Length];
        Array.Copy(_nonce, 0, combined, 0, 32);
        Array.Copy(_payload, 0, combined, 32, _payload.Length);

        _cachedHash = Sha256.Hash(combined);
        return _cachedHash;
    }

    /// <summary>
    /// Serializes the packet to a byte array.
    /// </summary>
    /// <returns>Serialized packet data.</returns>
    public byte[] ToBytes()
    {
        if (_cachedData != null)
            return _cachedData;

        byte[] hash = ComputeHash();
        uint size = (uint)(32 + _payload.Length + 32); // nonce + payload + hash

        byte[] data = new byte[4 + size];
        int offset = 0;

        // Write size (little-endian, 4 bytes)
        BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(offset), size);
        offset += 4;

        // Write nonce (32 bytes)
        Array.Copy(_nonce, 0, data, offset, 32);
        offset += 32;

        // Write payload
        Array.Copy(_payload, 0, data, offset, _payload.Length);
        offset += _payload.Length;

        // Write hash (32 bytes)
        Array.Copy(hash, 0, data, offset, 32);

        _cachedData = data;
        return data;
    }

    /// <summary>
    /// Attempts to parse an ADNL packet from a byte array.
    /// </summary>
    /// <param name="data">The data to parse.</param>
    /// <returns>Parsed packet, or null if data is insufficient.</returns>
    /// <exception cref="InvalidOperationException">Thrown if packet hash is invalid.</exception>
    public static AdnlPacket? TryParse(byte[] data)
    {
        if (data == null)
            throw new ArgumentNullException(nameof(data));

        // Need at least 4 bytes to read size
        if (data.Length < 4)
            return null;

        // Read size (little-endian)
        uint size = BinaryPrimitives.ReadUInt32LittleEndian(data);

        // Check if we have enough data
        if (data.Length < 4 + size)
            return null;

        // Size must be at least 64 bytes (32 nonce + 32 hash)
        if (size < 64)
            throw new InvalidOperationException($"Invalid packet size: {size} (minimum is 64)");

        int offset = 4;

        // Read nonce (32 bytes)
        byte[] nonce = new byte[32];
        Array.Copy(data, offset, nonce, 0, 32);
        offset += 32;

        // Read payload
        int payloadLength = (int)size - 64; // size - nonce - hash
        byte[] payload = new byte[payloadLength];
        Array.Copy(data, offset, payload, 0, payloadLength);
        offset += payloadLength;

        // Read hash (32 bytes)
        byte[] receivedHash = new byte[32];
        Array.Copy(data, offset, receivedHash, 0, 32);

        // Verify hash
        byte[] combined = new byte[32 + payloadLength];
        Array.Copy(nonce, 0, combined, 0, 32);
        Array.Copy(payload, 0, combined, 32, payloadLength);
        byte[] expectedHash = Sha256.Hash(combined);

        if (!receivedHash.AsSpan().SequenceEqual(expectedHash))
            throw new InvalidOperationException("Invalid packet hash: checksum verification failed");

        return new AdnlPacket(payload, nonce);
    }

    /// <summary>
    /// Parses an ADNL packet from a byte array.
    /// </summary>
    /// <param name="data">The data to parse.</param>
    /// <returns>Parsed packet.</returns>
    /// <exception cref="InvalidOperationException">Thrown if data is insufficient or hash is invalid.</exception>
    public static AdnlPacket Parse(byte[] data)
    {
        var packet = TryParse(data);
        if (packet == null)
            throw new InvalidOperationException("Insufficient data to parse ADNL packet");

        return packet;
    }

    /// <summary>
    /// Checks if the buffer contains a complete packet.
    /// </summary>
    /// <param name="buffer">The buffer to check.</param>
    /// <returns>True if buffer contains at least one complete packet.</returns>
    public static bool IsComplete(byte[] buffer)
    {
        if (buffer == null || buffer.Length < 4)
            return false;

        uint size = BinaryPrimitives.ReadUInt32LittleEndian(buffer);
        return buffer.Length >= 4 + size;
    }

    /// <summary>
    /// Gets the expected packet length from a buffer (must have at least 4 bytes).
    /// </summary>
    /// <param name="buffer">The buffer containing at least the size field.</param>
    /// <returns>Total packet length including the size field.</returns>
    public static int GetPacketLength(byte[] buffer)
    {
        if (buffer == null || buffer.Length < 4)
            throw new ArgumentException("Buffer must contain at least 4 bytes");

        uint size = BinaryPrimitives.ReadUInt32LittleEndian(buffer);
        return (int)(4 + size);
    }
}

