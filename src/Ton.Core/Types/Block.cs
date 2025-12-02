using Ton.Core.Boc;

namespace Ton.Core.Types;

/// <summary>
///     Full block structure.
///     Source:
///     https://github.com/ton-blockchain/ton/blob/24dc184a2ea67f9c47042b4104bbb4d82289fac1/crypto/block/block.tlb#L472
///     block#11ef55aa global_id:int32
///     info:^BlockInfo value_flow:^ValueFlow
///     state_update:^(MERKLE_UPDATE ShardState)
///     extra:^BlockExtra = Block;
/// </summary>
public record Block
{
    /// <summary>
    ///     Creates a new Block.
    /// </summary>
    public Block(
        int globalId,
        BlockInfo blockInfo,
        Cell valueFlow,
        Cell stateUpdate,
        Cell extra)
    {
        GlobalId = globalId;
        BlockInfo = blockInfo;
        ValueFlow = valueFlow;
        StateUpdate = stateUpdate;
        Extra = extra;
    }

    /// <summary>
    ///     Global ID (network identifier).
    /// </summary>
    public int GlobalId { get; init; }

    /// <summary>
    ///     Block information (header).
    /// </summary>
    public BlockInfo BlockInfo { get; init; }

    /// <summary>
    ///     Value flow cell.
    /// </summary>
    public Cell ValueFlow { get; init; }

    /// <summary>
    ///     State update cell (MERKLE_UPDATE ShardState).
    /// </summary>
    public Cell StateUpdate { get; init; }

    /// <summary>
    ///     Block extra cell.
    /// </summary>
    public Cell Extra { get; init; }

    /// <summary>
    ///     Loads Block from a Slice.
    /// </summary>
    public static Block Load(Slice slice)
    {
        if (slice.LoadUint(32) != 0x11ef55aa)
            throw new InvalidOperationException("Invalid Block prefix");

        int globalId = (int)slice.LoadInt(32);

        Cell blockInfoCell = slice.LoadRef();
        BlockInfo blockInfo = BlockInfo.Load(blockInfoCell.BeginParse());

        Cell valueFlow = slice.LoadRef();
        Cell stateUpdate = slice.LoadRef();
        Cell extra = slice.LoadRef();

        return new Block(globalId, blockInfo, valueFlow, stateUpdate, extra);
    }
}

