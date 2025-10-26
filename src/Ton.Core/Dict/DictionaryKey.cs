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
/// <typeparam name="K">Key type (must implement IDictionaryKeyType).</typeparam>
public interface IDictionaryKey<K> where K : IDictionaryKeyType
{
    /// <summary>
    ///     Number of bits for the key.
    /// </summary>
    int Bits { get; }

    /// <summary>
    ///     Serialize key to bigint representation.
    /// </summary>
    BigInteger Serialize(K key);

    /// <summary>
    ///     Parse key from bigint representation.
    /// </summary>
    K Parse(BigInteger value);
}

