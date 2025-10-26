using System.Numerics;
using Ton.Core.Boc;

namespace Ton.Core.Types;

/// <summary>
///     Transaction compute phase (union type).
///     Source:
///     https://github.com/ton-blockchain/ton/blob/24dc184a2ea67f9c47042b4104bbb4d82289fac1/crypto/block/block.tlb#L296
///     tr_phase_compute_skipped$0 reason:ComputeSkipReason = TrComputePhase;
///     tr_phase_compute_vm$1 success:Bool msg_state_used:Bool
///     account_activated:Bool gas_fees:Grams
///     ^[ gas_used:(VarUInteger 7) gas_limit:(VarUInteger 7) gas_credit:(Maybe (VarUInteger 3))
///     mode:int8 exit_code:int32 exit_arg:(Maybe int32)
///     vm_steps:uint32 vm_init_state_hash:bits256 vm_final_state_hash:bits256 ]
///     = TrComputePhase;
/// </summary>
public abstract record TransactionComputePhase
{
    /// <summary>
    ///     Loads TransactionComputePhase from a Slice.
    /// </summary>
    public static TransactionComputePhase Load(Slice slice)
    {
        // Skipped (0 bit)
        if (!slice.LoadBit())
        {
            ComputeSkipReason reason = slice.LoadComputeSkipReason();
            return new Skipped(reason);
        }

        // VM (1 bit)
        bool success = slice.LoadBit();
        bool messageStateUsed = slice.LoadBit();
        bool accountActivated = slice.LoadBit();
        BigInteger gasFees = slice.LoadCoins();

        Slice vmState = slice.LoadRef().BeginParse();
        BigInteger gasUsed = vmState.LoadVarUintBig(3); // VarUInteger 7 → log2(7) ≈ 3
        BigInteger gasLimit = vmState.LoadVarUintBig(3); // VarUInteger 7 → log2(7) ≈ 3
        BigInteger? gasCredit = vmState.LoadBit() ? vmState.LoadVarUintBig(2) : null; // VarUInteger 3 → log2(3) ≈ 2
        int mode = (int)vmState.LoadInt(8);
        int exitCode = (int)vmState.LoadInt(32);
        int? exitArg = vmState.LoadBit() ? (int)vmState.LoadInt(32) : null;
        uint vmSteps = (uint)vmState.LoadUint(32);
        BigInteger vmInitStateHash = vmState.LoadUintBig(256);
        BigInteger vmFinalStateHash = vmState.LoadUintBig(256);

        return new Vm(
            success,
            messageStateUsed,
            accountActivated,
            gasFees,
            gasUsed,
            gasLimit,
            gasCredit,
            mode,
            exitCode,
            exitArg,
            vmSteps,
            vmInitStateHash,
            vmFinalStateHash
        );
    }

    /// <summary>
    ///     Stores TransactionComputePhase into a Builder.
    /// </summary>
    public abstract void Store(Builder builder);

    /// <summary>
    ///     Compute was skipped.
    /// </summary>
    public record Skipped(ComputeSkipReason Reason) : TransactionComputePhase
    {
        public override void Store(Builder builder)
        {
            builder.StoreBit(false);
            builder.StoreComputeSkipReason(Reason);
        }
    }

    /// <summary>
    ///     Compute was executed in VM.
    /// </summary>
    public record Vm(
        bool Success,
        bool MessageStateUsed,
        bool AccountActivated,
        BigInteger GasFees,
        BigInteger GasUsed,
        BigInteger GasLimit,
        BigInteger? GasCredit,
        int Mode,
        int ExitCode,
        int? ExitArg,
        uint VmSteps,
        BigInteger VmInitStateHash,
        BigInteger VmFinalStateHash
    ) : TransactionComputePhase
    {
        public override void Store(Builder builder)
        {
            builder.StoreBit(true);
            builder.StoreBit(Success);
            builder.StoreBit(MessageStateUsed);
            builder.StoreBit(AccountActivated);
            builder.StoreCoins(GasFees);

            Builder vmStateBuilder = Builder.BeginCell();
            vmStateBuilder.StoreVarUint(GasUsed, 3); // VarUInteger 7 → log2(7) ≈ 3
            vmStateBuilder.StoreVarUint(GasLimit, 3); // VarUInteger 7 → log2(7) ≈ 3

            if (GasCredit == null)
            {
                vmStateBuilder.StoreBit(false);
            }
            else
            {
                vmStateBuilder.StoreBit(true);
                vmStateBuilder.StoreVarUint(GasCredit.Value, 2); // VarUInteger 3 → log2(3) ≈ 2
            }

            vmStateBuilder.StoreInt(Mode, 8);
            vmStateBuilder.StoreInt(ExitCode, 32);

            if (ExitArg == null)
            {
                vmStateBuilder.StoreBit(false);
            }
            else
            {
                vmStateBuilder.StoreBit(true);
                vmStateBuilder.StoreInt(ExitArg.Value, 32);
            }

            vmStateBuilder.StoreUint(VmSteps, 32);
            vmStateBuilder.StoreUint(VmInitStateHash, 256);
            vmStateBuilder.StoreUint(VmFinalStateHash, 256);

            builder.StoreRef(vmStateBuilder.EndCell());
        }
    }
}