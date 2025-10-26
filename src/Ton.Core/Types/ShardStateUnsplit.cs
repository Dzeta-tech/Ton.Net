using System.Numerics;
using Ton.Core.Boc;
using TonDict = Ton.Core.Dict;

namespace Ton.Core.Types;

/// <summary>
///     Shard state (unsplit).
///     Source:
///     https://github.com/ton-blockchain/ton/blob/24dc184a2ea67f9c47042b4104bbb4d82289fac1/crypto/block/block.tlb#L396
///     shard_state#9023afe2 global_id:int32
///     shard_id:ShardIdent
///     seq_no:uint32 vert_seq_no:#
///     gen_utime:uint32 gen_lt:uint64
///     min_ref_mc_seqno:uint32
///     out_msg_queue_info:^OutMsgQueueInfo
///     before_split:(## 1)
///     accounts:^ShardAccounts
///     ^[ overload_history:uint64 underload_history:uint64
///     total_balance:CurrencyCollection
///     total_validator_fees:CurrencyCollection
///     libraries:(HashmapE 256 LibDescr)
///     master_ref:(Maybe BlkMasterInfo) ]
///     custom:(Maybe ^McStateExtra)
///     = ShardStateUnsplit;
/// </summary>
public record ShardStateUnsplit(
    int GlobalId,
    ShardIdent ShardId,
    uint Seqno,
    uint VertSeqNo,
    uint GenUtime,
    BigInteger GenLt,
    uint MinRefMcSeqno,
    bool BeforeSplit,
    TonDict.Dictionary<TonDict.DictKeyBigInt, ShardAccountRef>? Accounts
)
{
    /// <summary>
    ///     Loads ShardStateUnsplit from a Slice.
    /// </summary>
    public static ShardStateUnsplit Load(Slice slice)
    {
        if (slice.LoadUint(32) != 0x9023afe2)
            throw new InvalidOperationException("Invalid ShardStateUnsplit prefix");

        int globalId = (int)slice.LoadInt(32);
        ShardIdent shardId = ShardIdent.Load(slice);
        uint seqno = (uint)slice.LoadUint(32);
        uint vertSeqNo = (uint)slice.LoadUint(32);
        uint genUtime = (uint)slice.LoadUint(32);
        BigInteger genLt = slice.LoadUintBig(64);
        uint minRefMcSeqno = (uint)slice.LoadUint(32);

        // Skip OutMsgQueueInfo (usually exotic)
        slice.LoadRef();

        bool beforeSplit = slice.LoadBit();

        // Parse accounts
        Cell shardAccountsRef = slice.LoadRef();
        TonDict.Dictionary<TonDict.DictKeyBigInt, ShardAccountRef>? accounts = null;
        if (!shardAccountsRef.IsExotic) accounts = shardAccountsRef.BeginParse().LoadShardAccounts();

        // Skip other fields (not used by most apps)
        // - overload_history, underload_history, balances, libraries, etc.
        // We're only interested in the core shard info and accounts

        return new ShardStateUnsplit(
            globalId,
            shardId,
            seqno,
            vertSeqNo,
            genUtime,
            genLt,
            minRefMcSeqno,
            beforeSplit,
            accounts
        );
    }

    /// <summary>
    ///     Stores ShardStateUnsplit into a Builder (partial implementation).
    /// </summary>
    public void Store(Builder builder)
    {
        builder.StoreUint(0x9023afe2, 32);
        builder.StoreInt(GlobalId, 32);
        ShardId.Store(builder);
        builder.StoreUint(Seqno, 32);
        builder.StoreUint(VertSeqNo, 32);
        builder.StoreUint(GenUtime, 32);
        builder.StoreUint(GenLt, 64);
        builder.StoreUint(MinRefMcSeqno, 32);

        // Note: Full serialization would require OutMsgQueueInfo and other fields
        // This is a simplified implementation for reading shard states
        throw new NotImplementedException("Full ShardStateUnsplit serialization not yet implemented");
    }
}