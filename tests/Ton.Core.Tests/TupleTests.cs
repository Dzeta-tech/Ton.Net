using System.Numerics;
using System.Text;
using NUnit.Framework;
using Ton.Core.Addresses;
using Ton.Core.Boc;
using Ton.Core.Tuple;
using TonTuple = Ton.Core.Tuple.Tuple;

namespace Ton.Core.Tests;

[TestFixture]
public class TupleTests
{
    // TODO: Add BOC serialization tests when ToBoc/FromBoc are implemented
    [Test]
    public void Test_SerializeTuple_WithNumbers()
    {
        var items = new TupleItem[]
        {
            new TupleItemInt(-1),
            new TupleItemInt(-1),
            new TupleItemInt(49800000000),
            new TupleItemInt(100000000),
            new TupleItemInt(100000000),
            new TupleItemInt(2500),
            new TupleItemInt(100000000)
        };

        var serialized = TonTuple.SerializeTuple(items);
        
        // Verify it creates a cell without throwing
        Assert.That(serialized, Is.Not.Null);
        Assert.That(serialized.Type, Is.EqualTo(CellType.Ordinary));
    }

    [Test]
    public void Test_SerializeTuple_LongNumbers()
    {
        var items = new TupleItem[]
        {
            new TupleItemInt(BigInteger.Parse("12312312312312323421"))
        };

        var serialized = TonTuple.SerializeTuple(items);
        
        // Verify it creates a cell and can be parsed back
        var parsed = TonTuple.ParseTuple(serialized);
        Assert.That(parsed.Length, Is.EqualTo(1));
        Assert.That((parsed[0] as TupleItemInt)?.Value, Is.EqualTo(BigInteger.Parse("12312312312312323421")));
    }

    [Test]
    public void Test_SerializeTuple_Address()
    {
        var addr = new Address(-1, new byte[32]); // Simple test address (masterchain)
        var items = new TupleItem[]
        {
            new TupleItemSlice(
                Builder.BeginCell()
                    .StoreAddress(addr)
                    .EndCell()
            )
        };

        var serialized = TonTuple.SerializeTuple(items);
        var parsed = TonTuple.ParseTuple(serialized);
        
        Assert.That(parsed.Length, Is.EqualTo(1));
        var sliceItem = parsed[0] as TupleItemSlice;
        Assert.That(sliceItem, Is.Not.Null);
        var readAddr = sliceItem!.Cell.BeginParse().LoadAddress();
        Assert.That(readAddr?.ToString(), Is.EqualTo(addr.ToString()));
    }

    [Test]
    public void Test_SerializeAndParse_Tuples_RoundTrip()
    {
        var originalItems = new TupleItem[]
        {
            new TupleItemInt(1),
            new TupleItemInt(2),
            new TupleItemInt(3),
            TupleItemNull.Instance,
            new TupleItemInt(-1),
            new TupleItemInt(123),
            new TupleItemInt(456)
        };

        var serialized = TonTuple.SerializeTuple(originalItems);
        var parsed = TonTuple.ParseTuple(serialized);

        Assert.That(parsed.Length, Is.EqualTo(originalItems.Length));
        
        for (int i = 0; i < originalItems.Length; i++)
        {
            if (originalItems[i] is TupleItemInt origInt && parsed[i] is TupleItemInt parsedInt)
            {
                Assert.That(parsedInt.Value, Is.EqualTo(origInt.Value));
            }
            else if (originalItems[i] is TupleItemNull && parsed[i] is TupleItemNull)
            {
                // Both are null, OK
                Assert.Pass();
            }
            else
            {
                Assert.Fail($"Mismatch at index {i}");
            }
        }
    }

    [Test]
    public void Test_TupleReader_ReadBigInteger()
    {
        var items = new TupleItem[]
        {
            new TupleItemInt(123),
            new TupleItemInt(-456),
            TupleItemNull.Instance
        };

        var reader = new TupleReader(items);
        Assert.That(reader.ReadBigInteger(), Is.EqualTo(new BigInteger(123)));
        Assert.That(reader.ReadBigInteger(), Is.EqualTo(new BigInteger(-456)));
        Assert.That(reader.ReadBigIntegerOpt(), Is.Null);
    }

    [Test]
    public void Test_TupleReader_ReadNumber()
    {
        var items = new TupleItem[]
        {
            new TupleItemInt(42),
            TupleItemNull.Instance
        };

        var reader = new TupleReader(items);
        Assert.That(reader.ReadNumber(), Is.EqualTo(42L));
        Assert.That(reader.ReadNumberOpt(), Is.Null);
    }

    [Test]
    public void Test_TupleReader_ReadBoolean()
    {
        var items = new TupleItem[]
        {
            new TupleItemInt(-1), // true
            new TupleItemInt(0), // false
            new TupleItemInt(5), // true (non-zero)
            TupleItemNull.Instance
        };

        var reader = new TupleReader(items);
        Assert.That(reader.ReadBoolean(), Is.True);
        Assert.That(reader.ReadBoolean(), Is.False);
        Assert.That(reader.ReadBoolean(), Is.True);
        Assert.That(reader.ReadBooleanOpt(), Is.Null);
    }

    [Test]
    public void Test_TupleReader_ReadAddress()
    {
        var addr = new Address(0, new byte[32]); // Simple test address
        var items = new TupleItem[]
        {
            new TupleItemSlice(Builder.BeginCell().StoreAddress(addr).EndCell()),
            TupleItemNull.Instance
        };

        var reader = new TupleReader(items);
        var read1 = reader.ReadAddress();
        Assert.That(read1.ToString(), Is.EqualTo(addr.ToString()));

        var read2 = reader.ReadAddressOpt();
        Assert.That(read2, Is.Null);
    }

    [Test]
    public void Test_TupleReader_ReadCell()
    {
        var cell = Builder.BeginCell().StoreUint(123, 32).EndCell();
        var items = new TupleItem[]
        {
            new TupleItemCell(cell),
            new TupleItemSlice(cell),
            new TupleItemBuilder(cell),
            TupleItemNull.Instance
        };

        var reader = new TupleReader(items);
        Assert.That(reader.ReadCell().Hash().SequenceEqual(cell.Hash()), Is.True);
        Assert.That(reader.ReadCell().Hash().SequenceEqual(cell.Hash()), Is.True);
        Assert.That(reader.ReadCell().Hash().SequenceEqual(cell.Hash()), Is.True);
        Assert.That(reader.ReadCellOpt(), Is.Null);
    }

    [Test]
    public void Test_TupleReader_ReadTuple()
    {
        var nested = new TupleItem[] { new TupleItemInt(99) };
        var items = new TupleItem[]
        {
            new TupleItemTuple(nested),
            TupleItemNull.Instance
        };

        var reader = new TupleReader(items);
        var nestedReader = reader.ReadTuple();
        Assert.That(nestedReader.ReadNumber(), Is.EqualTo(99L));

        var nullReader = reader.ReadTupleOpt();
        Assert.That(nullReader, Is.Null);
    }

    [Test]
    public void Test_TupleReader_ReadLispList()
    {
        // Cons list: (1, (2, (3, null)))
        var cons = new TupleItem[]
        {
            new TupleItemTuple(
            [
                new TupleItemInt(1),
                new TupleItemTuple(
                [
                    new TupleItemInt(2),
                    new TupleItemTuple(
                    [
                        new TupleItemInt(3),
                        TupleItemNull.Instance
                    ])
                ])
            ])
        };

        var reader = new TupleReader(cons);
        var list = reader.ReadLispList();

        Assert.That(list.Length, Is.EqualTo(3));
        Assert.That((list[0] as TupleItemInt)?.Value, Is.EqualTo(new BigInteger(1)));
        Assert.That((list[1] as TupleItemInt)?.Value, Is.EqualTo(new BigInteger(2)));
        Assert.That((list[2] as TupleItemInt)?.Value, Is.EqualTo(new BigInteger(3)));
    }

    [Test]
    public void Test_TupleReader_ReadLispListDirect_Empty()
    {
        var cons = new TupleItem[] { TupleItemNull.Instance };
        var reader = new TupleReader(cons);
        var list = reader.ReadLispListDirect();
        Assert.That(list.Length, Is.EqualTo(0));
    }

    [Test]
    public void Test_TupleReader_ReadLispList_Empty()
    {
        var cons = new TupleItem[] { TupleItemNull.Instance };
        var reader = new TupleReader(cons);
        var list = reader.ReadLispList();
        Assert.That(list.Length, Is.EqualTo(0));
    }

    [Test]
    public void Test_TupleReader_ReadLispList_InvalidThrows()
    {
        var cons = new TupleItem[] { new TupleItemInt(1) };
        var reader = new TupleReader(cons);

        Assert.Throws<InvalidOperationException>(() => reader.ReadLispListDirect());
    }

    [Test]
    public void Test_TupleReader_ReadBuffer()
    {
        var buffer = new byte[] { 1, 2, 3, 4, 5 };
        var items = new TupleItem[]
        {
            new TupleItemSlice(Builder.BeginCell().StoreBuffer(buffer).EndCell()),
            TupleItemNull.Instance
        };

        var reader = new TupleReader(items);
        var read1 = reader.ReadBuffer();
        Assert.That(read1.SequenceEqual(buffer), Is.True);

        var read2 = reader.ReadBufferOpt();
        Assert.That(read2, Is.Null);
    }

    [Test]
    public void Test_TupleReader_ReadString()
    {
        var str = "Hello, TON!";
        var items = new TupleItem[]
        {
            new TupleItemSlice(Builder.BeginCell().StoreStringTail(str).EndCell()),
            TupleItemNull.Instance
        };

        var reader = new TupleReader(items);
        var read1 = reader.ReadString();
        Assert.That(read1, Is.EqualTo(str));

        var read2 = reader.ReadStringOpt();
        Assert.That(read2, Is.Null);
    }

    [Test]
    public void Test_TupleReader_Skip()
    {
        var items = new TupleItem[]
        {
            new TupleItemInt(1),
            new TupleItemInt(2),
            new TupleItemInt(3)
        };

        var reader = new TupleReader(items);
        reader.Skip(2);
        Assert.That(reader.ReadNumber(), Is.EqualTo(3L));
        Assert.That(reader.Remaining, Is.EqualTo(0));
    }

    [Test]
    public void Test_TupleReader_Peek()
    {
        var items = new TupleItem[] { new TupleItemInt(42) };
        var reader = new TupleReader(items);

        var peeked = reader.Peek();
        Assert.That((peeked as TupleItemInt)?.Value, Is.EqualTo(new BigInteger(42)));
        Assert.That(reader.Remaining, Is.EqualTo(1)); // Not consumed

        var popped = reader.Pop();
        Assert.That((popped as TupleItemInt)?.Value, Is.EqualTo(new BigInteger(42)));
        Assert.That(reader.Remaining, Is.EqualTo(0)); // Consumed
    }

    [Test]
    public void Test_TupleBuilder_WriteNumber()
    {
        var builder = new TupleBuilder();
        builder.WriteNumber(123L);
        builder.WriteNumber(456L);
        builder.WriteNumber((long?)null);

        var items = builder.Build();
        Assert.That(items.Length, Is.EqualTo(3));
        Assert.That((items[0] as TupleItemInt)?.Value, Is.EqualTo(new BigInteger(123)));
        Assert.That((items[1] as TupleItemInt)?.Value, Is.EqualTo(new BigInteger(456)));
        Assert.That(items[2], Is.InstanceOf<TupleItemNull>());
    }

    [Test]
    public void Test_TupleBuilder_WriteBoolean()
    {
        var builder = new TupleBuilder();
        builder.WriteBoolean(true);
        builder.WriteBoolean(false);
        builder.WriteBoolean(null);

        var items = builder.Build();
        Assert.That((items[0] as TupleItemInt)?.Value, Is.EqualTo(new BigInteger(-1)));
        Assert.That((items[1] as TupleItemInt)?.Value, Is.EqualTo(BigInteger.Zero));
        Assert.That(items[2], Is.InstanceOf<TupleItemNull>());
    }

    [Test]
    public void Test_TupleBuilder_WriteAddress()
    {
        var addr = new Address(0, new byte[32]); // Simple test address
        var builder = new TupleBuilder();
        builder.WriteAddress(addr);
        builder.WriteAddress(null);

        var items = builder.Build();
        Assert.That(items.Length, Is.EqualTo(2));
        Assert.That(items[0], Is.InstanceOf<TupleItemSlice>());
        Assert.That(items[1], Is.InstanceOf<TupleItemNull>());

        var reader = new TupleReader(items);
        var readAddr = reader.ReadAddress();
        Assert.That(readAddr.ToString(), Is.EqualTo(addr.ToString()));
    }

    [Test]
    public void Test_TupleBuilder_WriteCell()
    {
        var cell = Builder.BeginCell().StoreUint(789, 32).EndCell();
        var builder = new TupleBuilder();
        builder.WriteCell(cell);
        builder.WriteCell((Cell?)null);

        var items = builder.Build();
        Assert.That(items.Length, Is.EqualTo(2));
        Assert.That(items[0], Is.InstanceOf<TupleItemCell>());
        Assert.That(items[1], Is.InstanceOf<TupleItemNull>());
    }

    [Test]
    public void Test_TupleBuilder_WriteString()
    {
        var builder = new TupleBuilder();
        builder.WriteString("test");
        builder.WriteString(null);

        var items = builder.Build();
        Assert.That(items.Length, Is.EqualTo(2));
        Assert.That(items[0], Is.InstanceOf<TupleItemSlice>());
        Assert.That(items[1], Is.InstanceOf<TupleItemNull>());

        var reader = new TupleReader(items);
        Assert.That(reader.ReadString(), Is.EqualTo("test"));
    }

    [Test]
    public void Test_TupleBuilder_WriteBuffer()
    {
        var buffer = new byte[] { 10, 20, 30 };
        var builder = new TupleBuilder();
        builder.WriteBuffer(buffer);
        builder.WriteBuffer(null);

        var items = builder.Build();
        Assert.That(items.Length, Is.EqualTo(2));
        Assert.That(items[0], Is.InstanceOf<TupleItemSlice>());
        Assert.That(items[1], Is.InstanceOf<TupleItemNull>());

        var reader = new TupleReader(items);
        var read = reader.ReadBuffer();
        Assert.That(read.SequenceEqual(buffer), Is.True);
    }

    [Test]
    public void Test_TupleBuilder_WriteTuple()
    {
        var nested = new TupleItem[] { new TupleItemInt(999) };
        var builder = new TupleBuilder();
        builder.WriteTuple(nested);
        builder.WriteTuple(null);

        var items = builder.Build();
        Assert.That(items.Length, Is.EqualTo(2));
        Assert.That(items[0], Is.InstanceOf<TupleItemTuple>());
        Assert.That(items[1], Is.InstanceOf<TupleItemNull>());
    }
}

