using Ton.Adnl.TL;

namespace Ton.Adnl.Protocol;

/// <summary>
///     Interface for lite server request objects that can be serialized
/// </summary>
public interface ILiteRequest
{
    /// <summary>
    ///     Writes the request to the buffer (including constructor ID)
    /// </summary>
    void WriteTo(TLWriteBuffer writer);
}