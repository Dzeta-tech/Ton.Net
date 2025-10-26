using System.Numerics;

namespace Ton.Core.Dict;

/// <summary>
///     Supported dictionary key types in TON blockchain.
/// </summary>
public interface IDictionaryKeyType
{
}

/// <summary>
///     Dictionary key serializer interface.
/// </summary>
/// <typeparam name="TK">Key type (must implement IDictionaryKeyType).</typeparam>
public interface IDictionaryKey<TK> where TK : IDictionaryKeyType
{
    /// <summary>
    ///     Number of bits for the key.
    /// </summary>
    int Bits { get; }

    /// <summary>
    ///     Serialize key to bigint representation.
    /// </summary>
    BigInteger Serialize(TK key);

    /// <summary>
    ///     Parse key from bigint representation.
    /// </summary>
    TK Parse(BigInteger value);
}