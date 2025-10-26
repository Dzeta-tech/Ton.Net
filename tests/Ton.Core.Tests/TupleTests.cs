using System.Numerics;
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
        TupleItem[] items =
        [
            new TupleItemInt(-1),
            new TupleItemInt(-1),
            new TupleItemInt(49800000000),
            new TupleItemInt(100000000),
            new TupleItemInt(100000000),
            new TupleItemInt(2500),
            new TupleItemInt(100000000)
        ];

        Cell serialized = TonTuple.SerializeTuple(items);

        // Verify it creates a cell without throwing
        Assert.That(serialized, Is.Not.Null);
        Assert.That(serialized.Type, Is.EqualTo(CellType.Ordinary));
    }

    [Test]
    public void Test_SerializeTuple_LongNumbers()
    {
        TupleItem[] items =
        [
            new TupleItemInt(BigInteger.Parse("12312312312312323421"))
        ];

        Cell serialized = TonTuple.SerializeTuple(items);

        // Verify it creates a cell and can be parsed back
        TupleItem[] parsed = TonTuple.ParseTuple(serialized);
        Assert.Multiple(() =>
        {
            Assert.That(parsed.Length, Is.EqualTo(1));
            Assert.That((parsed[0] as TupleItemInt)?.Value, Is.EqualTo(BigInteger.Parse("12312312312312323421")));
        });
    }

    [Test]
    public void Test_SerializeTuple_Address()
    {
        Address addr = new(-1, new byte[32]); // Simple test address (masterchain)
        TupleItem[] items =
        [
            new TupleItemSlice(
                Builder.BeginCell()
                    .StoreAddress(addr)
                    .EndCell()
            )
        ];

        Cell serialized = TonTuple.SerializeTuple(items);
        TupleItem[] parsed = TonTuple.ParseTuple(serialized);

        Assert.That(parsed.Length, Is.EqualTo(1));
        TupleItemSlice? sliceItem = parsed[0] as TupleItemSlice;
        Assert.That(sliceItem, Is.Not.Null);
        Address? readAddr = sliceItem!.Cell.BeginParse().LoadAddress();
        Assert.That(readAddr?.ToString(), Is.EqualTo(addr.ToString()));
    }

    [Test]
    public void Test_SerializeAndParse_Tuples_RoundTrip()
    {
        TupleItem[] originalItems =
        [
            new TupleItemInt(1),
            new TupleItemInt(2),
            new TupleItemInt(3),
            TupleItemNull.Instance,
            new TupleItemInt(-1),
            new TupleItemInt(123),
            new TupleItemInt(456)
        ];

        Cell serialized = TonTuple.SerializeTuple(originalItems);
        TupleItem[] parsed = TonTuple.ParseTuple(serialized);

        Assert.That(parsed.Length, Is.EqualTo(originalItems.Length));

        for (int i = 0; i < originalItems.Length; i++)
            if (originalItems[i] is TupleItemInt origInt && parsed[i] is TupleItemInt parsedInt)
                Assert.That(parsedInt.Value, Is.EqualTo(origInt.Value));
            else if (originalItems[i] is TupleItemNull && parsed[i] is TupleItemNull)
                // Both are null, OK
                Assert.Pass();
            else
                Assert.Fail($"Mismatch at index {i}");
    }

    [Test]
    public void Test_TupleReader_ReadBigInteger()
    {
        TupleItem[] items =
        [
            new TupleItemInt(123),
            new TupleItemInt(-456),
            TupleItemNull.Instance
        ];

        TupleReader reader = new(items);
        Assert.That(reader.ReadBigInteger(), Is.EqualTo(new BigInteger(123)));
        Assert.Multiple(() =>
        {
            Assert.That(reader.ReadBigInteger(), Is.EqualTo(new BigInteger(-456)));
            Assert.That(reader.ReadBigIntegerOpt(), Is.Null);
        });
    }

    [Test]
    public void Test_TupleReader_ReadNumber()
    {
        TupleItem[] items =
        [
            new TupleItemInt(42),
            TupleItemNull.Instance
        ];

        TupleReader reader = new(items);
        Assert.Multiple(() =>
        {
            Assert.That(reader.ReadNumber(), Is.EqualTo(42L));
            Assert.That(reader.ReadNumberOpt(), Is.Null);
        });
    }

    [Test]
    public void Test_TupleReader_ReadBoolean()
    {
        TupleItem[] items =
        [
            new TupleItemInt(-1), // true
            new TupleItemInt(0), // false
            new TupleItemInt(5), // true (non-zero)
            TupleItemNull.Instance
        ];

        TupleReader reader = new(items);
        Assert.That(reader.ReadBoolean(), Is.True);
        Assert.That(reader.ReadBoolean(), Is.False);
        Assert.Multiple(() =>
        {
            Assert.That(reader.ReadBoolean(), Is.True);
            Assert.That(reader.ReadBooleanOpt(), Is.Null);
        });
    }

    [Test]
    public void Test_TupleReader_ReadAddress()
    {
        Address addr = new(0, new byte[32]); // Simple test address
        TupleItem[] items =
        [
            new TupleItemSlice(Builder.BeginCell().StoreAddress(addr).EndCell()),
            TupleItemNull.Instance
        ];

        TupleReader reader = new(items);
        Address read1 = reader.ReadAddress();
        Assert.That(read1.ToString(), Is.EqualTo(addr.ToString()));

        Address? read2 = reader.ReadAddressOpt();
        Assert.That(read2, Is.Null);
    }

    [Test]
    public void Test_TupleReader_ReadCell()
    {
        Cell cell = Builder.BeginCell().StoreUint(123, 32).EndCell();
        TupleItem[] items =
        [
            new TupleItemCell(cell),
            new TupleItemSlice(cell),
            new TupleItemBuilder(cell),
            TupleItemNull.Instance
        ];

        TupleReader reader = new(items);
        Assert.That(reader.ReadCell().Hash().SequenceEqual(cell.Hash()), Is.True);
        Assert.That(reader.ReadCell().Hash().SequenceEqual(cell.Hash()), Is.True);
        Assert.Multiple(() =>
        {
            Assert.That(reader.ReadCell().Hash().SequenceEqual(cell.Hash()), Is.True);
            Assert.That(reader.ReadCellOpt(), Is.Null);
        });
    }

    [Test]
    public void Test_TupleReader_ReadTuple()
    {
        TupleItem[] nested = [new TupleItemInt(99)];
        TupleItem[] items =
        [
            new TupleItemTuple(nested),
            TupleItemNull.Instance
        ];

        TupleReader reader = new(items);
        TupleReader nestedReader = reader.ReadTuple();
        Assert.That(nestedReader.ReadNumber(), Is.EqualTo(99L));

        TupleReader? nullReader = reader.ReadTupleOpt();
        Assert.That(nullReader, Is.Null);
    }

    [Test]
    public void Test_TupleReader_ReadLispList()
    {
        // Cons list: (1, (2, (3, null)))
        TupleItem[] cons =
        [
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
        ];

        TupleReader reader = new(cons);
        TupleItem[] list = reader.ReadLispList();

        Assert.Multiple(() =>
        {
            Assert.That(list.Length, Is.EqualTo(3));
            Assert.That((list[0] as TupleItemInt)?.Value, Is.EqualTo(new BigInteger(1)));
            Assert.That((list[1] as TupleItemInt)?.Value, Is.EqualTo(new BigInteger(2)));
            Assert.That((list[2] as TupleItemInt)?.Value, Is.EqualTo(new BigInteger(3)));
        });
    }

    [Test]
    public void Test_TupleReader_ReadLispListDirect_Empty()
    {
        TupleItem[] cons = [TupleItemNull.Instance];
        TupleReader reader = new(cons);
        TupleItem[] list = reader.ReadLispListDirect();
        Assert.That(list.Length, Is.EqualTo(0));
    }

    [Test]
    public void Test_TupleReader_ReadLispList_Empty()
    {
        TupleItem[] cons = [TupleItemNull.Instance];
        TupleReader reader = new(cons);
        TupleItem[] list = reader.ReadLispList();
        Assert.That(list.Length, Is.EqualTo(0));
    }

    [Test]
    public void Test_TupleReader_ReadLispList_InvalidThrows()
    {
        TupleItem[] cons = [new TupleItemInt(1)];
        TupleReader reader = new(cons);

        Assert.Throws<InvalidOperationException>(() => reader.ReadLispListDirect());
    }

    [Test]
    public void Test_TupleReader_ReadBuffer()
    {
        byte[] buffer = [1, 2, 3, 4, 5];
        TupleItem[] items =
        [
            new TupleItemSlice(Builder.BeginCell().StoreBuffer(buffer).EndCell()),
            TupleItemNull.Instance
        ];

        TupleReader reader = new(items);
        byte[] read1 = reader.ReadBuffer();
        Assert.That(read1.SequenceEqual(buffer), Is.True);

        byte[]? read2 = reader.ReadBufferOpt();
        Assert.That(read2, Is.Null);
    }

    [Test]
    public void Test_TupleReader_ReadString()
    {
        string str = "Hello, TON!";
        TupleItem[] items =
        [
            new TupleItemSlice(Builder.BeginCell().StoreStringTail(str).EndCell()),
            TupleItemNull.Instance
        ];

        TupleReader reader = new(items);
        string read1 = reader.ReadString();
        Assert.That(read1, Is.EqualTo(str));

        string? read2 = reader.ReadStringOpt();
        Assert.That(read2, Is.Null);
    }

    [Test]
    public void Test_TupleReader_Skip()
    {
        TupleItem[] items =
        [
            new TupleItemInt(1),
            new TupleItemInt(2),
            new TupleItemInt(3)
        ];

        TupleReader reader = new(items);
        reader.Skip(2);
        Assert.Multiple(() =>
        {
            Assert.That(reader.ReadNumber(), Is.EqualTo(3L));
            Assert.That(reader.Remaining, Is.EqualTo(0));
        });
    }

    [Test]
    public void Test_TupleReader_Peek()
    {
        TupleItem[] items = [new TupleItemInt(42)];
        TupleReader reader = new(items);

        TupleItem peeked = reader.Peek();
        Assert.Multiple(() =>
        {
            Assert.That((peeked as TupleItemInt)?.Value, Is.EqualTo(new BigInteger(42)));
            Assert.That(reader.Remaining, Is.EqualTo(1)); // Not consumed
        });

        TupleItem popped = reader.Pop();
        Assert.Multiple(() =>
        {
            Assert.That((popped as TupleItemInt)?.Value, Is.EqualTo(new BigInteger(42)));
            Assert.That(reader.Remaining, Is.EqualTo(0)); // Consumed
        });
    }

    [Test]
    public void Test_TupleBuilder_WriteNumber()
    {
        TupleBuilder builder = new();
        builder.WriteNumber(123L);
        builder.WriteNumber(456L);
        builder.WriteNumber(null);

        TupleItem[] items = builder.Build();
        Assert.Multiple(() =>
        {
            Assert.That(items.Length, Is.EqualTo(3));
            Assert.That((items[0] as TupleItemInt)?.Value, Is.EqualTo(new BigInteger(123)));
            Assert.That((items[1] as TupleItemInt)?.Value, Is.EqualTo(new BigInteger(456)));
            Assert.That(items[2], Is.InstanceOf<TupleItemNull>());
        });
    }

    [Test]
    public void Test_TupleBuilder_WriteBoolean()
    {
        TupleBuilder builder = new();
        builder.WriteBoolean(true);
        builder.WriteBoolean(false);
        builder.WriteBoolean(null);

        TupleItem[] items = builder.Build();
        Assert.Multiple(() =>
        {
            Assert.That((items[0] as TupleItemInt)?.Value, Is.EqualTo(new BigInteger(-1)));
            Assert.That((items[1] as TupleItemInt)?.Value, Is.EqualTo(BigInteger.Zero));
            Assert.That(items[2], Is.InstanceOf<TupleItemNull>());
        });
    }

    [Test]
    public void Test_TupleBuilder_WriteAddress()
    {
        Address addr = new(0, new byte[32]); // Simple test address
        TupleBuilder builder = new();
        builder.WriteAddress(addr);
        builder.WriteAddress(null);

        TupleItem[] items = builder.Build();
        Assert.That(items.Length, Is.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(items[0], Is.InstanceOf<TupleItemSlice>());
            Assert.That(items[1], Is.InstanceOf<TupleItemNull>());
        });

        TupleReader reader = new(items);
        Address readAddr = reader.ReadAddress();
        Assert.That(readAddr.ToString(), Is.EqualTo(addr.ToString()));
    }

    [Test]
    public void Test_TupleBuilder_WriteCell()
    {
        Cell cell = Builder.BeginCell().StoreUint(789, 32).EndCell();
        TupleBuilder builder = new();
        builder.WriteCell(cell);
        builder.WriteCell((Cell?)null);

        TupleItem[] items = builder.Build();
        Assert.That(items.Length, Is.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(items[0], Is.InstanceOf<TupleItemCell>());
            Assert.That(items[1], Is.InstanceOf<TupleItemNull>());
        });
    }

    [Test]
    public void Test_TupleBuilder_WriteString()
    {
        TupleBuilder builder = new();
        builder.WriteString("test");
        builder.WriteString(null);

        TupleItem[] items = builder.Build();
        Assert.That(items.Length, Is.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(items[0], Is.InstanceOf<TupleItemSlice>());
            Assert.That(items[1], Is.InstanceOf<TupleItemNull>());
        });

        TupleReader reader = new(items);
        Assert.That(reader.ReadString(), Is.EqualTo("test"));
    }

    [Test]
    public void Test_TupleBuilder_WriteBuffer()
    {
        byte[] buffer = [10, 20, 30];
        TupleBuilder builder = new();
        builder.WriteBuffer(buffer);
        builder.WriteBuffer(null);

        TupleItem[] items = builder.Build();
        Assert.That(items.Length, Is.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(items[0], Is.InstanceOf<TupleItemSlice>());
            Assert.That(items[1], Is.InstanceOf<TupleItemNull>());
        });

        TupleReader reader = new(items);
        byte[] read = reader.ReadBuffer();
        Assert.That(read.SequenceEqual(buffer), Is.True);
    }

    [Test]
    public void Test_TupleBuilder_WriteTuple()
    {
        TupleItem[] nested = [new TupleItemInt(999)];
        TupleBuilder builder = new();
        builder.WriteTuple(nested);
        builder.WriteTuple(null);

        TupleItem[] items = builder.Build();
        Assert.That(items.Length, Is.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(items[0], Is.InstanceOf<TupleItemTuple>());
            Assert.That(items[1], Is.InstanceOf<TupleItemNull>());
        });
    }
}