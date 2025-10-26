using System.Numerics;
using Ton.Core.Addresses;
using Ton.Core.Boc;

namespace Ton.Core.Types;

/// <summary>
///     Common message info variants for TON messages.
///     Source:
///     https://github.com/ton-blockchain/ton/blob/24dc184a2ea67f9c47042b4104bbb4d82289fac1/crypto/block/block.tlb#L123
/// </summary>
public abstract record CommonMessageInfo
{
    /// <summary>
    ///     Loads CommonMessageInfo from a slice.
    /// </summary>
    public static CommonMessageInfo Load(Slice slice)
    {
        // Read first bit to determine message type
        if (!slice.LoadBit())
        {
            // Internal message (starts with 0)
            bool ihrDisabled = slice.LoadBit();
            bool bounce = slice.LoadBit();
            bool bounced = slice.LoadBit();
            Address src = slice.LoadAddress()!;
            Address dest = slice.LoadAddress()!;
            CurrencyCollection value = CurrencyCollection.Load(slice);
            BigInteger ihrFee = slice.LoadCoins();
            BigInteger forwardFee = slice.LoadCoins();
            BigInteger createdLt = slice.LoadUintBig(64);
            uint createdAt = (uint)slice.LoadUint(32);

            return new Internal(
                ihrDisabled,
                bounce,
                bounced,
                src,
                dest,
                value,
                ihrFee,
                forwardFee,
                createdLt,
                createdAt
            );
        }

        // External message - read second bit
        if (!slice.LoadBit())
        {
            // External In message (starts with 10)
            ExternalAddress? src = slice.LoadMaybeExternalAddress();
            Address dest = slice.LoadAddress()!;
            BigInteger importFee = slice.LoadCoins();

            return new ExternalIn(src, dest, importFee);
        }

        // External Out message (starts with 11)
        Address srcOut = slice.LoadAddress()!;
        ExternalAddress? destOut = slice.LoadMaybeExternalAddress();
        BigInteger createdLtOut = slice.LoadUintBig(64);
        uint createdAtOut = (uint)slice.LoadUint(32);

        return new ExternalOut(srcOut, destOut, createdLtOut, createdAtOut);
    }

    /// <summary>
    ///     Stores CommonMessageInfo to a builder.
    /// </summary>
    public void Store(Builder builder)
    {
        switch (this)
        {
            case Internal i:
                builder.StoreBit(false); // 0 for internal
                builder.StoreBit(i.IhrDisabled);
                builder.StoreBit(i.Bounce);
                builder.StoreBit(i.Bounced);
                builder.StoreAddress(i.Src);
                builder.StoreAddress(i.Dest);
                i.Value.Store(builder);
                builder.StoreCoins(i.IhrFee);
                builder.StoreCoins(i.ForwardFee);
                builder.StoreUint(i.CreatedLt, 64);
                builder.StoreUint(i.CreatedAt, 32);
                break;

            case ExternalIn ei:
                builder.StoreBit(true); // 1
                builder.StoreBit(false); // 0 -> 10 for external-in
                builder.StoreAddress(ei.Src);
                builder.StoreAddress(ei.Dest);
                builder.StoreCoins(ei.ImportFee);
                break;

            case ExternalOut eo:
                builder.StoreBit(true); // 1
                builder.StoreBit(true); // 1 -> 11 for external-out
                builder.StoreAddress(eo.Src);
                builder.StoreAddress(eo.Dest);
                builder.StoreUint(eo.CreatedLt, 64);
                builder.StoreUint(eo.CreatedAt, 32);
                break;

            default:
                throw new InvalidOperationException($"Unknown CommonMessageInfo type: {GetType()}");
        }
    }

    /// <summary>
    ///     Internal message (between contracts on TON).
    ///     int_msg_info$0 ihr_disabled:Bool bounce:Bool bounced:Bool
    ///     src:MsgAddressInt dest:MsgAddressInt
    ///     value:CurrencyCollection ihr_fee:Grams fwd_fee:Grams
    ///     created_lt:uint64 created_at:uint32 = CommonMsgInfo;
    /// </summary>
    public record Internal(
        bool IhrDisabled,
        bool Bounce,
        bool Bounced,
        Address Src,
        Address Dest,
        CurrencyCollection Value,
        BigInteger IhrFee,
        BigInteger ForwardFee,
        BigInteger CreatedLt,
        uint CreatedAt
    ) : CommonMessageInfo;

    /// <summary>
    ///     External incoming message (from outside TON to a contract).
    ///     ext_in_msg_info$10 src:MsgAddressExt dest:MsgAddressInt
    ///     import_fee:Grams = CommonMsgInfo;
    /// </summary>
    public record ExternalIn(
        ExternalAddress? Src,
        Address Dest,
        BigInteger ImportFee
    ) : CommonMessageInfo;

    /// <summary>
    ///     External outgoing message (from a contract to outside TON).
    ///     ext_out_msg_info$11 src:MsgAddressInt dest:MsgAddressExt
    ///     created_lt:uint64 created_at:uint32 = CommonMsgInfo;
    /// </summary>
    public record ExternalOut(
        Address Src,
        ExternalAddress? Dest,
        BigInteger CreatedLt,
        uint CreatedAt
    ) : CommonMessageInfo;
}