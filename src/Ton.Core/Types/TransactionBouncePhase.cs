using System.Numerics;
using Ton.Core.Boc;

namespace Ton.Core.Types;

/// <summary>
///     Transaction bounce phase (union type).
///     Source:
///     https://github.com/ton-blockchain/ton/blob/24dc184a2ea67f9c47042b4104bbb4d82289fac1/crypto/block/block.tlb#L318
///     tr_phase_bounce_negfunds$00 = TrBouncePhase;
///     tr_phase_bounce_nofunds$01 msg_size:StorageUsedShort req_fwd_fees:Grams = TrBouncePhase;
///     tr_phase_bounce_ok$1 msg_size:StorageUsedShort msg_fees:Grams fwd_fees:Grams = TrBouncePhase;
/// </summary>
public abstract record TransactionBouncePhase
{
    /// <summary>
    ///     Loads TransactionBouncePhase from a Slice.
    /// </summary>
    public static TransactionBouncePhase Load(Slice slice)
    {
        // Ok (1 bit)
        if (slice.LoadBit())
        {
            StorageUsed messageSize = StorageUsed.Load(slice);
            BigInteger messageFees = slice.LoadCoins();
            BigInteger forwardFees = slice.LoadCoins();
            return new Ok(messageSize, messageFees, forwardFees);
        }

        // No funds (01)
        if (slice.LoadBit())
        {
            StorageUsed messageSize = StorageUsed.Load(slice);
            BigInteger requiredForwardFees = slice.LoadCoins();
            return new NoFunds(messageSize, requiredForwardFees);
        }

        // Negative funds (00)
        return new NegativeFunds();
    }

    /// <summary>
    ///     Stores TransactionBouncePhase into a Builder.
    /// </summary>
    public abstract void Store(Builder builder);

    /// <summary>
    ///     Bounce succeeded.
    /// </summary>
    public record Ok(StorageUsed MessageSize, BigInteger MessageFees, BigInteger ForwardFees) : TransactionBouncePhase
    {
        public override void Store(Builder builder)
        {
            builder.StoreBit(true);
            MessageSize.Store(builder);
            builder.StoreCoins(MessageFees);
            builder.StoreCoins(ForwardFees);
        }
    }

    /// <summary>
    ///     No funds for bounce.
    /// </summary>
    public record NoFunds(StorageUsed MessageSize, BigInteger RequiredForwardFees) : TransactionBouncePhase
    {
        public override void Store(Builder builder)
        {
            builder.StoreBit(false);
            builder.StoreBit(true);
            MessageSize.Store(builder);
            builder.StoreCoins(RequiredForwardFees);
        }
    }

    /// <summary>
    ///     Negative funds (cannot bounce).
    /// </summary>
    public record NegativeFunds : TransactionBouncePhase
    {
        public override void Store(Builder builder)
        {
            builder.StoreBit(false);
            builder.StoreBit(false);
        }
    }
}