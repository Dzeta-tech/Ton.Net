using Ton.Core.Boc;

namespace Ton.Core.Types;

/// <summary>
///     Library reference (union type).
///     Source:
///     https://github.com/ton-blockchain/ton/blob/128a85bee568e84146f1e985a92ea85011d1e380/crypto/block/block.tlb#L385-L386
///     libref_hash$0 lib_hash:bits256 = LibRef;
///     libref_ref$1 library:^Cell = LibRef;
/// </summary>
public abstract record LibRef
{
    /// <summary>
    ///     Loads LibRef from a Slice.
    /// </summary>
    public static LibRef Load(Slice slice)
    {
        int type = (int)slice.LoadUint(1);

        if (type == 0) return new Hash(slice.LoadBuffer(32));

        return new Ref(slice.LoadRef());
    }

    /// <summary>
    ///     Stores LibRef into a Builder.
    /// </summary>
    public abstract void Store(Builder builder);

    /// <summary>
    ///     Library reference by hash.
    /// </summary>
    public record Hash(byte[] LibHash) : LibRef
    {
        public override void Store(Builder builder)
        {
            builder.StoreUint(0, 1);
            builder.StoreBuffer(LibHash);
        }
    }

    /// <summary>
    ///     Library reference by cell.
    /// </summary>
    public record Ref(Cell Library) : LibRef
    {
        public override void Store(Builder builder)
        {
            builder.StoreUint(1, 1);
            builder.StoreRef(Library);
        }
    }
}