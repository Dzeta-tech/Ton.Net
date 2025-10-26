using System.Numerics;
using Ton.Core.Boc;

namespace Ton.Core.Types;

/// <summary>
///     Account state variants.
///     Source:
///     https://github.com/ton-blockchain/ton/blob/24dc184a2ea67f9c47042b4104bbb4d82289fac1/crypto/block/block.tlb#L239
///     account_uninit$00 = AccountState;
///     account_active$1 _:StateInit = AccountState;
///     account_frozen$01 state_hash:bits256 = AccountState;
/// </summary>
public abstract record AccountState
{
    /// <summary>
    ///     Loads AccountState from a Slice.
    /// </summary>
    public static AccountState Load(Slice slice)
    {
        if (slice.LoadBit())
            return new Active(StateInit.Load(slice));

        if (slice.LoadBit())
            return new Frozen(slice.LoadUintBig(256));

        return new Uninit();
    }

    /// <summary>
    ///     Stores AccountState into a Builder.
    /// </summary>
    public void Store(Builder builder)
    {
        switch (this)
        {
            case Active active:
                builder.StoreBit(true);
                active.State.Store(builder);
                break;
            case Frozen frozen:
                builder.StoreBit(false);
                builder.StoreBit(true);
                builder.StoreUint(frozen.StateHash, 256);
                break;
            case Uninit:
                builder.StoreBit(false);
                builder.StoreBit(false);
                break;
        }
    }

    /// <summary>
    ///     Uninitialized account state.
    /// </summary>
    public record Uninit : AccountState;

    /// <summary>
    ///     Active account state with StateInit.
    /// </summary>
    public record Active(StateInit State) : AccountState;

    /// <summary>
    ///     Frozen account state with state hash.
    /// </summary>
    public record Frozen(BigInteger StateHash) : AccountState;
}