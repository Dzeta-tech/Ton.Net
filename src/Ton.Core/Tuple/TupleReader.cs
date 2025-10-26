using System.Numerics;
using Ton.Core.Addresses;
using Ton.Core.Boc;

namespace Ton.Core.Tuple;

/// <summary>
///     Reader for tuple items with type-safe accessors.
/// </summary>
public class TupleReader(TupleItem[] items)
{
    readonly List<TupleItem> items = [..items];

    /// <summary>
    ///     Gets the number of remaining items.
    /// </summary>
    public int Remaining => items.Count;

    /// <summary>
    ///     Peek at the next item without consuming it.
    /// </summary>
    public TupleItem Peek()
    {
        if (items.Count == 0)
            throw new InvalidOperationException("EOF");
        return items[0];
    }

    /// <summary>
    ///     Pop and return the next item.
    /// </summary>
    public TupleItem Pop()
    {
        if (items.Count == 0)
            throw new InvalidOperationException("EOF");
        TupleItem res = items[0];
        items.RemoveAt(0);
        return res;
    }

    /// <summary>
    ///     Skip the specified number of items.
    /// </summary>
    public TupleReader Skip(int num = 1)
    {
        for (int i = 0; i < num; i++)
            Pop();
        return this;
    }

    /// <summary>
    ///     Read a BigInteger value.
    /// </summary>
    public BigInteger ReadBigInteger()
    {
        TupleItem popped = Pop();
        if (popped is not TupleItemInt intItem)
            throw new InvalidOperationException("Not a number");
        return intItem.Value;
    }

    /// <summary>
    ///     Read an optional BigInteger value (null if TupleItemNull).
    /// </summary>
    public BigInteger? ReadBigIntegerOpt()
    {
        TupleItem popped = Pop();
        if (popped is TupleItemNull)
            return null;
        if (popped is not TupleItemInt intItem)
            throw new InvalidOperationException("Not a number");
        return intItem.Value;
    }

    /// <summary>
    ///     Read a number as int/long.
    /// </summary>
    public long ReadNumber()
    {
        return (long)ReadBigInteger();
    }

    /// <summary>
    ///     Read an optional number.
    /// </summary>
    public long? ReadNumberOpt()
    {
        BigInteger? r = ReadBigIntegerOpt();
        return r.HasValue ? (long)r.Value : null;
    }

    /// <summary>
    ///     Read a boolean value (0 = false, non-zero = true).
    /// </summary>
    public bool ReadBoolean()
    {
        long res = ReadNumber();
        return res != 0;
    }

    /// <summary>
    ///     Read an optional boolean value.
    /// </summary>
    public bool? ReadBooleanOpt()
    {
        long? res = ReadNumberOpt();
        return res.HasValue ? res.Value != 0 : null;
    }

    /// <summary>
    ///     Read an address from a cell.
    /// </summary>
    public Address ReadAddress()
    {
        Address? r = ReadCell().BeginParse().LoadAddress();
        if (r == null)
            throw new InvalidOperationException("Not an address");
        return r;
    }

    /// <summary>
    ///     Read an optional address.
    /// </summary>
    public Address? ReadAddressOpt()
    {
        Cell? r = ReadCellOpt();
        return r?.BeginParse().LoadMaybeAddress();
    }

    /// <summary>
    ///     Read a cell (accepts cell, slice, or builder tuple items).
    /// </summary>
    public Cell ReadCell()
    {
        TupleItem popped = Pop();
        return popped switch
        {
            TupleItemCell cellItem => cellItem.Cell,
            TupleItemSlice sliceItem => sliceItem.Cell,
            TupleItemBuilder builderItem => builderItem.Cell,
            _ => throw new InvalidOperationException($"Not a cell: {popped.Type}")
        };
    }

    /// <summary>
    ///     Read an optional cell.
    /// </summary>
    public Cell? ReadCellOpt()
    {
        TupleItem popped = Pop();
        if (popped is TupleItemNull)
            return null;

        return popped switch
        {
            TupleItemCell cellItem => cellItem.Cell,
            TupleItemSlice sliceItem => sliceItem.Cell,
            TupleItemBuilder builderItem => builderItem.Cell,
            _ => throw new InvalidOperationException("Not a cell")
        };
    }

    /// <summary>
    ///     Read a nested tuple.
    /// </summary>
    public TupleReader ReadTuple()
    {
        TupleItem popped = Pop();
        if (popped is not TupleItemTuple tupleItem)
            throw new InvalidOperationException("Not a tuple");
        return new TupleReader(tupleItem.Items);
    }

    /// <summary>
    ///     Read an optional nested tuple.
    /// </summary>
    public TupleReader? ReadTupleOpt()
    {
        TupleItem popped = Pop();
        if (popped is TupleItemNull)
            return null;
        if (popped is not TupleItemTuple tupleItem)
            throw new InvalidOperationException("Not a tuple");
        return new TupleReader(tupleItem.Items);
    }

    /// <summary>
    ///     Read a Lisp-style cons list: (head, (head2, (head3, null)))
    /// </summary>
    public TupleItem[] ReadLispList()
    {
        return ReadLispListInternal(ReadTupleOpt());
    }

    /// <summary>
    ///     Read a Lisp-style cons list directly from current reader.
    /// </summary>
    public TupleItem[] ReadLispListDirect()
    {
        if (items is [TupleItemNull])
            return [];

        return ReadLispListInternal(this);
    }

    /// <summary>
    ///     Read a buffer from a cell (must be byte-aligned, no refs).
    /// </summary>
    public byte[] ReadBuffer()
    {
        Slice s = ReadCell().BeginParse();
        if (s.RemainingRefs != 0)
            throw new InvalidOperationException("Not a buffer");
        if (s.RemainingBits % 8 != 0)
            throw new InvalidOperationException("Not a buffer");
        return s.LoadBuffer(s.RemainingBits / 8);
    }

    /// <summary>
    ///     Read an optional buffer.
    /// </summary>
    public byte[]? ReadBufferOpt()
    {
        Cell? r = ReadCellOpt();
        if (r == null)
            return null;

        Slice s = r.BeginParse();
        if (s.RemainingRefs != 0 || s.RemainingBits % 8 != 0)
            throw new InvalidOperationException("Not a buffer");
        return s.LoadBuffer(s.RemainingBits / 8);
    }

    /// <summary>
    ///     Read a string from a cell.
    /// </summary>
    public string ReadString()
    {
        Slice s = ReadCell().BeginParse();
        return s.LoadStringTail();
    }

    /// <summary>
    ///     Read an optional string.
    /// </summary>
    public string? ReadStringOpt()
    {
        Cell? r = ReadCellOpt();
        if (r == null)
            return null;
        Slice s = r.BeginParse();
        return s.LoadStringTail();
    }

    static TupleItem[] ReadLispListInternal(TupleReader? reader)
    {
        List<TupleItem> result = [];

        TupleReader? tail = reader;
        while (tail != null)
        {
            TupleItem head = tail.Pop();
            if (tail.items.Count == 0 ||
                (tail.items[0] is not TupleItemTuple && tail.items[0] is not TupleItemNull))
                throw new InvalidOperationException(
                    "Lisp list consists only from (any, tuple) elements and ends with null");

            tail = tail.ReadTupleOpt();
            result.Add(head);
        }

        return result.ToArray();
    }
}