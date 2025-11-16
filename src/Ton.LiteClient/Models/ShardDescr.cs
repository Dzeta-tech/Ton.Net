using System.Numerics;
using Ton.Core.Types;

namespace Ton.LiteClient.Models;

/// <summary>
///     Full shard description as returned by lite servers (<c>ShardDescr</c> TL-B).
///     Inherits <see cref="BlockId" /> so it can be used anywhere a block identifier is expected.
///     Source:
///     <see href="https://github.com/ton-blockchain/ton/blob/master/crypto/block/block.tlb#L519"/>.
/// </summary>
public sealed record ShardDescr : BlockId
{
    /// <summary>
    ///     Creates a new shard description record.
    /// </summary>
    /// <param name="workchain">Workchain identifier for this shard (-1 for masterchain, 0 for basechain).</param>
    /// <param name="shard">Shard identifier (as in <c>ShardIdent</c> / block ID).</param>
    /// <param name="seqno">Shardchain block sequence number (<c>seq_no</c>).</param>
    /// <param name="rootHash">Header root hash of the shard block (<c>root_hash</c>).</param>
    /// <param name="fileHash">File hash of the shard block (<c>file_hash</c>).</param>
    /// <param name="regMcSeqno">
    ///     Reference masterchain sequence number (<c>reg_mc_seqno</c>) used to register this shard
    ///     in the masterchain.
    /// </param>
    /// <param name="startLt">Logical time at the start of this block (<c>start_lt</c>).</param>
    /// <param name="endLt">Logical time at the end of this block (<c>end_lt</c>).</param>
    /// <param name="beforeSplit">Whether this shard is in <c>before_split</c> state.</param>
    /// <param name="beforeMerge">Whether this shard is in <c>before_merge</c> state.</param>
    /// <param name="wantSplit">Whether validators want to split this shard (<c>want_split</c>).</param>
    /// <param name="wantMerge">Whether validators want to merge this shard (<c>want_merge</c>).</param>
    /// <param name="nxCcUpdated">
    ///     Indicates that the next catchain configuration has changed for this shard
    ///     (<c>nx_cc_updated</c> flag).
    /// </param>
    /// <param name="flags">Reserved shard flags (<c>flags</c>), currently expected to be 0.</param>
    /// <param name="nextCatchainSeqno">Sequence number of the next catchain for this shard (<c>next_catchain_seqno</c>).</param>
    /// <param name="nextValidatorShard">
    ///     Identifier of the shard that this shard’s validator set is responsible for next
    ///     (<c>next_validator_shard</c>).
    /// </param>
    /// <param name="minRefMcSeqno">
    ///     Minimal referenced masterchain sequence number for blocks that this shard depends on
    ///     (<c>min_ref_mc_seqno</c>).
    /// </param>
    /// <param name="genUtime">Generation time of this shard block (unix timestamp, <c>gen_utime</c>).</param>
    /// <param name="splitMergeAt">
    ///     Scheduled split/merge information (<c>split_merge_at:FutureSplitMerge</c>),
    ///     or <see cref="FutureSplitMerge.None" /> if none.
    /// </param>
    /// <param name="feesCollected">
    ///     Total fees collected in this shard block (<c>fees_collected:CurrencyCollection</c>).
    /// </param>
    /// <param name="fundsCreated">
    ///     Total funds created in this shard block (<c>funds_created:CurrencyCollection</c>).
    /// </param>
    public ShardDescr(
        int workchain,
        long shard,
        uint seqno,
        byte[] rootHash,
        byte[] fileHash,
        uint regMcSeqno,
        BigInteger startLt,
        BigInteger endLt,
        bool beforeSplit,
        bool beforeMerge,
        bool wantSplit,
        bool wantMerge,
        bool nxCcUpdated,
        byte flags,
        uint nextCatchainSeqno,
        BigInteger nextValidatorShard,
        uint minRefMcSeqno,
        uint genUtime,
        FutureSplitMerge splitMergeAt,
        CurrencyCollection feesCollected,
        CurrencyCollection fundsCreated)
        : base(workchain, shard, seqno, rootHash, fileHash)
    {
        RegMcSeqno = regMcSeqno;
        StartLt = startLt;
        EndLt = endLt;
        BeforeSplit = beforeSplit;
        BeforeMerge = beforeMerge;
        WantSplit = wantSplit;
        WantMerge = wantMerge;
        NxCcUpdated = nxCcUpdated;
        Flags = flags;
        NextCatchainSeqno = nextCatchainSeqno;
        NextValidatorShard = nextValidatorShard;
        MinRefMcSeqno = minRefMcSeqno;
        GenUtime = genUtime;
        SplitMergeAt = splitMergeAt;
        FeesCollected = feesCollected;
        FundsCreated = fundsCreated;
    }

    /// <summary>
    ///     Reference masterchain sequence number (<c>reg_mc_seqno</c>).
    ///     Indicates which masterchain block registered this shard block.
    /// </summary>
    public uint RegMcSeqno { get; init; }

    /// <summary>
    ///     Logical time at the start of this shard block (<c>start_lt</c>).
    /// </summary>
    public BigInteger StartLt { get; init; }

    /// <summary>
    ///     Logical time at the end of this shard block (<c>end_lt</c>).
    /// </summary>
    public BigInteger EndLt { get; init; }

    /// <summary>
    ///     Indicates that this shard is still in the pre-split state (<c>before_split</c> flag).
    /// </summary>
    public bool BeforeSplit { get; init; }

    /// <summary>
    ///     Indicates that this shard is still in the pre-merge state (<c>before_merge</c> flag).
    /// </summary>
    public bool BeforeMerge { get; init; }

    /// <summary>
    ///     Signals that validators want to split this shard (<c>want_split</c> flag).
    /// </summary>
    public bool WantSplit { get; init; }

    /// <summary>
    ///     Signals that validators want to merge this shard (<c>want_merge</c> flag).
    /// </summary>
    public bool WantMerge { get; init; }

    /// <summary>
    ///     Indicates that the next catchain configuration has been updated for this shard
    ///     (<c>nx_cc_updated</c> flag).
    /// </summary>
    public bool NxCcUpdated { get; init; }

    /// <summary>
    ///     Additional shard flags (<c>flags</c>); must be zero in current protocol versions.
    /// </summary>
    public byte Flags { get; init; }

    /// <summary>
    ///     Sequence number of the next catchain for this shard (<c>next_catchain_seqno</c>).
    /// </summary>
    public uint NextCatchainSeqno { get; init; }

    /// <summary>
    ///     Identifier of the shard for which this shard’s validator set will be responsible next
    ///     (<c>next_validator_shard</c>, stored as <c>uint64</c> in TL-B).
    /// </summary>
    public BigInteger NextValidatorShard { get; init; }

    /// <summary>
    ///     Minimum referenced masterchain sequence number for blocks this shard depends on
    ///     (<c>min_ref_mc_seqno</c>).
    /// </summary>
    public uint MinRefMcSeqno { get; init; }

    /// <summary>
    ///     Shard block generation time (unix timestamp, <c>gen_utime</c>).
    /// </summary>
    public uint GenUtime { get; init; }

    /// <summary>
    ///     Scheduled split/merge operation for this shard (<c>split_merge_at:FutureSplitMerge</c>).
    /// </summary>
    public FutureSplitMerge SplitMergeAt { get; init; } = FutureSplitMerge.None;

    /// <summary>
    ///     Total fees collected in this shard block (<c>fees_collected</c>).
    /// </summary>
    public CurrencyCollection FeesCollected { get; init; } = new(0);

    /// <summary>
    ///     Total funds created in this shard block (<c>funds_created</c>).
    /// </summary>
    public CurrencyCollection FundsCreated { get; init; } = new(0);
}


