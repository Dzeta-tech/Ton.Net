using Ton.Core.Boc;
using Ton.Core.Dict;

namespace Ton.Core.Types;

/// <summary>
///     Represents a TON message.
///     Source:
///     https://github.com/ton-blockchain/ton/blob/24dc184a2ea67f9c47042b4104bbb4d82289fac1/crypto/block/block.tlb#L147
///     message$_ {X:Type} info:CommonMsgInfo
///     init:(Maybe (Either StateInit ^StateInit))
///     body:(Either X ^X) = Message X;
/// </summary>
public record Message
{
    /// <summary>
    ///     Creates a new Message.
    /// </summary>
    public Message(CommonMessageInfo info, Cell body, StateInit? init = null)
    {
        Info = info;
        Body = body;
        Init = init;
    }

    /// <summary>
    ///     Common message info (internal/external-in/external-out).
    /// </summary>
    public CommonMessageInfo Info { get; init; }

    /// <summary>
    ///     Optional state initialization for contract deployment.
    /// </summary>
    public StateInit? Init { get; init; }

    /// <summary>
    ///     Message body (can be empty).
    /// </summary>
    public Cell Body { get; init; }

    /// <summary>
    ///     Loads a Message from a Slice.
    /// </summary>
    /// <param name="slice">The slice to load from.</param>
    /// <returns>A new Message instance.</returns>
    public static Message Load(Slice slice)
    {
        CommonMessageInfo info = CommonMessageInfo.Load(slice);
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

        return new Message(info, body, init);
    }

    /// <summary>
    ///     Stores the Message into a Builder.
    /// </summary>
    /// <param name="builder">The builder to store into.</param>
    /// <param name="forceRef">Force storing init and body as references.</param>
    public void Store(Builder builder, bool forceRef = false)
    {
        // Store CommonMsgInfo
        Info.Store(builder);

        // Store init
        if (Init != null)
        {
            builder.StoreBit(true);
            Builder initCell = Builder.BeginCell();
            Init.Store(initCell);

            // Check if need to store it in ref
            bool needRef = forceRef ||
                           builder.AvailableBits - 2 /* At least two bits for ref flags */ <
                           initCell.Bits + Body.Bits.Length;

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
                           builder.AvailableBits - 1 /* At least one bit for ref flag */ < Body.Bits.Length ||
                           builder.Refs + Body.Refs.Length > 4;

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

/// <summary>
///     Dictionary value serializer for Message.
/// </summary>
internal class MessageValue : IDictionaryValue<Message>
{
    public void Serialize(Message value, Builder builder)
    {
        Builder messageBuilder = Builder.BeginCell();
        value.Store(messageBuilder);
        builder.StoreRef(messageBuilder.EndCell());
    }

    public Message Parse(Slice slice)
    {
        return Message.Load(slice.LoadRef().BeginParse());
    }
}