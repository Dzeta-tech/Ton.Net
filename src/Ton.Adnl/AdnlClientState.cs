namespace Ton.Adnl;

/// <summary>
/// Represents the state of an ADNL client connection.
/// </summary>
public enum AdnlClientState
{
    /// <summary>
    /// Client is disconnected and not attempting to connect.
    /// </summary>
    Closed,

    /// <summary>
    /// Client is establishing a TCP connection.
    /// </summary>
    Connecting,

    /// <summary>
    /// TCP connection established, performing ADNL handshake.
    /// </summary>
    Handshaking,

    /// <summary>
    /// ADNL handshake complete, connection ready for queries.
    /// </summary>
    Ready,

    /// <summary>
    /// Connection is being closed.
    /// </summary>
    Closing
}

