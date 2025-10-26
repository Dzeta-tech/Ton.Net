using System.Numerics;
using Ton.Core.Boc;
using TonDict = Ton.Core.Dict;

namespace Ton.Core.Types;

/// <summary>
///     Represents a currency collection containing TON coins and optional extra currencies.
///     Source:
///     https://github.com/ton-blockchain/ton/blob/24dc184a2ea67f9c47042b4104bbb4d82289fac1/crypto/block/block.tlb#L120
///     extra_currencies$_ dict:(HashmapE 32 (VarUInteger 32)) = ExtraCurrencyCollection;
///     currencies$_ grams:Grams other:ExtraCurrencyCollection = CurrencyCollection;
/// </summary>
public record CurrencyCollection
{
    /// <summary>
    ///     Creates a new CurrencyCollection.
    /// </summary>
    public CurrencyCollection(BigInteger coins, TonDict.Dictionary<TonDict.DictKeyUint, BigInteger>? other = null)
    {
        Coins = coins;
        Other = other;
    }

    /// <summary>
    ///     TON coins (in nanotons).
    /// </summary>
    public BigInteger Coins { get; init; }

    /// <summary>
    ///     Optional extra currencies dictionary.
    ///     Maps currency ID (uint32) to amount (VarUInteger 32).
    /// </summary>
    public TonDict.Dictionary<TonDict.DictKeyUint, BigInteger>? Other { get; init; }

    /// <summary>
    ///     Loads a CurrencyCollection from a Slice.
    /// </summary>
    /// <param name="slice">The slice to load from.</param>
    /// <returns>A new CurrencyCollection instance.</returns>
    public static CurrencyCollection Load(Slice slice)
    {
        BigInteger coins = slice.LoadCoins();

        // Load extra currencies dictionary (HashmapE 32 (VarUInteger 32))
        TonDict.Dictionary<TonDict.DictKeyUint, BigInteger>? other = slice.LoadDict(
            TonDict.DictionaryKeys.Uint(32),
            TonDict.DictionaryValues.BigVarUint(5) // log2(32) = 5
        );

        // JS SDK treats empty dictionary as undefined
        if (other is { Size: 0 }) other = null;

        return new CurrencyCollection(coins, other);
    }

    /// <summary>
    ///     Stores the CurrencyCollection into a Builder.
    /// </summary>
    /// <param name="builder">The builder to store into.</param>
    public void Store(Builder builder)
    {
        builder.StoreCoins(Coins);

        if (Other != null)
            builder.StoreDict(Other);
        else
            builder.StoreBit(false); // No extra currencies
    }
}