using Ton.Core.Boc;

namespace Ton.Core.Types;

/// <summary>
///     Hash update structure for state changes.
///     Source:
///     https://github.com/ton-blockchain/ton/blob/24dc184a2ea67f9c47042b4104bbb4d82289fac1/crypto/block/block.tlb#L273
///     update_hashes#72 {X:Type} old_hash:bits256 new_hash:bits256 = HASH_UPDATE X;
/// </summary>
public record HashUpdate(byte[] OldHash, byte[] NewHash)
{
    /// <summary>
    ///     Loads HashUpdate from a Slice.
    /// </summary>
    public static HashUpdate Load(Slice slice)
    {
        if (slice.LoadUint(8) != 0x72)
            throw new InvalidOperationException("Invalid HashUpdate prefix");

        return new HashUpdate(
            slice.LoadBuffer(32),
            slice.LoadBuffer(32)
        );
    }

    /// <summary>
    ///     Stores HashUpdate into a Builder.
    /// </summary>
    public void Store(Builder builder)
    {
        builder.StoreUint(0x72, 8);
        builder.StoreBuffer(OldHash);
        builder.StoreBuffer(NewHash);
    }
}