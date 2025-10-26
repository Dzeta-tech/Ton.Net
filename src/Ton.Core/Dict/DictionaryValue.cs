using Ton.Core.Boc;

namespace Ton.Core.Dict;

/// <summary>
///     Dictionary value serializer interface.
/// </summary>
/// <typeparam name="V">Value type.</typeparam>
public interface IDictionaryValue<V>
{
    /// <summary>
    ///     Serialize value to builder.
    /// </summary>
    void Serialize(V value, Builder builder);

    /// <summary>
    ///     Parse value from slice.
    /// </summary>
    V Parse(Slice slice);
}

