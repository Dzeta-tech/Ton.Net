using System.Numerics;
using Ton.Core.Boc;
using TonDict = Ton.Core.Dict;

namespace Ton.Core.Types;

/// <summary>
///     Transaction structure.
///     Source:
///     https://github.com/ton-blockchain/ton/blob/24dc184a2ea67f9c47042b4104bbb4d82289fac1/crypto/block/block.tlb#L263
///     transaction$0111 account_addr:bits256 lt:uint64
///     prev_trans_hash:bits256 prev_trans_lt:uint64 now:uint32
///     outmsg_cnt:uint15
///     orig_status:AccountStatus end_status:AccountStatus
///     ^[ in_msg:(Maybe ^(Message Any)) out_msgs:(HashmapE 15 ^(Message Any)) ]
///     total_fees:CurrencyCollection state_update:^(HASH_UPDATE Account)
///     description:^TransactionDescr = Transaction;
/// </summary>
public record Transaction(
    BigInteger Address,
    ulong Lt,
    BigInteger PrevTransactionHash,
    ulong PrevTransactionLt,
    uint Now,
    ushort OutMessagesCount,
    AccountStatus OldStatus,
    AccountStatus EndStatus,
    Message? InMessage,
    TonDict.Dictionary<TonDict.DictKeyUint, Message> OutMessages,
    CurrencyCollection TotalFees,
    HashUpdate StateUpdate,
    TransactionDescription Description,
    Cell Raw
)
{
    /// <summary>
    ///     Gets the hash of the transaction.
    /// </summary>
    public byte[] Hash()
    {
        return Raw.Hash();
    }

    /// <summary>
    ///     Loads Transaction from a Slice (with original cell for hash).
    /// </summary>
    public static Transaction Load(Slice slice, Cell? originalCell = null)
    {
        Cell raw = originalCell ?? slice.AsCell();

        if (slice.LoadUint(4) != 0x07)
            throw new InvalidOperationException("Invalid transaction prefix");

        BigInteger address = slice.LoadUintBig(256);
        ulong lt = (ulong)slice.LoadUint(64);
        BigInteger prevTransactionHash = slice.LoadUintBig(256);
        ulong prevTransactionLt = (ulong)slice.LoadUint(64);
        uint now = (uint)slice.LoadUint(32);
        ushort outMessagesCount = (ushort)slice.LoadUint(15);
        AccountStatus oldStatus = slice.LoadAccountStatus();
        AccountStatus endStatus = slice.LoadAccountStatus();

        // Load messages from ref
        Slice msgSlice = slice.LoadRef().BeginParse();
        Message? inMessage = msgSlice.LoadBit() ? Message.Load(msgSlice.LoadRef().BeginParse()) : null;
        TonDict.Dictionary<TonDict.DictKeyUint, Message> outMessages = msgSlice.LoadDict(
            TonDict.DictionaryKeys.Uint(15),
            new MessageDictValue()
        ) ?? TonDict.Dictionary<TonDict.DictKeyUint, Message>.Empty(
            TonDict.DictionaryKeys.Uint(15),
            new MessageDictValue()
        );

        CurrencyCollection totalFees = CurrencyCollection.Load(slice);
        HashUpdate stateUpdate = HashUpdate.Load(slice.LoadRef().BeginParse());
        TransactionDescription description = TransactionDescription.Load(slice.LoadRef().BeginParse());

        return new Transaction(
            address,
            lt,
            prevTransactionHash,
            prevTransactionLt,
            now,
            outMessagesCount,
            oldStatus,
            endStatus,
            inMessage,
            outMessages,
            totalFees,
            stateUpdate,
            description,
            raw
        );
    }

    /// <summary>
    ///     Stores Transaction into a Builder.
    /// </summary>
    public void Store(Builder builder)
    {
        builder.StoreUint(0x07, 4);
        builder.StoreUint(Address, 256);
        builder.StoreUint(Lt, 64);
        builder.StoreUint(PrevTransactionHash, 256);
        builder.StoreUint(PrevTransactionLt, 64);
        builder.StoreUint(Now, 32);
        builder.StoreUint(OutMessagesCount, 15);
        builder.StoreAccountStatus(OldStatus);
        builder.StoreAccountStatus(EndStatus);

        // Store messages in ref
        Builder msgBuilder = Builder.BeginCell();
        if (InMessage != null)
        {
            msgBuilder.StoreBit(true);
            Builder inMsgBuilder = Builder.BeginCell();
            InMessage.Store(inMsgBuilder);
            msgBuilder.StoreRef(inMsgBuilder.EndCell());
        }
        else
        {
            msgBuilder.StoreBit(false);
        }

        msgBuilder.StoreDict(OutMessages);
        builder.StoreRef(msgBuilder.EndCell());

        TotalFees.Store(builder);

        Builder stateUpdateBuilder = Builder.BeginCell();
        StateUpdate.Store(stateUpdateBuilder);
        builder.StoreRef(stateUpdateBuilder.EndCell());

        Builder descriptionBuilder = Builder.BeginCell();
        Description.Store(descriptionBuilder);
        builder.StoreRef(descriptionBuilder.EndCell());
    }
}

/// <summary>
///     Dictionary value type for Message (stored as ^Message in dict).
/// </summary>
internal class MessageDictValue : TonDict.IDictionaryValue<Message>
{
    public Message Parse(Slice slice)
    {
        return Message.Load(slice.LoadRef().BeginParse());
    }

    public void Serialize(Message value, Builder builder)
    {
        Builder msgBuilder = Builder.BeginCell();
        value.Store(msgBuilder);
        builder.StoreRef(msgBuilder.EndCell());
    }
}