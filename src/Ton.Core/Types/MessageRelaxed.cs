using Ton.Core.Boc;

namespace Ton.Core.Types;

/// <summary>
///     Represents a TON message with relaxed CommonMessageInfo (allows Maybe for src addresses).
///     Source:
///     https://github.com/ton-blockchain/ton/blob/24dc184a2ea67f9c47042b4104bbb4d82289fac1/crypto/block/block.tlb#L151
///     message$_ {X:Type} info:CommonMsgInfoRelaxed
///     init:(Maybe (Either StateInit ^StateInit))
///     body:(Either X ^X) = MessageRelaxed X;
/// </summary>
public record MessageRelaxed
{
    /// <summary>
    ///     Creates a new MessageRelaxed.
    /// </summary>
    public MessageRelaxed(CommonMessageInfoRelaxed info, Cell body, StateInit? init = null)
    {
        Info = info;
        Body = body;
        Init = init;
    }

    /// <summary>
    ///     Common message info (relaxed - allows Maybe for src).
    /// </summary>
    public CommonMessageInfoRelaxed Info { get; init; }

    /// <summary>
    ///     Optional state initialization for contract deployment.
    /// </summary>
    public StateInit? Init { get; init; }

    /// <summary>
    ///     Message body (can be empty).
    /// </summary>
    public Cell Body { get; init; }

    /// <summary>
    ///     Loads a MessageRelaxed from a Slice.
    /// </summary>
    public static MessageRelaxed Load(Slice slice)
    {
        CommonMessageInfoRelaxed info = CommonMessageInfoRelaxed.Load(slice);
        StateInit? init = null;

        // Load init (Maybe (Either StateInit ^StateInit))
        if (slice.LoadBit())
            // Has init
            if (!slice.LoadBit())
                // Inline StateInit
                init = StateInit.Load(slice);
            else
                // StateInit in ref
                init = StateInit.Load(slice.LoadRef().BeginParse());

        // Load body (Either X ^X)
        Cell body = slice.LoadBit() ? slice.LoadRef() : slice.AsCell();

        return new MessageRelaxed(info, body, init);
    }

    /// <summary>
    ///     Stores the MessageRelaxed into a Builder.
    /// </summary>
    public void Store(Builder builder, bool forceRef = false)
    {
        // Store CommonMsgInfoRelaxed
        Info.Store(builder);

        // Store init
        if (Init != null)
        {
            builder.StoreBit(true);
            Builder initCell = Builder.BeginCell();
            Init.Store(initCell);

            // Check if need to store it in ref
            bool needRef = forceRef || builder.AvailableBits - 2 < initCell.Bits;

            // Persist init
            if (needRef)
            {
                builder.StoreBit(true);
                builder.StoreRef(initCell.EndCell());
            }
            else
            {
                builder.StoreBit(false);
                builder.StoreBuilder(initCell);
            }
        }
        else
        {
            builder.StoreBit(false);
        }

        // Store body
        bool bodyNeedRef = forceRef ||
                           builder.AvailableBits - 1 < Body.Bits.Length ||
                           builder.Refs + Body.Refs.Length > 4 ||
                           Body.IsExotic;

        if (bodyNeedRef)
        {
            builder.StoreBit(true);
            builder.StoreRef(Body);
        }
        else
        {
            builder.StoreBit(false);
            builder.StoreBuilder(Body.AsBuilder());
        }
    }
}