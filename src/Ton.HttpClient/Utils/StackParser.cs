using System.Numerics;
using System.Text.Json;
using Ton.Core.Tuple;

namespace Ton.HttpClient.Utils;

/// <summary>
/// Utility for parsing TVM stack from Toncenter API responses.
/// </summary>
public static class StackParser
{
    /// <summary>
    /// Parse stack from API response to TupleReader.
    /// </summary>
    public static TupleReader ParseStack(List<object> stack)
    {
        var items = new List<TupleItem>();
        
        foreach (var item in stack)
        {
            items.Add(ParseStackItem(item));
        }
        
        return new TupleReader(items.ToArray());
    }

    private static TupleItem ParseStackItem(object item)
    {
        // Stack items come as arrays: [type, value]
        if (item is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Array)
        {
            var array = jsonElement.EnumerateArray().ToArray();
            if (array.Length == 0) throw new InvalidOperationException("Empty stack item");

            var type = array[0].GetString();
            
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

    private static TupleItem ParseNum(JsonElement[] array)
    {
        if (array.Length < 2) throw new InvalidOperationException("Invalid num format");
        
        var valueStr = array[1].GetString() ?? throw new InvalidOperationException("Missing num value");
        
        // Handle hex format (0x prefix)
        if (valueStr.StartsWith("0x"))
        {
            valueStr = valueStr.Substring(2);
            return new TupleItemInt(BigInteger.Parse(valueStr, System.Globalization.NumberStyles.HexNumber));
        }
        
        return new TupleItemInt(BigInteger.Parse(valueStr));
    }

    private static TupleItem ParseCell(JsonElement[] array)
    {
        if (array.Length < 2) throw new InvalidOperationException("Invalid cell format");
        
        var base64 = array[1].GetString() ?? throw new InvalidOperationException("Missing cell value");
        var boc = Convert.FromBase64String(base64);
        var cell = Cell.FromBoc(boc)[0];
        
        return new TupleItemCell(cell);
    }

    private static TupleItem ParseSlice(JsonElement[] array)
    {
        if (array.Length < 2) throw new InvalidOperationException("Invalid slice format");
        
        var base64 = array[1].GetString() ?? throw new InvalidOperationException("Missing slice value");
        var boc = Convert.FromBase64String(base64);
        var cell = Cell.FromBoc(boc)[0];
        
        return new TupleItemSlice(cell);
    }

    private static TupleItem ParseBuilder(JsonElement[] array)
    {
        if (array.Length < 2) throw new InvalidOperationException("Invalid builder format");
        
        var base64 = array[1].GetString() ?? throw new InvalidOperationException("Missing builder value");
        var boc = Convert.FromBase64String(base64);
        var cell = Cell.FromBoc(boc)[0];
        
        return new TupleItemBuilder(cell);
    }

    private static TupleItem ParseTuple(JsonElement[] array)
    {
        if (array.Length < 2) throw new InvalidOperationException("Invalid tuple format");
        
        var elements = array[1].GetProperty("elements").EnumerateArray().ToList();
        var items = new List<TupleItem>();
        
        foreach (var element in elements)
        {
            items.Add(ParseStackItem(element));
        }
        
        return new TupleItemTuple(items.ToArray());
    }
}

