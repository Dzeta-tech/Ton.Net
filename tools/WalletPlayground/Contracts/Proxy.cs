// Auto-generated from Tact ABI
// DO NOT EDIT MANUALLY

using Ton.Core.Boc;
using Ton.Core.Addresses;
using Ton.Core.Types;
using Ton.Core.Contracts;
using System.Numerics;

namespace WalletPlayground.Contracts;

public record ProxyMessage(
    Address To,
    BigInteger Value,
    Cell? Body,
    bool Bounce,
    Cell? Code,
    Cell? Data
)
{
    public const uint OpCode = 0x00B59D21;

    public void Store(Builder builder)
    {
        builder.StoreUint(OpCode, 32);
        builder.StoreAddress(To);
        builder.StoreInt(Value, 257);
        if (Body != null)
        {
            builder.StoreBit(true);
            builder.StoreRef(Body);
        }
        else
        {
            builder.StoreBit(false);
        }
        builder.StoreBit(Bounce);
        if (Code != null)
        {
            builder.StoreBit(true);
            builder.StoreRef(Code);
        }
        else
        {
            builder.StoreBit(false);
        }
        if (Data != null)
        {
            builder.StoreBit(true);
            builder.StoreRef(Data);
        }
        else
        {
            builder.StoreBit(false);
        }
    }

    public static ProxyMessage Load(Slice slice)
    {
        var opcode = slice.LoadUint(32);
        if (opcode != OpCode) throw new InvalidOperationException($"Invalid opcode: {opcode}");

        var to = slice.LoadAddress()!;
        var value = slice.LoadIntBig(257);
        var hasBody = slice.LoadBit();
        Cell? body = null;
        if (hasBody)
        {
            body = slice.LoadRef();
        }
        var bounce = slice.LoadBit();
        var hasCode = slice.LoadBit();
        Cell? code = null;
        if (hasCode)
        {
            code = slice.LoadRef();
        }
        var hasData = slice.LoadBit();
        Cell? data = null;
        if (hasData)
        {
            data = slice.LoadRef();
        }

        return new ProxyMessage(to, value, body, bounce, code, data);
    }
}

public class Proxy : IContract
{
    public static readonly Cell Code = Cell.FromBoc(Convert.FromBase64String("te6ccgEBAgEAogABhv8AII66MAHQctch0gDSAPpAIRA0UGZvBPhhAvhi7UTQ+kDT/1lsEjACkVvg1w0f8uCCAYIItZ0huuMCW/LAguHyyAsBALT6QIEBAdcA9ATSAPQE9AQwggCOk/hCUAjHBRfy9BBFEDSBAKBDFBBGVSLIz4WAygDPhEDOAfoCgGnPQAJcbgFusJNbz4GdWM+GgM+EgPQA9ADPgeL0AMkB+wA="))[0];

    public Address Address { get; }
    public StateInit? Init { get; }
    public ContractABI? ABI => null; // TODO: Implement ABI

    public Proxy(Address address, StateInit? init = null)
    {
        Address = address;
        Init = init;
    }

    public static Proxy Create(Address owner, BigInteger invoiceId)
    {
        var builder = Builder.BeginCell();
        builder.StoreAddress(owner);
        builder.StoreUint(invoiceId, 256);
        var dataCell = builder.EndCell();
        var init = new StateInit(
            code: Code,
            data: dataCell
        );
        var address = ContractAddress.From(0, init);
        return new Proxy(address, init);
    }

    /// <summary>
    /// Creates a message body for sending ProxyMessage to this contract.
    /// Use this with your wallet's CreateTransfer method.
    /// </summary>
    public Cell CreateProxyMessageBody(ProxyMessage message)
    {
        var body = Builder.BeginCell();
        message.Store(body);
        return body.EndCell();
    }

}
