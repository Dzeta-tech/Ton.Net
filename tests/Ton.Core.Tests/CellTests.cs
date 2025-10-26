using Ton.Core.Boc;

namespace Ton.Core.Tests;

[TestFixture]
public class CellTests
{
    [Test]
    public void Test_Construct_EmptyCell()
    {
        var cell = new Cell();
        Assert.That(cell.Type, Is.EqualTo(CellType.Ordinary));
        Assert.That(cell.Bits.Equals(BitString.Empty), Is.True);
        Assert.That(cell.Refs.Length, Is.EqualTo(0));
        Assert.That(cell.IsExotic, Is.False);
    }

    [Test]
    public void Test_Construct_WithBits()
    {
        var builder = new BitBuilder();
        builder.WriteUint(123, 32);
        var bits = builder.Build();
        
        var cell = new Cell(bits);
        Assert.That(cell.Bits.Equals(bits), Is.True);
        Assert.That(cell.Refs.Length, Is.EqualTo(0));
    }

    [Test]
    public void Test_Construct_WithRefs()
    {
        var ref1 = new Cell();
        var ref2 = new Cell();
        
        var cell = new Cell(null, [ref1, ref2]);
        Assert.That(cell.Refs.Length, Is.EqualTo(2));
        Assert.That(cell.Refs[0], Is.EqualTo(ref1));
        Assert.That(cell.Refs[1], Is.EqualTo(ref2));
    }

    [Test]
    public void Test_TooManyRefs_ThrowsException()
    {
        var refs = new Cell[5];
        for (int i = 0; i < 5; i++)
        {
            refs[i] = new Cell();
        }
        
        var ex = Assert.Throws<ArgumentException>(() => new Cell(null, refs));
        Assert.That(ex!.Message, Does.Contain("Invalid number of references"));
    }

    [Test]
    public void Test_TooManyBits_ThrowsException()
    {
        var builder = new BitBuilder();
        for (int i = 0; i < 1024; i++)
        {
            builder.WriteBit(true);
        }
        
        var ex = Assert.Throws<ArgumentException>(() => new Cell(builder.Build()));
        Assert.That(ex!.Message, Does.Contain("Bits overflow"));
    }

    [Test]
    public void Test_Hash_IsDeterministic()
    {
        var builder = new BitBuilder();
        builder.WriteUint(12345, 32);
        var bits = builder.Build();
        
        var cell1 = new Cell(bits);
        var cell2 = new Cell(bits);
        
        Assert.That(cell1.Hash().SequenceEqual(cell2.Hash()), Is.True);
    }

    [Test]
    public void Test_Equals()
    {
        var builder = new BitBuilder();
        builder.WriteUint(12345, 32);
        var bits = builder.Build();
        
        var cell1 = new Cell(bits);
        var cell2 = new Cell(bits);
        
        Assert.That(cell1.Equals(cell2), Is.True);
    }

    [Test]
    public void Test_BeginParse()
    {
        var builder = new BitBuilder();
        builder.WriteUint(123, 32);
        var cell = new Cell(builder.Build());
        
        var slice = cell.BeginParse();
        Assert.That(slice.LoadUint(32), Is.EqualTo(123));
    }

    [Test]
    public void Test_AsSlice()
    {
        var builder = new BitBuilder();
        builder.WriteUint(456, 32);
        var cell = new Cell(builder.Build());
        
        var slice = cell.AsSlice();
        Assert.That(slice.LoadUint(32), Is.EqualTo(456));
    }

    [Test]
    public void Test_Depth_EmptyCell()
    {
        var cell = new Cell();
        Assert.That(cell.Depth(), Is.EqualTo(0));
    }

    [Test]
    public void Test_Depth_WithRefs()
    {
        var ref1 = new Cell();
        var ref2 = Builder.BeginCell()
            .StoreRef(new Cell())
            .EndCell();
        
        var cell = new Cell(null, [ref1, ref2]);
        Assert.That(cell.Depth(), Is.GreaterThan(0));
    }

    [Test]
    public void Test_ToString()
    {
        var cell = new Cell();
        var str = cell.ToString();
        Assert.That(str, Does.Contain("x{"));
    }
}

