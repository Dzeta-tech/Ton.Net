using Ton.Core.Boc;

namespace Ton.Core.Types;

/// <summary>
///     Transaction description (union type with 7 variants).
///     Source:
///     https://github.com/ton-blockchain/ton/blob/24dc184a2ea67f9c47042b4104bbb4d82289fac1/crypto/block/block.tlb#L324
/// </summary>
public abstract record TransactionDescription
{
    /// <summary>
    ///     Loads TransactionDescription from a Slice.
    /// </summary>
    public static TransactionDescription Load(Slice slice)
    {
        int type = (int)slice.LoadUint(4);

        // trans_ord$0000 - Generic/ordinary transaction
        if (type == 0x00)
        {
            bool creditFirst = slice.LoadBit();
            TransactionStoragePhase? storagePhase = slice.LoadBit() ? TransactionStoragePhase.Load(slice) : null;
            TransactionCreditPhase? creditPhase = slice.LoadBit() ? TransactionCreditPhase.Load(slice) : null;
            TransactionComputePhase computePhase = TransactionComputePhase.Load(slice);
            TransactionActionPhase? actionPhase = slice.LoadBit()
                ? TransactionActionPhase.Load(slice.LoadRef().BeginParse())
                : null;
            bool aborted = slice.LoadBit();
            TransactionBouncePhase? bouncePhase = slice.LoadBit() ? TransactionBouncePhase.Load(slice) : null;
            bool destroyed = slice.LoadBit();

            return new Generic(
                creditFirst,
                storagePhase,
                creditPhase,
                computePhase,
                actionPhase,
                bouncePhase,
                aborted,
                destroyed
            );
        }

        // trans_storage$0001 - Storage-only transaction
        if (type == 0x01)
        {
            TransactionStoragePhase storagePhase = TransactionStoragePhase.Load(slice);
            return new Storage(storagePhase);
        }

        // trans_tick_tock$001x - Tick/tock transaction
        if (type == 0x02 || type == 0x03)
        {
            bool isTock = type == 0x03;
            TransactionStoragePhase storagePhase = TransactionStoragePhase.Load(slice);
            TransactionComputePhase computePhase = TransactionComputePhase.Load(slice);
            TransactionActionPhase? actionPhase = slice.LoadBit()
                ? TransactionActionPhase.Load(slice.LoadRef().BeginParse())
                : null;
            bool aborted = slice.LoadBit();
            bool destroyed = slice.LoadBit();

            return new TickTock(isTock, storagePhase, computePhase, actionPhase, aborted, destroyed);
        }

        // trans_split_prepare$0100 - Split prepare
        if (type == 0x04)
        {
            SplitMergeInfo splitInfo = SplitMergeInfo.Load(slice);
            TransactionStoragePhase? storagePhase = slice.LoadBit() ? TransactionStoragePhase.Load(slice) : null;
            TransactionComputePhase computePhase = TransactionComputePhase.Load(slice);
            TransactionActionPhase? actionPhase = slice.LoadBit()
                ? TransactionActionPhase.Load(slice.LoadRef().BeginParse())
                : null;
            bool aborted = slice.LoadBit();
            bool destroyed = slice.LoadBit();

            return new SplitPrepare(splitInfo, storagePhase, computePhase, actionPhase, aborted, destroyed);
        }

        // trans_split_install$0101 - Split install
        if (type == 0x05)
        {
            SplitMergeInfo splitInfo = SplitMergeInfo.Load(slice);
            Transaction prepareTransaction = Transaction.Load(slice.LoadRef().BeginParse());
            bool installed = slice.LoadBit();

            return new SplitInstall(splitInfo, prepareTransaction, installed);
        }

        // trans_merge_prepare$0110 - Merge prepare
        if (type == 0x06)
        {
            SplitMergeInfo splitInfo = SplitMergeInfo.Load(slice);
            TransactionStoragePhase storagePhase = TransactionStoragePhase.Load(slice);
            bool aborted = slice.LoadBit();

            return new MergePrepare(splitInfo, storagePhase, aborted);
        }

        // trans_merge_install$0111 - Merge install
        if (type == 0x07)
        {
            SplitMergeInfo splitInfo = SplitMergeInfo.Load(slice);
            Transaction prepareTransaction = Transaction.Load(slice.LoadRef().BeginParse());
            TransactionStoragePhase? storagePhase = slice.LoadBit() ? TransactionStoragePhase.Load(slice) : null;
            TransactionCreditPhase? creditPhase = slice.LoadBit() ? TransactionCreditPhase.Load(slice) : null;
            TransactionComputePhase computePhase = TransactionComputePhase.Load(slice);
            TransactionActionPhase? actionPhase = slice.LoadBit()
                ? TransactionActionPhase.Load(slice.LoadRef().BeginParse())
                : null;
            bool aborted = slice.LoadBit();
            bool destroyed = slice.LoadBit();

            return new MergeInstall(
                splitInfo,
                prepareTransaction,
                storagePhase,
                creditPhase,
                computePhase,
                actionPhase,
                aborted,
                destroyed
            );
        }

        throw new InvalidOperationException($"Unsupported transaction description type: {type}");
    }

    /// <summary>
    ///     Stores TransactionDescription into a Builder.
    /// </summary>
    public abstract void Store(Builder builder);

    /// <summary>
    ///     Generic/ordinary transaction.
    /// </summary>
    public record Generic(
        bool CreditFirst,
        TransactionStoragePhase? StoragePhase,
        TransactionCreditPhase? CreditPhase,
        TransactionComputePhase ComputePhase,
        TransactionActionPhase? ActionPhase,
        TransactionBouncePhase? BouncePhase,
        bool Aborted,
        bool Destroyed
    ) : TransactionDescription
    {
        public override void Store(Builder builder)
        {
            builder.StoreUint(0x00, 4);
            builder.StoreBit(CreditFirst);

            if (StoragePhase != null)
            {
                builder.StoreBit(true);
                StoragePhase.Store(builder);
            }
            else
            {
                builder.StoreBit(false);
            }

            if (CreditPhase != null)
            {
                builder.StoreBit(true);
                CreditPhase.Store(builder);
            }
            else
            {
                builder.StoreBit(false);
            }

            ComputePhase.Store(builder);

            if (ActionPhase != null)
            {
                builder.StoreBit(true);
                Builder actionBuilder = Builder.BeginCell();
                ActionPhase.Store(actionBuilder);
                builder.StoreRef(actionBuilder.EndCell());
            }
            else
            {
                builder.StoreBit(false);
            }

            builder.StoreBit(Aborted);

            if (BouncePhase != null)
            {
                builder.StoreBit(true);
                BouncePhase.Store(builder);
            }
            else
            {
                builder.StoreBit(false);
            }

            builder.StoreBit(Destroyed);
        }
    }

    /// <summary>
    ///     Storage-only transaction.
    /// </summary>
    public record Storage(TransactionStoragePhase StoragePhase) : TransactionDescription
    {
        public override void Store(Builder builder)
        {
            builder.StoreUint(0x01, 4);
            StoragePhase.Store(builder);
        }
    }

    /// <summary>
    ///     Tick/tock transaction (for special contracts).
    /// </summary>
    public record TickTock(
        bool IsTock,
        TransactionStoragePhase StoragePhase,
        TransactionComputePhase ComputePhase,
        TransactionActionPhase? ActionPhase,
        bool Aborted,
        bool Destroyed
    ) : TransactionDescription
    {
        public override void Store(Builder builder)
        {
            builder.StoreUint(IsTock ? 0x03 : 0x02, 4);
            StoragePhase.Store(builder);
            ComputePhase.Store(builder);

            if (ActionPhase != null)
            {
                builder.StoreBit(true);
                Builder actionBuilder = Builder.BeginCell();
                ActionPhase.Store(actionBuilder);
                builder.StoreRef(actionBuilder.EndCell());
            }
            else
            {
                builder.StoreBit(false);
            }

            builder.StoreBit(Aborted);
            builder.StoreBit(Destroyed);
        }
    }

    /// <summary>
    ///     Split prepare transaction (for sharding).
    /// </summary>
    public record SplitPrepare(
        SplitMergeInfo SplitInfo,
        TransactionStoragePhase? StoragePhase,
        TransactionComputePhase ComputePhase,
        TransactionActionPhase? ActionPhase,
        bool Aborted,
        bool Destroyed
    ) : TransactionDescription
    {
        public override void Store(Builder builder)
        {
            builder.StoreUint(0x04, 4);
            SplitInfo.Store(builder);

            if (StoragePhase != null)
            {
                builder.StoreBit(true);
                StoragePhase.Store(builder);
            }
            else
            {
                builder.StoreBit(false);
            }

            ComputePhase.Store(builder);

            if (ActionPhase != null)
            {
                builder.StoreBit(true);
                Builder actionBuilder = Builder.BeginCell();
                ActionPhase.Store(actionBuilder);
                builder.StoreRef(actionBuilder.EndCell());
            }
            else
            {
                builder.StoreBit(false);
            }

            builder.StoreBit(Aborted);
            builder.StoreBit(Destroyed);
        }
    }

    /// <summary>
    ///     Split install transaction (for sharding).
    /// </summary>
    public record SplitInstall(
        SplitMergeInfo SplitInfo,
        Transaction PrepareTransaction,
        bool Installed
    ) : TransactionDescription
    {
        public override void Store(Builder builder)
        {
            builder.StoreUint(0x05, 4);
            SplitInfo.Store(builder);

            Builder txBuilder = Builder.BeginCell();
            PrepareTransaction.Store(txBuilder);
            builder.StoreRef(txBuilder.EndCell());

            builder.StoreBit(Installed);
        }
    }

    /// <summary>
    ///     Merge prepare transaction (for sharding).
    /// </summary>
    public record MergePrepare(
        SplitMergeInfo SplitInfo,
        TransactionStoragePhase StoragePhase,
        bool Aborted
    ) : TransactionDescription
    {
        public override void Store(Builder builder)
        {
            builder.StoreUint(0x06, 4);
            SplitInfo.Store(builder);
            StoragePhase.Store(builder);
            builder.StoreBit(Aborted);
        }
    }

    /// <summary>
    ///     Merge install transaction (for sharding).
    /// </summary>
    public record MergeInstall(
        SplitMergeInfo SplitInfo,
        Transaction PrepareTransaction,
        TransactionStoragePhase? StoragePhase,
        TransactionCreditPhase? CreditPhase,
        TransactionComputePhase ComputePhase,
        TransactionActionPhase? ActionPhase,
        bool Aborted,
        bool Destroyed
    ) : TransactionDescription
    {
        public override void Store(Builder builder)
        {
            builder.StoreUint(0x07, 4);
            SplitInfo.Store(builder);

            Builder txBuilder = Builder.BeginCell();
            PrepareTransaction.Store(txBuilder);
            builder.StoreRef(txBuilder.EndCell());

            if (StoragePhase != null)
            {
                builder.StoreBit(true);
                StoragePhase.Store(builder);
            }
            else
            {
                builder.StoreBit(false);
            }

            if (CreditPhase != null)
            {
                builder.StoreBit(true);
                CreditPhase.Store(builder);
            }
            else
            {
                builder.StoreBit(false);
            }

            ComputePhase.Store(builder);

            if (ActionPhase != null)
            {
                builder.StoreBit(true);
                Builder actionBuilder = Builder.BeginCell();
                ActionPhase.Store(actionBuilder);
                builder.StoreRef(actionBuilder.EndCell());
            }
            else
            {
                builder.StoreBit(false);
            }

            builder.StoreBit(Aborted);
            builder.StoreBit(Destroyed);
        }
    }
}