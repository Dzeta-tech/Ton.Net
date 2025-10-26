using Ton.Core.Boc;

namespace Ton.Core.Types;

/// <summary>
///     Output action (union type with 4 variants).
///     Source:
///     https://github.com/ton-blockchain/ton/blob/24dc184a2ea67f9c47042b4104bbb4d82289fac1/crypto/block/block.tlb#L188-L197
/// </summary>
public abstract record OutAction
{
    /// <summary>
    ///     Loads OutAction from a Slice.
    /// </summary>
    public static OutAction Load(Slice slice)
    {
        uint tag = (uint)slice.LoadUint(32);

        return tag switch
        {
            0x0ec3c86d => new SendMsg(
                (SendMode)slice.LoadUint(8),
                MessageRelaxed.Load(slice.LoadRef().BeginParse())
            ),
            0xad4de08e => new SetCode(slice.LoadRef()),
            0x36e6b809 => new Reserve(
                (ReserveMode)slice.LoadUint(8),
                CurrencyCollection.Load(slice)
            ),
            0x26fa1dd4 => new ChangeLibrary(
                (byte)slice.LoadUint(7),
                LibRef.Load(slice)
            ),
            _ => throw new InvalidOperationException($"Unknown out action tag: 0x{tag:x}")
        };
    }

    /// <summary>
    ///     Stores OutAction into a Builder.
    /// </summary>
    public abstract void Store(Builder builder);

    /// <summary>
    ///     Send message action.
    ///     action_send_msg#0ec3c86d mode:(## 8) out_msg:^(MessageRelaxed Any) = OutAction;
    /// </summary>
    public record SendMsg(SendMode Mode, MessageRelaxed OutMsg) : OutAction
    {
        public override void Store(Builder builder)
        {
            builder.StoreUint(0x0ec3c86d, 32);
            builder.StoreUint((byte)Mode, 8);

            Builder msgBuilder = Builder.BeginCell();
            OutMsg.Store(msgBuilder);
            builder.StoreRef(msgBuilder.EndCell());
        }
    }

    /// <summary>
    ///     Set code action.
    ///     action_set_code#ad4de08e new_code:^Cell = OutAction;
    /// </summary>
    public record SetCode(Cell NewCode) : OutAction
    {
        public override void Store(Builder builder)
        {
            builder.StoreUint(0xad4de08e, 32);
            builder.StoreRef(NewCode);
        }
    }

    /// <summary>
    ///     Reserve currency action.
    ///     action_reserve_currency#36e6b809 mode:(## 8) currency:CurrencyCollection = OutAction;
    /// </summary>
    public record Reserve(ReserveMode Mode, CurrencyCollection Currency) : OutAction
    {
        public override void Store(Builder builder)
        {
            builder.StoreUint(0x36e6b809, 32);
            builder.StoreUint((byte)Mode, 8);
            Currency.Store(builder);
        }
    }

    /// <summary>
    ///     Change library action.
    ///     action_change_library#26fa1dd4 mode:(## 7) libref:LibRef = OutAction;
    /// </summary>
    public record ChangeLibrary(byte Mode, LibRef LibRef) : OutAction
    {
        public override void Store(Builder builder)
        {
            builder.StoreUint(0x26fa1dd4, 32);
            builder.StoreUint(Mode, 7);
            LibRef.Store(builder);
        }
    }
}

/// <summary>
///     Output action list (linked list structure).
///     Source:
///     https://github.com/ton-blockchain/ton/blob/24dc184a2ea67f9c47042b4104bbb4d82289fac1/crypto/block/block.tlb#L199-L201
///     out_list_empty$_ = OutList 0;
///     out_list$_ {n:#} prev:^(OutList n) action:OutAction = OutList (n + 1);
/// </summary>
public static class OutList
{
    /// <summary>
    ///     Loads OutAction list from a Slice.
    /// </summary>
    public static List<OutAction> Load(Slice slice)
    {
        List<OutAction> actions = new();

        while (slice.RemainingRefs > 0)
        {
            Cell nextCell = slice.LoadRef();
            actions.Add(OutAction.Load(slice));
            slice = nextCell.BeginParse();
        }

        actions.Reverse();
        return actions;
    }

    /// <summary>
    ///     Stores OutAction list into a Builder.
    /// </summary>
    public static void Store(Builder builder, List<OutAction> actions)
    {
        Cell cell = Builder.BeginCell().EndCell();

        // Build linked list from right to left
        foreach (OutAction action in actions)
        {
            Builder actionBuilder = Builder.BeginCell();
            actionBuilder.StoreRef(cell);
            action.Store(actionBuilder);
            cell = actionBuilder.EndCell();
        }

        builder.StoreSlice(cell.BeginParse());
    }
}