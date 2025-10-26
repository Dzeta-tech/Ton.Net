using System.Numerics;
using Ton.Core.Boc;
using TonDict = Ton.Core.Dict;

namespace Ton.Core.Types;

/// <summary>
///     Extra currencies (non-TON currencies like jettons).
///     Source:
///     https://github.com/ton-blockchain/ton/blob/24dc184a2ea67f9c47042b4104bbb4d82289fac1/crypto/block/block.tlb#L120
///     extra_currencies$_ dict:(HashmapE 32 (VarUInteger 32)) = ExtraCurrencyCollection;
/// </summary>
public static class ExtraCurrency
{
    /// <summary>
    ///     Creates an empty extra currency dictionary.
    /// </summary>
    public static TonDict.Dictionary<TonDict.DictKeyUint, BigInteger> Create()
    {
        return TonDict.Dictionary<TonDict.DictKeyUint, BigInteger>.Empty(
            TonDict.DictionaryKeys.Uint(32),
            TonDict.DictionaryValues.BigVarUint(5) // log2(32) = 5
        );
    }

    /// <summary>
    ///     Packs extra currency dictionary to a cell.
    /// </summary>
    public static Cell Pack(TonDict.Dictionary<TonDict.DictKeyUint, BigInteger> dict)
    {
        return Builder.BeginCell()
            .StoreDictDirect(dict)
            .EndCell();
    }

    /// <summary>
    ///     Unpacks extra currency dictionary from a cell.
    /// </summary>
    public static TonDict.Dictionary<TonDict.DictKeyUint, BigInteger> Unpack(Cell cell)
    {
        return TonDict.Dictionary<TonDict.DictKeyUint, BigInteger>.LoadDirect(
            TonDict.DictionaryKeys.Uint(32),
            TonDict.DictionaryValues.BigVarUint(5),
            cell.BeginParse()
        );
    }
}