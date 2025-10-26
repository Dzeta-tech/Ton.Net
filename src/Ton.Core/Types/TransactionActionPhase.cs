using System.Numerics;
using Ton.Core.Boc;

namespace Ton.Core.Types;

/// <summary>
///     Transaction action phase.
///     Source:
///     https://github.com/ton-blockchain/ton/blob/24dc184a2ea67f9c47042b4104bbb4d82289fac1/crypto/block/block.tlb#L310
///     tr_phase_action$_ success:Bool valid:Bool no_funds:Bool
///     status_change:AccStatusChange
///     total_fwd_fees:(Maybe Grams) total_action_fees:(Maybe Grams)
///     result_code:int32 result_arg:(Maybe int32) tot_actions:uint16
///     spec_actions:uint16 skipped_actions:uint16 msgs_created:uint16
///     action_list_hash:bits256 tot_msg_size:StorageUsedShort
///     = TrActionPhase;
/// </summary>
public record TransactionActionPhase(
    bool Success,
    bool Valid,
    bool NoFunds,
    AccountStatusChange StatusChange,
    BigInteger? TotalFwdFees,
    BigInteger? TotalActionFees,
    int ResultCode,
    int? ResultArg,
    ushort TotalActions,
    ushort SpecActions,
    ushort SkippedActions,
    ushort MessagesCreated,
    BigInteger ActionListHash,
    StorageUsed TotalMessageSize
)
{
    /// <summary>
    ///     Loads TransactionActionPhase from a Slice.
    /// </summary>
    public static TransactionActionPhase Load(Slice slice)
    {
        bool success = slice.LoadBit();
        bool valid = slice.LoadBit();
        bool noFunds = slice.LoadBit();
        AccountStatusChange statusChange = slice.LoadAccountStatusChange();
        BigInteger? totalFwdFees = slice.LoadBit() ? slice.LoadCoins() : null;
        BigInteger? totalActionFees = slice.LoadBit() ? slice.LoadCoins() : null;
        int resultCode = (int)slice.LoadInt(32);
        int? resultArg = slice.LoadBit() ? (int)slice.LoadInt(32) : null;
        ushort totalActions = (ushort)slice.LoadUint(16);
        ushort specActions = (ushort)slice.LoadUint(16);
        ushort skippedActions = (ushort)slice.LoadUint(16);
        ushort messagesCreated = (ushort)slice.LoadUint(16);
        BigInteger actionListHash = slice.LoadUintBig(256);
        StorageUsed totalMessageSize = StorageUsed.Load(slice);

        return new TransactionActionPhase(
            success,
            valid,
            noFunds,
            statusChange,
            totalFwdFees,
            totalActionFees,
            resultCode,
            resultArg,
            totalActions,
            specActions,
            skippedActions,
            messagesCreated,
            actionListHash,
            totalMessageSize
        );
    }

    /// <summary>
    ///     Stores TransactionActionPhase into a Builder.
    /// </summary>
    public void Store(Builder builder)
    {
        builder.StoreBit(Success);
        builder.StoreBit(Valid);
        builder.StoreBit(NoFunds);
        builder.StoreAccountStatusChange(StatusChange);

        if (TotalFwdFees == null)
        {
            builder.StoreBit(false);
        }
        else
        {
            builder.StoreBit(true);
            builder.StoreCoins(TotalFwdFees.Value);
        }

        if (TotalActionFees == null)
        {
            builder.StoreBit(false);
        }
        else
        {
            builder.StoreBit(true);
            builder.StoreCoins(TotalActionFees.Value);
        }

        builder.StoreInt(ResultCode, 32);

        if (ResultArg == null)
        {
            builder.StoreBit(false);
        }
        else
        {
            builder.StoreBit(true);
            builder.StoreInt(ResultArg.Value, 32);
        }

        builder.StoreUint(TotalActions, 16);
        builder.StoreUint(SpecActions, 16);
        builder.StoreUint(SkippedActions, 16);
        builder.StoreUint(MessagesCreated, 16);
        builder.StoreUint(ActionListHash, 256);
        TotalMessageSize.Store(builder);
    }
}