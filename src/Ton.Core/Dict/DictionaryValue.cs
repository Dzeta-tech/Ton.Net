using Ton.Core.Boc;

namespace Ton.Core.Dict;

/// <summary>
///     Dictionary value serializer interface.
/// </summary>
/// <typeparam name="TV">Value type.</typeparam>
public interface IDictionaryValue<TV>
{
    /// <summary>
    ///     Serialize value to builder.
    /// </summary>
    void Serialize(TV value, Builder builder);

    /// <summary>
    ///     Parse value from slice.
    /// </summary>
    TV Parse(Slice slice);
}