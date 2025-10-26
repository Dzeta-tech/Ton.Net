using System.Numerics;
using Ton.Core.Boc;

namespace Ton.Core.Dict;

/// <summary>
///     Parser for TON blockchain dictionaries (binary Patricia trees).
/// </summary>
internal static class DictParser
{
    /// <summary>
    ///     Parse dictionary from slice.
    /// </summary>
    public static System.Collections.Generic.Dictionary<BigInteger, V> ParseDict<V>(Slice? slice, int keySize, Func<Slice, V> extractor)
    {
        var result = new System.Collections.Generic.Dictionary<BigInteger, V>();
        if (slice != null)
        {
            DoParse("", slice, keySize, result, extractor);
        }

        return result;
    }

    static void DoParse<V>(string prefix, Slice slice, int n, System.Collections.Generic.Dictionary<BigInteger, V> res,
        Func<Slice, V> extractor)
    {
        // Reading label
        int lb0 = slice.LoadBit() ? 1 : 0;
        int prefixLength = 0;
        string pp = prefix;

        if (lb0 == 0)
        {
            // hml_short$0 - Short label detected
            // Unary length encoding
            prefixLength = ReadUnaryLength(slice);

            // Read prefix bits
            for (int i = 0; i < prefixLength; i++)
            {
                pp += slice.LoadBit() ? '1' : '0';
            }
        }
        else
        {
            int lb1 = slice.LoadBit() ? 1 : 0;
            if (lb1 == 0)
            {
                // hml_long$10 - Long label detected
                prefixLength = (int)slice.LoadUint((int)Math.Ceiling(Math.Log2(n + 1)));
                for (int i = 0; i < prefixLength; i++)
                {
                    pp += slice.LoadBit() ? '1' : '0';
                }
            }
            else
            {
                // hml_same$11 - Same label detected (repeated bit)
                string bit = slice.LoadBit() ? "1" : "0";
                prefixLength = (int)slice.LoadUint((int)Math.Ceiling(Math.Log2(n + 1)));
                for (int i = 0; i < prefixLength; i++)
                {
                    pp += bit;
                }
            }
        }

        if (n - prefixLength == 0)
        {
            // Leaf node - extract value
            res[BigInteger.Parse("0" + pp, System.Globalization.NumberStyles.AllowBinarySpecifier)] = extractor(slice);
        }
        else
        {
            // Fork node - traverse left and right
            Cell left = slice.LoadRef();
            Cell right = slice.LoadRef();

            // NOTE: Left and right branches implicitly contain prefixes '0' and '1'
            if (!left.IsExotic)
            {
                DoParse(pp + "0", left.BeginParse(), n - prefixLength - 1, res, extractor);
            }

            if (!right.IsExotic)
            {
                DoParse(pp + "1", right.BeginParse(), n - prefixLength - 1, res, extractor);
            }
        }
    }

    static int ReadUnaryLength(Slice slice)
    {
        int res = 0;
        while (slice.LoadBit())
        {
            res++;
        }

        return res;
    }
}

