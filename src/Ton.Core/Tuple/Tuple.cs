using System.Numerics;
using Ton.Core.Boc;

namespace Ton.Core.Tuple;

/// <summary>
///     Tuple serialization and parsing functions for TON VM stack values.
/// </summary>
public static class Tuple
{
    static readonly BigInteger Int64Min = BigInteger.Parse("-9223372036854775808");
    static readonly BigInteger Int64Max = BigInteger.Parse("9223372036854775807");

    /// <summary>
    ///     Serialize a tuple item to a builder.
    /// </summary>
    public static void SerializeTupleItem(TupleItem src, Builder builder)
    {
        switch (src)
        {
            case TupleItemNull:
                builder.StoreUint(0x00, 8);
                break;

            case TupleItemInt intItem:
                if (intItem.Value <= Int64Max && intItem.Value >= Int64Min)
                {
                    builder.StoreUint(0x01, 8);
                    builder.StoreInt(intItem.Value, 64);
                }
                else
                {
                    builder.StoreUint(0x0100, 15);
                    builder.StoreInt(intItem.Value, 257);
                }

                break;

            case TupleItemNaN:
                builder.StoreInt(0x02ff, 16);
                break;

            case TupleItemCell cellItem:
                builder.StoreUint(0x03, 8);
                builder.StoreRef(cellItem.Cell);
                break;

            case TupleItemSlice sliceItem:
                builder.StoreUint(0x04, 8);
                builder.StoreUint(0, 10); // start_bits
                builder.StoreUint(sliceItem.Cell.Bits.Length, 10); // end_bits
                builder.StoreUint(0, 3); // start_refs
                builder.StoreUint(sliceItem.Cell.Refs.Length, 3); // end_refs
                builder.StoreRef(sliceItem.Cell);
                break;

            case TupleItemBuilder builderItem:
                builder.StoreUint(0x05, 8);
                builder.StoreRef(builderItem.Cell);
                break;

            case TupleItemTuple tupleItem:
                Cell? head = null;
                Cell? tail = null;

                for (int i = 0; i < tupleItem.Items.Length; i++)
                {
                    // Swap
                    Cell? s = head;
                    head = tail;
                    tail = s;

                    if (i > 1)
                        head = Builder.BeginCell()
                            .StoreRef(tail!)
                            .StoreRef(head!)
                            .EndCell();

                    Builder bc = Builder.BeginCell();
                    SerializeTupleItem(tupleItem.Items[i], bc);
                    tail = bc.EndCell();
                }

                builder.StoreUint(0x07, 8);
                builder.StoreUint(tupleItem.Items.Length, 16);
                if (head != null)
                    builder.StoreRef(head);
                if (tail != null)
                    builder.StoreRef(tail);
                break;

            default:
                throw new ArgumentException($"Invalid tuple item type: {src.GetType().Name}");
        }
    }

    /// <summary>
    ///     Parse a tuple item from a slice.
    /// </summary>
    public static TupleItem ParseTupleItem(Slice cs)
    {
        int kind = (int)cs.LoadUint(8);

        switch (kind)
        {
            case 0:
                return TupleItemNull.Instance;

            case 1:
                return new TupleItemInt(cs.LoadIntBig(64));

            case 2:
            {
                if (cs.LoadUint(7) == 0)
                    return new TupleItemInt(cs.LoadIntBig(257));
                cs.LoadBit(); // must eq 1
                return TupleItemNaN.Instance;
            }

            case 3:
                return new TupleItemCell(cs.LoadRef());

            case 4:
            {
                int startBits = (int)cs.LoadUint(10);
                int endBits = (int)cs.LoadUint(10);
                int startRefs = (int)cs.LoadUint(3);
                int endRefs = (int)cs.LoadUint(3);

                // Copy to new cell
                Slice rs = cs.LoadRef().BeginParse();
                rs.Skip(startBits);
                BitString dt = rs.LoadBits(endBits - startBits);

                Builder builder = Builder.BeginCell()
                    .StoreBits(dt);

                // Copy refs if exist
                if (startRefs < endRefs)
                {
                    for (int i = 0; i < startRefs; i++)
                        rs.LoadRef();
                    for (int i = 0; i < endRefs - startRefs; i++)
                        builder.StoreRef(rs.LoadRef());
                }

                return new TupleItemSlice(builder.EndCell());
            }

            case 5:
                return new TupleItemBuilder(cs.LoadRef());

            case 7:
            {
                int length = (int)cs.LoadUint(16);
                List<TupleItem> items = [];

                if (length > 1)
                {
                    Slice head = cs.LoadRef().BeginParse();
                    Slice tail = cs.LoadRef().BeginParse();
                    items.Insert(0, ParseTupleItem(tail));

                    for (int i = 0; i < length - 2; i++)
                    {
                        Slice ohead = head;
                        head = ohead.LoadRef().BeginParse();
                        tail = ohead.LoadRef().BeginParse();
                        items.Insert(0, ParseTupleItem(tail));
                    }

                    items.Insert(0, ParseTupleItem(head));
                }
                else if (length == 1)
                {
                    items.Add(ParseTupleItem(cs.LoadRef().BeginParse()));
                }

                return new TupleItemTuple(items.ToArray());
            }

            default:
                throw new ArgumentException($"Unsupported stack item: {kind}");
        }
    }

    /// <summary>
    ///     Serialize a tuple to a cell.
    /// </summary>
    public static Cell SerializeTuple(TupleItem[] src)
    {
        Builder builder = Builder.BeginCell();
        builder.StoreUint(src.Length, 24);
        TupleItem[] r = src.ToArray();
        SerializeTupleTail(r, builder);
        return builder.EndCell();
    }

    /// <summary>
    ///     Parse a tuple from a cell.
    /// </summary>
    public static TupleItem[] ParseTuple(Cell src)
    {
        List<TupleItem> res = [];
        Slice cs = src.BeginParse();
        int size = (int)cs.LoadUint(24);

        for (int i = 0; i < size; i++)
        {
            Cell next = cs.LoadRef();
            res.Insert(0, ParseTupleItem(cs));
            cs = next.BeginParse();
        }

        return res.ToArray();
    }

    static void SerializeTupleTail(TupleItem[] src, Builder builder)
    {
        if (src.Length > 0)
        {
            // rest:^(VmStackList n)
            Builder tail = Builder.BeginCell();
            SerializeTupleTail(src[..^1], tail);
            builder.StoreRef(tail.EndCell());

            // tos
            SerializeTupleItem(src[^1], builder);
        }
    }
}