using System.Numerics;
using Ton.Core.Boc;

namespace Ton.Core.Dict;

/// <summary>
///     Serializer for TON blockchain dictionaries (binary Patricia trees).
/// </summary>
internal static class DictSerializer
{
    /// <summary>
    ///     Serialize dictionary to Cell.
    /// </summary>
    public static Cell? SerializeDict<V>(System.Collections.Generic.Dictionary<BigInteger, V> src, int keyLength, Action<V, Builder> serializer)
    {
        if (src.Count == 0)
            return null;

        var tree = BuildTree(src, keyLength);
        var builder = new Builder();
        SerializeEdge(tree, keyLength, serializer, builder);
        return builder.EndCell();
    }

    #region Tree Building

    class Node<T>
    {
        public bool IsLeaf { get; set; }
        public T? Value { get; set; }
        public Edge<T>? Left { get; set; }
        public Edge<T>? Right { get; set; }
    }

    class Edge<T>
    {
        public string Label { get; set; } = "";
        public Node<T> Node { get; set; } = null!;
    }

    static Edge<T> BuildTree<T>(System.Collections.Generic.Dictionary<BigInteger, T> src, int keyLength)
    {
        // Convert map keys to binary strings
        var converted = new System.Collections.Generic.Dictionary<string, T>();
        foreach (var (key, value) in src)
        {
            var padded = PadBinary(key, keyLength);
            converted[padded] = value;
        }

        return BuildEdge(converted, 0);
    }

    static Edge<T> BuildEdge<T>(System.Collections.Generic.Dictionary<string, T> src, int prefixLen)
    {
        if (src.Count == 0)
            throw new InvalidOperationException("Internal inconsistency");

        var label = FindCommonPrefix(src.Keys.ToArray(), prefixLen);
        return new Edge<T>
        {
            Label = label,
            Node = BuildNode(src, label.Length + prefixLen)
        };
    }

    static Node<T> BuildNode<T>(System.Collections.Generic.Dictionary<string, T> src, int prefixLen)
    {
        if (src.Count == 0)
            throw new InvalidOperationException("Internal inconsistency");

        if (src.Count == 1)
        {
            return new Node<T>
            {
                IsLeaf = true,
                Value = src.Values.First()
            };
        }

        var (left, right) = ForkMap(src, prefixLen);
        return new Node<T>
        {
            IsLeaf = false,
            Left = BuildEdge(left, prefixLen + 1),
            Right = BuildEdge(right, prefixLen + 1)
        };
    }

    static (System.Collections.Generic.Dictionary<string, T> left, System.Collections.Generic.Dictionary<string, T> right) ForkMap<T>(System.Collections.Generic.Dictionary<string, T> src,
        int prefixLen)
    {
        var left = new System.Collections.Generic.Dictionary<string, T>();
        var right = new System.Collections.Generic.Dictionary<string, T>();

        foreach (var (key, value) in src)
        {
            if (key[prefixLen] == '0')
                left[key] = value;
            else
                right[key] = value;
        }

        if (left.Count == 0)
            throw new InvalidOperationException("Internal inconsistency. Left empty.");
        if (right.Count == 0)
            throw new InvalidOperationException("Internal inconsistency. Right empty.");

        return (left, right);
    }

    static string FindCommonPrefix(string[] keys, int prefixLen)
    {
        if (keys.Length == 0)
            return "";

        if (keys.Length == 1)
            return keys[0].Substring(prefixLen);

        var prefix = "";
        var first = keys[0];

        for (int i = prefixLen; i < first.Length; i++)
        {
            var bit = first[i];
            bool allMatch = keys.All(k => i < k.Length && k[i] == bit);
            if (!allMatch)
                break;

            prefix += bit;
        }

        return prefix;
    }

    #endregion

    #region Serialization

    static void SerializeEdge<V>(Edge<V> edge, int keyLength, Action<V, Builder> serializer, Builder builder)
    {
        // Write label
        WriteLabel(edge.Label, keyLength, builder);

        // Write node
        if (edge.Node.IsLeaf)
        {
            serializer(edge.Node.Value!, builder);
        }
        else
        {
            var leftCell = new Builder();
            SerializeEdge(edge.Node.Left!, keyLength, serializer, leftCell);

            var rightCell = new Builder();
            SerializeEdge(edge.Node.Right!, keyLength, serializer, rightCell);

            builder.StoreRef(leftCell.EndCell());
            builder.StoreRef(rightCell.EndCell());
        }
    }

    static void WriteLabel(string label, int keyLength, Builder builder)
    {
        // Calculate lengths for different encoding types
        int shortLength = LabelShortLength(label);
        int longLength = LabelLongLength(label, keyLength);
        int sameLength = LabelSameLength(label, keyLength);

        // Choose optimal encoding
        if (IsSameBits(label) && sameLength < shortLength && sameLength < longLength)
        {
            WriteLabelSame(label, keyLength, builder);
        }
        else if (shortLength <= longLength)
        {
            WriteLabelShort(label, builder);
        }
        else
        {
            WriteLabelLong(label, keyLength, builder);
        }
    }

    static void WriteLabelShort(string label, Builder builder)
    {
        // hml_short$0 - Header
        builder.StoreBit(false);

        // Unary length
        for (int i = 0; i < label.Length; i++)
            builder.StoreBit(true);
        builder.StoreBit(false);

        // Value
        if (label.Length > 0)
            builder.StoreUint(BigInteger.Parse("0" + label, System.Globalization.NumberStyles.AllowBinarySpecifier),
                label.Length);
    }

    static void WriteLabelLong(string label, int keyLength, Builder builder)
    {
        // hml_long$10 - Header
        builder.StoreBit(true);
        builder.StoreBit(false);

        // Length
        int lengthBits = (int)Math.Ceiling(Math.Log2(keyLength + 1));
        builder.StoreUint(label.Length, lengthBits);

        // Value
        if (label.Length > 0)
            builder.StoreUint(BigInteger.Parse("0" + label, System.Globalization.NumberStyles.AllowBinarySpecifier),
                label.Length);
    }

    static void WriteLabelSame(string label, int keyLength, Builder builder)
    {
        // hml_same$11 - Header
        builder.StoreBit(true);
        builder.StoreBit(true);

        // Bit value
        builder.StoreBit(label.Length > 0 && label[0] == '1');

        // Length
        int lengthBits = (int)Math.Ceiling(Math.Log2(keyLength + 1));
        builder.StoreUint(label.Length, lengthBits);
    }

    static int LabelShortLength(string label)
    {
        return 1 + label.Length + 1 + label.Length;
    }

    static int LabelLongLength(string label, int keyLength)
    {
        return 2 + (int)Math.Ceiling(Math.Log2(keyLength + 1)) + label.Length;
    }

    static int LabelSameLength(string label, int keyLength)
    {
        return 3 + (int)Math.Ceiling(Math.Log2(keyLength + 1));
    }

    static bool IsSameBits(string label)
    {
        if (label.Length == 0)
            return false;

        char first = label[0];
        return label.All(c => c == first);
    }

    static string PadBinary(BigInteger value, int length)
    {
        var binary = "";
        var temp = value;

        // Handle zero specially
        if (value == 0)
        {
            return new string('0', length);
        }

        // Convert to binary
        while (temp > 0)
        {
            binary = (temp % 2 == 0 ? '0' : '1') + binary;
            temp /= 2;
        }

        // Pad to length
        while (binary.Length < length)
        {
            binary = '0' + binary;
        }

        return binary;
    }

    #endregion
}

