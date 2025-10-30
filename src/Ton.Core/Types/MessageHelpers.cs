using System.Numerics;
using Ton.Core.Addresses;
using Ton.Core.Boc;
using TonDict = Ton.Core.Dict;

namespace Ton.Core.Types;

public static class MessageHelpers
{
    public static MessageRelaxed Internal(
        Address to,
        BigInteger valueNano,
        Cell? body = null,
        bool bounce = true,
        StateInit? init = null, TonDict.Dictionary<TonDict.DictKeyUint, BigInteger>? extraCurrencies = null
    )
    {
        CurrencyCollection currency = extraCurrencies == null
            ? new CurrencyCollection(valueNano)
            : new CurrencyCollection(valueNano, extraCurrencies);

        return new MessageRelaxed(
            new CommonMessageInfoRelaxed.Internal(
                true,
                bounce,
                false,
                null,
                to,
                currency,
                0,
                0,
                0,
                0
            ),
            body ?? Builder.BeginCell().EndCell(),
            init
        );
    }

    public static Cell Comment(string text)
    {
        return Builder.BeginCell()
            .StoreUint(0, 32)
            .StoreStringTail(text)
            .EndCell();
    }
}