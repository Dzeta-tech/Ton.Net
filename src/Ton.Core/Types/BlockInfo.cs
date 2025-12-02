using Ton.Core.Boc;

namespace Ton.Core.Types;

/// <summary>
///     Block header information (BlockInfo from TL-B).
///     Source:
///     https://github.com/ton-blockchain/ton/blob/24dc184a2ea67f9c47042b4104bbb4d82289fac1/crypto/block/block.tlb#L448
///     block_info#9bc7a987 version:uint32 ...
/// </summary>
public record BlockInfo
{
    /// <summary>
    ///     Creates a new BlockInfo.
    /// </summary>
    public BlockInfo(
        uint version,
        bool notMaster,
        bool afterMerge,
        bool beforeSplit,
        bool afterSplit,
        bool wantSplit,
        bool wantMerge,
        bool keyBlock,
        bool vertSeqnoIncr,
        uint flags,
        uint seqNo,
        uint vertSeqNo,
        ShardIdent shard,
        uint genUtime,
        ulong startLt,
        ulong endLt,
        uint genValidatorListHashShort,
        uint genCatchainSeqno,
        uint minRefMcSeqno,
        uint prevKeyBlockSeqno,
        GlobalVersion? genSoftware,
        ExtBlkRef? masterRef,
        BlkPrevInfo prevRef,
        BlkPrevInfo? prevVertRef)
    {
        Version = version;
        NotMaster = notMaster;
        AfterMerge = afterMerge;
        BeforeSplit = beforeSplit;
        AfterSplit = afterSplit;
        WantSplit = wantSplit;
        WantMerge = wantMerge;
        KeyBlock = keyBlock;
        VertSeqnoIncr = vertSeqnoIncr;
        Flags = flags;
        SeqNo = seqNo;
        VertSeqNo = vertSeqNo;
        Shard = shard;
        GenUtime = genUtime;
        StartLt = startLt;
        EndLt = endLt;
        GenValidatorListHashShort = genValidatorListHashShort;
        GenCatchainSeqno = genCatchainSeqno;
        MinRefMcSeqno = minRefMcSeqno;
        PrevKeyBlockSeqno = prevKeyBlockSeqno;
        GenSoftware = genSoftware;
        MasterRef = masterRef;
        PrevRef = prevRef;
        PrevVertRef = prevVertRef;
    }

    /// <summary>
    ///     Protocol version.
    /// </summary>
    public uint Version { get; init; }

    /// <summary>
    ///     True if this is not a masterchain block.
    /// </summary>
    public bool NotMaster { get; init; }

    /// <summary>
    ///     True if this block is after a merge.
    /// </summary>
    public bool AfterMerge { get; init; }

    /// <summary>
    ///     True if this block is before a split.
    /// </summary>
    public bool BeforeSplit { get; init; }

    /// <summary>
    ///     True if this block is after a split.
    /// </summary>
    public bool AfterSplit { get; init; }

    /// <summary>
    ///     True if shard wants to split.
    /// </summary>
    public bool WantSplit { get; init; }

    /// <summary>
    ///     True if shard wants to merge.
    /// </summary>
    public bool WantMerge { get; init; }

    /// <summary>
    ///     True if this is a key block.
    /// </summary>
    public bool KeyBlock { get; init; }

    /// <summary>
    ///     True if vertical sequence number is incremented.
    /// </summary>
    public bool VertSeqnoIncr { get; init; }

    /// <summary>
    ///     Flags (8 bits, must be less than or equal to 1).
    /// </summary>
    public uint Flags { get; init; }

    /// <summary>
    ///     Block sequence number.
    /// </summary>
    public uint SeqNo { get; init; }

    /// <summary>
    ///     Vertical sequence number.
    /// </summary>
    public uint VertSeqNo { get; init; }

    /// <summary>
    ///     Shard identifier.
    /// </summary>
    public ShardIdent Shard { get; init; }

    /// <summary>
    ///     Generation unix time.
    /// </summary>
    public uint GenUtime { get; init; }

    /// <summary>
    ///     Start logical time.
    /// </summary>
    public ulong StartLt { get; init; }

    /// <summary>
    ///     End logical time.
    /// </summary>
    public ulong EndLt { get; init; }

    /// <summary>
    ///     Generation validator list hash (short).
    /// </summary>
    public uint GenValidatorListHashShort { get; init; }

    /// <summary>
    ///     Generation catchain sequence number.
    /// </summary>
    public uint GenCatchainSeqno { get; init; }

    /// <summary>
    ///     Minimum referenced masterchain sequence number.
    /// </summary>
    public uint MinRefMcSeqno { get; init; }

    /// <summary>
    ///     Previous key block sequence number.
    /// </summary>
    public uint PrevKeyBlockSeqno { get; init; }

    /// <summary>
    ///     Generation software version (if flags bit 0 is set).
    /// </summary>
    public GlobalVersion? GenSoftware { get; init; }

    /// <summary>
    ///     Master reference (if not masterchain).
    /// </summary>
    public ExtBlkRef? MasterRef { get; init; }

    /// <summary>
    ///     Previous block reference(s).
    /// </summary>
    public BlkPrevInfo PrevRef { get; init; }

    /// <summary>
    ///     Previous vertical reference (if vert_seqno_incr is set).
    /// </summary>
    public BlkPrevInfo? PrevVertRef { get; init; }

    /// <summary>
    ///     Loads BlockInfo from a Slice.
    /// </summary>
    public static BlockInfo Load(Slice slice)
    {
        if (slice.LoadUint(32) != 0x9bc7a987)
            throw new InvalidOperationException("Invalid BlockInfo prefix");

        uint version = (uint)slice.LoadUint(32);
        bool notMaster = slice.LoadBit();
        bool afterMerge = slice.LoadBit();
        bool beforeSplit = slice.LoadBit();
        bool afterSplit = slice.LoadBit();
        bool wantSplit = slice.LoadBit();
        bool wantMerge = slice.LoadBit();
        bool keyBlock = slice.LoadBit();
        bool vertSeqnoIncr = slice.LoadBit();
        uint flags = (uint)slice.LoadUint(8);
        uint seqNo = (uint)slice.LoadUint(32);
        uint vertSeqNo = (uint)slice.LoadUint(32);
        ShardIdent shard = ShardIdent.Load(slice);
        uint genUtime = (uint)slice.LoadUint(32);
        ulong startLt = (ulong)slice.LoadUintBig(64);
        ulong endLt = (ulong)slice.LoadUintBig(64);
        uint genValidatorListHashShort = (uint)slice.LoadUint(32);
        uint genCatchainSeqno = (uint)slice.LoadUint(32);
        uint minRefMcSeqno = (uint)slice.LoadUint(32);
        uint prevKeyBlockSeqno = (uint)slice.LoadUint(32);

        GlobalVersion? genSoftware = null;
        if ((flags & 1) != 0)
        {
            genSoftware = GlobalVersion.Load(slice);
        }

        ExtBlkRef? masterRef = null;
        if (notMaster)
        {
            Cell masterRefCell = slice.LoadRef();
            masterRef = ExtBlkRef.Load(masterRefCell.BeginParse());
        }

        Cell prevRefCell = slice.LoadRef();
        BlkPrevInfo prevRef = BlkPrevInfo.Load(prevRefCell.BeginParse(), afterMerge);

        BlkPrevInfo? prevVertRef = null;
        if (vertSeqnoIncr)
        {
            Cell prevVertRefCell = slice.LoadRef();
            prevVertRef = BlkPrevInfo.Load(prevVertRefCell.BeginParse(), false);
        }

        return new BlockInfo(
            version,
            notMaster,
            afterMerge,
            beforeSplit,
            afterSplit,
            wantSplit,
            wantMerge,
            keyBlock,
            vertSeqnoIncr,
            flags,
            seqNo,
            vertSeqNo,
            shard,
            genUtime,
            startLt,
            endLt,
            genValidatorListHashShort,
            genCatchainSeqno,
            minRefMcSeqno,
            prevKeyBlockSeqno,
            genSoftware,
            masterRef,
            prevRef,
            prevVertRef);
    }
}

