using Ton.Core.Boc;

namespace Ton.Core.Tests;

[TestFixture]
public class CellTests
{
    [Test]
    public void Test_Construct_EmptyCell()
    {
        Cell cell = new();
        Assert.Multiple(() =>
        {
            Assert.That(cell.Type, Is.EqualTo(CellType.Ordinary));
            Assert.That(cell.Bits.Equals(BitString.Empty), Is.True);
            Assert.That(cell.Refs.Length, Is.EqualTo(0));
            Assert.That(cell.IsExotic, Is.False);
        });
    }

    [Test]
    public void Test_Construct_WithBits()
    {
        BitBuilder builder = new();
        builder.WriteUint(123, 32);
        BitString bits = builder.Build();

        Cell cell = new(bits);
        Assert.Multiple(() =>
        {
            Assert.That(cell.Bits.Equals(bits), Is.True);
            Assert.That(cell.Refs.Length, Is.EqualTo(0));
        });
    }

    [Test]
    public void Test_Construct_WithRefs()
    {
        Cell ref1 = new();
        Cell ref2 = new();

        Cell cell = new(null, [ref1, ref2]);
        Assert.That(cell.Refs, Has.Length.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(cell.Refs[0], Is.EqualTo(ref1));
            Assert.That(cell.Refs[1], Is.EqualTo(ref2));
        });
    }

    [Test]
    public void Test_TooManyRefs_ThrowsException()
    {
        Cell[] refs = new Cell[5];
        for (int i = 0; i < 5; i++) refs[i] = new Cell();

        ArgumentException? ex = Assert.Throws<ArgumentException>(() => new Cell(null, refs));
        Assert.That(ex!.Message, Does.Contain("Invalid number of references"));
    }

    [Test]
    public void Test_TooManyBits_ThrowsException()
    {
        BitBuilder builder = new();
        for (int i = 0; i < 1024; i++) builder.WriteBit(true);

        ArgumentException? ex = Assert.Throws<ArgumentException>(() => new Cell(builder.Build()));
        Assert.That(ex!.Message, Does.Contain("Bits overflow"));
    }

    [Test]
    public void Test_Hash_IsDeterministic()
    {
        BitBuilder builder = new();
        builder.WriteUint(12345, 32);
        BitString bits = builder.Build();

        Cell cell1 = new(bits);
        Cell cell2 = new(bits);

        Assert.That(cell1.Hash().SequenceEqual(cell2.Hash()), Is.True);
    }

    [Test]
    public void Test_Equals()
    {
        BitBuilder builder = new();
        builder.WriteUint(12345, 32);
        BitString bits = builder.Build();

        Cell cell1 = new(bits);
        Cell cell2 = new(bits);

        Assert.That(cell1.Equals(cell2), Is.True);
    }

    [Test]
    public void Test_BeginParse()
    {
        BitBuilder builder = new();
        builder.WriteUint(123, 32);
        Cell cell = new(builder.Build());

        Slice slice = cell.BeginParse();
        Assert.That(slice.LoadUint(32), Is.EqualTo(123));
    }

    [Test]
    public void Test_AsSlice()
    {
        BitBuilder builder = new();
        builder.WriteUint(456, 32);
        Cell cell = new(builder.Build());

        Slice slice = cell.AsSlice();
        Assert.That(slice.LoadUint(32), Is.EqualTo(456));
    }

    [Test]
    public void Test_Depth_EmptyCell()
    {
        Cell cell = new();
        Assert.That(cell.Depth(), Is.EqualTo(0));
    }

    [Test]
    public void Test_Depth_WithRefs()
    {
        Cell ref1 = new();
        Cell ref2 = Builder.BeginCell()
            .StoreRef(new Cell())
            .EndCell();

        Cell cell = new(null, [ref1, ref2]);
        Assert.That(cell.Depth(), Is.GreaterThan(0));
    }

    [Test]
    public void Test_ToString()
    {
        Cell cell = new();
        string str = cell.ToString();
        Assert.That(str, Does.Contain("x{"));
    }
}