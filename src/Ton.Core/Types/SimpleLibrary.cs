using Ton.Core.Boc;

namespace Ton.Core.Types;

/// <summary>
///     Simple library structure for contract libraries.
///     Source:
///     https://github.com/ton-blockchain/ton/blob/24dc184a2ea67f9c47042b4104bbb4d82289fac1/crypto/block/block.tlb#L145
///     TL-B: simple_lib$_ public:Bool root:^Cell = SimpleLib;
/// </summary>
public record SimpleLibrary
{
    /// <summary>
    ///     Creates a new SimpleLibrary instance.
    /// </summary>
    public SimpleLibrary(bool isPublic, Cell root)
    {
        Public = isPublic;
        Root = root;
    }

    /// <summary>
    ///     Whether this library is public (accessible to other contracts).
    /// </summary>
    public bool Public { get; init; }

    /// <summary>
    ///     Root cell of the library.
    /// </summary>
    public Cell Root { get; init; }

    /// <summary>
    ///     Loads SimpleLibrary from a slice.
    /// </summary>
    /// <param name="slice">Slice to load from.</param>
    /// <returns>Loaded SimpleLibrary.</returns>
    public static SimpleLibrary Load(Slice slice)
    {
        bool isPublic = slice.LoadBit();
        Cell root = slice.LoadRef();
        return new SimpleLibrary(isPublic, root);
    }

    /// <summary>
    ///     Stores SimpleLibrary to a builder.
    /// </summary>
    /// <param name="builder">Builder to store to.</param>
    public void Store(Builder builder)
    {
        builder.StoreBit(Public);
        builder.StoreRef(Root);
    }
}