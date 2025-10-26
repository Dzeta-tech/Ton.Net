using Ton.Core.Boc;
using TonDict = Ton.Core.Dict;

namespace Ton.Core.Types;

/// <summary>
///     State initialization data for TON smart contracts.
///     Contains code, data, and optional special features like split depth and libraries.
///     Source:
///     https://github.com/ton-blockchain/ton/blob/24dc184a2ea67f9c47042b4104bbb4d82289fac1/crypto/block/block.tlb#L141
///     TL-B: _ split_depth:(Maybe (## 5)) special:(Maybe TickTock)
///     code:(Maybe ^Cell) data:(Maybe ^Cell)
///     library:(HashmapE 256 SimpleLib) = StateInit;
/// </summary>
public record StateInit
{
    /// <summary>
    ///     Creates a new StateInit instance.
    /// </summary>
    public StateInit(
        Cell? code = null,
        Cell? data = null,
        int? splitDepth = null,
        TickTock? special = null, TonDict.Dictionary<TonDict.DictKeyBigInt, SimpleLibrary>? libraries = null)
    {
        Code = code;
        Data = data;
        SplitDepth = splitDepth;
        Special = special;
        Libraries = libraries;
    }

    /// <summary>
    ///     Split depth for large contracts (0-31). Used in sharded smart contracts.
    /// </summary>
    public int? SplitDepth { get; init; }

    /// <summary>
    ///     Special tick-tock flag for system contracts.
    /// </summary>
    public TickTock? Special { get; init; }

    /// <summary>
    ///     Contract code (TVM bytecode).
    /// </summary>
    public Cell? Code { get; init; }

    /// <summary>
    ///     Contract persistent data.
    /// </summary>
    public Cell? Data { get; init; }

    /// <summary>
    ///     Libraries dictionary. Maps 256-bit library hashes to library cells.
    /// </summary>
    public TonDict.Dictionary<TonDict.DictKeyBigInt, SimpleLibrary>? Libraries { get; init; }

    /// <summary>
    ///     Loads StateInit from a slice.
    /// </summary>
    /// <param name="slice">Slice to load from.</param>
    /// <returns>Loaded StateInit.</returns>
    public static StateInit Load(Slice slice)
    {
        // Load split depth (Maybe (## 5))
        int? splitDepth = null;
        if (slice.LoadBit()) splitDepth = (int)slice.LoadUint(5);

        // Load special (Maybe TickTock)
        TickTock? special = null;
        if (slice.LoadBit()) special = TickTock.Load(slice);

        // Load code and data (Maybe ^Cell)
        Cell? code = slice.LoadMaybeRef();
        Cell? data = slice.LoadMaybeRef();

        // Load libraries (HashmapE 256 SimpleLib)
        // Note: Despite being HashmapE, it's stored as Maybe ^Cell in practice
        TonDict.Dictionary<TonDict.DictKeyBigInt, SimpleLibrary>? libraries = slice.LoadDict(
            TonDict.DictionaryKeys.BigUint(256),
            new SimpleLibraryValue()
        );

        // JS SDK treats empty dictionary as undefined
        if (libraries != null && libraries.Size == 0) libraries = null;

        return new StateInit(code, data, splitDepth, special, libraries);
    }

    /// <summary>
    ///     Stores StateInit to a builder.
    /// </summary>
    /// <param name="builder">Builder to store to.</param>
    public void Store(Builder builder)
    {
        // Store split depth
        if (SplitDepth.HasValue)
        {
            builder.StoreBit(true);
            builder.StoreUint(SplitDepth.Value, 5);
        }
        else
        {
            builder.StoreBit(false);
        }

        // Store special
        if (Special != null)
        {
            builder.StoreBit(true);
            Special.Store(builder);
        }
        else
        {
            builder.StoreBit(false);
        }

        // Store code and data
        builder.StoreMaybeRef(Code);
        builder.StoreMaybeRef(Data);

        // Store libraries (HashmapE as Maybe ^Cell in practice)
        builder.StoreDict(Libraries);
    }

    /// <summary>
    ///     Dictionary value serializer for SimpleLibrary.
    /// </summary>
    class SimpleLibraryValue : TonDict.IDictionaryValue<SimpleLibrary>
    {
        public void Serialize(SimpleLibrary value, Builder builder)
        {
            value.Store(builder);
        }

        public SimpleLibrary Parse(Slice slice)
        {
            return SimpleLibrary.Load(slice);
        }
    }
}