using System.Globalization;
using System.Numerics;
using System.Text.Json;
using Ton.Core.Tuple;

namespace Ton.HttpClient.Utils;

/// <summary>
///     Utility for parsing TVM stack from Toncenter API responses.
/// </summary>
public static class StackParser
{
    /// <summary>
    ///     Parse stack from API response to TupleReader.
    /// </summary>
    public static TupleReader ParseStack(List<object> stack)
    {
        List<TupleItem> items = new();

        foreach (object item in stack) items.Add(ParseStackItem(item));

        return new TupleReader(items.ToArray());
    }

    static TupleItem ParseStackItem(object item)
    {
        // Stack items come as arrays: [type, value]
        if (item is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Array)
        {
            JsonElement[] array = jsonElement.EnumerateArray().ToArray();
            if (array.Length == 0) throw new InvalidOperationException("Empty stack item");

            string? type = array[0].GetString();

            return type switch
            {
                "num" => ParseNum(array),
                "cell" => ParseCell(array),
                "slice" => ParseSlice(array),
                "builder" => ParseBuilder(array),
                "nan" => TupleItemNaN.Instance,
                "null" => TupleItemNull.Instance,
                "tuple" => ParseTuple(array),
                _ => throw new NotSupportedException($"Stack item type '{type}' not supported")
            };
        }

        throw new InvalidOperationException("Invalid stack item format");
    }

    static TupleItem ParseNum(JsonElement[] array)
    {
        if (array.Length < 2) throw new InvalidOperationException("Invalid num format");

        string valueStr = array[1].GetString() ?? throw new InvalidOperationException("Missing num value");

        // Handle hex format (0x prefix)
        if (valueStr.StartsWith("0x"))
        {
            valueStr = valueStr.Substring(2);
            return new TupleItemInt(BigInteger.Parse(valueStr, NumberStyles.HexNumber));
        }

        return new TupleItemInt(BigInteger.Parse(valueStr));
    }

    static TupleItem ParseCell(JsonElement[] array)
    {
        if (array.Length < 2) throw new InvalidOperationException("Invalid cell format");

        string base64 = array[1].GetString() ?? throw new InvalidOperationException("Missing cell value");
        byte[] boc = Convert.FromBase64String(base64);
        Cell cell = Cell.FromBoc(boc)[0];

        return new TupleItemCell(cell);
    }

    static TupleItem ParseSlice(JsonElement[] array)
    {
        if (array.Length < 2) throw new InvalidOperationException("Invalid slice format");

        string base64 = array[1].GetString() ?? throw new InvalidOperationException("Missing slice value");
        byte[] boc = Convert.FromBase64String(base64);
        Cell cell = Cell.FromBoc(boc)[0];

        return new TupleItemSlice(cell);
    }

    static TupleItem ParseBuilder(JsonElement[] array)
    {
        if (array.Length < 2) throw new InvalidOperationException("Invalid builder format");

        string base64 = array[1].GetString() ?? throw new InvalidOperationException("Missing builder value");
        byte[] boc = Convert.FromBase64String(base64);
        Cell cell = Cell.FromBoc(boc)[0];

        return new TupleItemBuilder(cell);
    }

    static TupleItem ParseTuple(JsonElement[] array)
    {
        if (array.Length < 2) throw new InvalidOperationException("Invalid tuple format");

        List<JsonElement> elements = array[1].GetProperty("elements").EnumerateArray().ToList();
        List<TupleItem> items = new();

        foreach (JsonElement element in elements) items.Add(ParseStackItem(element));

        return new TupleItemTuple(items.ToArray());
    }
}