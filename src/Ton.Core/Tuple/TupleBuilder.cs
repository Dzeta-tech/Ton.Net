using System.Numerics;
using Ton.Core.Addresses;
using Ton.Core.Boc;

namespace Ton.Core.Tuple;

/// <summary>
///     Builder for creating tuple items.
/// </summary>
public class TupleBuilder
{
    readonly List<TupleItem> tuple = [];

    /// <summary>
    ///     Write a number (or null).
    /// </summary>
    public void WriteNumber(BigInteger? v)
    {
        if (v == null)
            tuple.Add(TupleItemNull.Instance);
        else
            tuple.Add(new TupleItemInt(v.Value));
    }

    /// <summary>
    ///     Write a number (or null).
    /// </summary>
    public void WriteNumber(long? v)
    {
        if (v == null)
            tuple.Add(TupleItemNull.Instance);
        else
            tuple.Add(new TupleItemInt(v.Value));
    }

    /// <summary>
    ///     Write a boolean (or null). true = -1, false = 0.
    /// </summary>
    public void WriteBoolean(bool? v)
    {
        if (v == null)
            tuple.Add(TupleItemNull.Instance);
        else
            tuple.Add(new TupleItemInt(v.Value ? -1 : 0));
    }

    /// <summary>
    ///     Write a buffer (or null) as a slice.
    /// </summary>
    public void WriteBuffer(byte[]? v)
    {
        if (v == null)
            tuple.Add(TupleItemNull.Instance);
        else
            tuple.Add(new TupleItemSlice(Builder.BeginCell().StoreBuffer(v).EndCell()));
    }

    /// <summary>
    ///     Write a string (or null) as a slice.
    /// </summary>
    public void WriteString(string? v)
    {
        if (v == null)
            tuple.Add(TupleItemNull.Instance);
        else
            tuple.Add(new TupleItemSlice(Builder.BeginCell().StoreStringTail(v).EndCell()));
    }

    /// <summary>
    ///     Write a cell (or null).
    /// </summary>
    public void WriteCell(Cell? v)
    {
        if (v == null)
            tuple.Add(TupleItemNull.Instance);
        else
            tuple.Add(new TupleItemCell(v));
    }

    /// <summary>
    ///     Write a cell or slice (or null) as a cell.
    /// </summary>
    public void WriteCell(Slice? v)
    {
        if (v == null)
            tuple.Add(TupleItemNull.Instance);
        else
            tuple.Add(new TupleItemCell(v.AsCell()));
    }

    /// <summary>
    ///     Write a slice (or null).
    /// </summary>
    public void WriteSlice(Cell? v)
    {
        if (v == null)
            tuple.Add(TupleItemNull.Instance);
        else
            tuple.Add(new TupleItemSlice(v));
    }

    /// <summary>
    ///     Write a slice (or null).
    /// </summary>
    public void WriteSlice(Slice? v)
    {
        if (v == null)
            tuple.Add(TupleItemNull.Instance);
        else
            tuple.Add(new TupleItemSlice(v.AsCell()));
    }

    /// <summary>
    ///     Write a builder (or null).
    /// </summary>
    public void WriteBuilder(Cell? v)
    {
        if (v == null)
            tuple.Add(TupleItemNull.Instance);
        else
            tuple.Add(new TupleItemBuilder(v));
    }

    /// <summary>
    ///     Write a builder (or null).
    /// </summary>
    public void WriteBuilder(Slice? v)
    {
        if (v == null)
            tuple.Add(TupleItemNull.Instance);
        else
            tuple.Add(new TupleItemBuilder(v.AsCell()));
    }

    /// <summary>
    ///     Write a tuple (or null).
    /// </summary>
    public void WriteTuple(TupleItem[]? v)
    {
        if (v == null)
            tuple.Add(TupleItemNull.Instance);
        else
            tuple.Add(new TupleItemTuple(v));
    }

    /// <summary>
    ///     Write an address (or null) as a slice.
    /// </summary>
    public void WriteAddress(Address? v)
    {
        if (v == null)
            tuple.Add(TupleItemNull.Instance);
        else
            tuple.Add(new TupleItemSlice(Builder.BeginCell().StoreAddress(v).EndCell()));
    }

    /// <summary>
    ///     Build and return the tuple items array.
    /// </summary>
    public TupleItem[] Build()
    {
        return tuple.ToArray();
    }
}