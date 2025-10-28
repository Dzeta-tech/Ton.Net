using Ton.Crypto.Primitives;

namespace Ton.Adnl.Crypto;

/// <summary>
/// ADNL AES encryption parameters used during handshake.
/// Contains cryptographically random data for establishing secure communication.
/// </summary>
public sealed class AdnlAesParams
{
    /// <summary>
    /// Creates new random AES parameters for ADNL handshake.
    /// </summary>
    public AdnlAesParams()
    {
        // Generate 160 bytes of random data
        Bytes = AdnlKeys.GenerateRandomBytes(160);
        Hash = Sha256.Hash(Bytes);
    }

    /// <summary>
    /// Gets the raw parameter bytes (160 bytes).
    /// </summary>
    public byte[] Bytes { get; }

    /// <summary>
    /// Gets the SHA-256 hash of the parameter bytes (32 bytes).
    /// </summary>
    public byte[] Hash { get; }

    /// <summary>
    /// Gets the transmission key (32 bytes) derived from the parameters.
    /// Used for encrypting outgoing data.
    /// </summary>
    public byte[] TxKey
    {
        get
        {
            byte[] key = new byte[32];
            Array.Copy(Bytes, 0, key, 0, 32);
            return key;
        }
    }

    /// <summary>
    /// Gets the transmission nonce/IV (16 bytes) derived from the parameters.
    /// Used as initial counter for encrypting outgoing data.
    /// </summary>
    public byte[] TxNonce
    {
        get
        {
            byte[] nonce = new byte[16];
            Array.Copy(Bytes, 32, nonce, 0, 16);
            return nonce;
        }
    }

    /// <summary>
    /// Gets the reception key (32 bytes) derived from the parameters.
    /// Used for decrypting incoming data.
    /// </summary>
    public byte[] RxKey
    {
        get
        {
            byte[] key = new byte[32];
            Array.Copy(Bytes, 64, key, 0, 32);
            return key;
        }
    }

    /// <summary>
    /// Gets the reception nonce/IV (16 bytes) derived from the parameters.
    /// Used as initial counter for decrypting incoming data.
    /// </summary>
    public byte[] RxNonce
    {
        get
        {
            byte[] nonce = new byte[16];
            Array.Copy(Bytes, 96, nonce, 0, 16);
            return nonce;
        }
    }
}

