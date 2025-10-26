using Ton.Core.Addresses;
using Ton.Core.Boc;

namespace Ton.Core.Tests;

[TestFixture]
public class BuilderTests
{
    [Test]
    public void Test_BeginCell()
    {
        Builder builder = Builder.BeginCell();
        Assert.Multiple(() =>
        {
            Assert.That(builder.Bits, Is.EqualTo(0));
            Assert.That(builder.Refs, Is.EqualTo(0));
        });
    }

    [Test]
    public void Test_StoreBit()
    {
        Builder builder = Builder.BeginCell();
        builder.StoreBit(true);
        builder.StoreBit(false);

        Assert.That(builder.Bits, Is.EqualTo(2));

        Slice slice = builder.AsSlice();
        Assert.That(slice.LoadBit(), Is.True);
        Assert.That(slice.LoadBit(), Is.False);
    }

    [Test]
    public void Test_StoreUint()
    {
        Builder builder = Builder.BeginCell();
        builder.StoreUint(12345, 32);

        Slice slice = builder.AsSlice();
        Assert.That(slice.LoadUint(32), Is.EqualTo(12345));
    }

    [Test]
    public void Test_StoreInt()
    {
        Builder builder = Builder.BeginCell();
        builder.StoreInt(-12345, 32);

        Slice slice = builder.AsSlice();
        Assert.That(slice.LoadInt(32), Is.EqualTo(-12345));
    }

    [Test]
    public void Test_StoreCoins()
    {
        Builder builder = Builder.BeginCell();
        builder.StoreCoins(1000000000);

        Slice slice = builder.AsSlice();
        Assert.That((long)slice.LoadCoins(), Is.EqualTo(1000000000));
    }

    [Test]
    public void Test_StoreAddress()
    {
        Address address = Address.Parse("EQAs9VlT6S776tq3unJcP5Ogsj-ELLunLXuOb1EKcOQi4wJB");

        Builder builder = Builder.BeginCell();
        builder.StoreAddress(address);

        Slice slice = builder.AsSlice();
        Address? loaded = slice.LoadAddress();
        Assert.That(loaded, Is.Not.Null);
        Assert.That(loaded.ToString(), Is.EqualTo(address.ToString()));
    }

    [Test]
    public void Test_StoreRef()
    {
        Cell innerCell = Builder.BeginCell()
            .StoreUint(123, 32)
            .EndCell();

        Builder builder = Builder.BeginCell();
        builder.StoreRef(innerCell);

        Assert.That(builder.Refs, Is.EqualTo(1));

        Slice slice = builder.AsSlice();
        Cell ref1 = slice.LoadRef();
        Assert.That(ref1.BeginParse().LoadUint(32), Is.EqualTo(123));
    }

    [Test]
    public void Test_StoreSlice()
    {
        Cell source = Builder.BeginCell()
            .StoreUint(111, 32)
            .StoreUint(222, 32)
            .EndCell();

        Builder builder = Builder.BeginCell();
        builder.StoreSlice(source.BeginParse());

        Slice slice = builder.AsSlice();
        Assert.That(slice.LoadUint(32), Is.EqualTo(111));
        Assert.That(slice.LoadUint(32), Is.EqualTo(222));
    }

    [Test]
    public void Test_EndCell()
    {
        Cell cell = Builder.BeginCell()
            .StoreUint(12345, 32)
            .EndCell();

        Assert.That(cell, Is.Not.Null);
        Assert.That(cell.Type, Is.EqualTo(CellType.Ordinary));
    }

    [Test]
    public void Test_AvailableBits()
    {
        Builder builder = Builder.BeginCell();
        Assert.That(builder.AvailableBits, Is.EqualTo(1023));

        builder.StoreUint(0, 100);
        Assert.That(builder.AvailableBits, Is.EqualTo(923));
    }

    [Test]
    public void Test_AvailableRefs()
    {
        Builder builder = Builder.BeginCell();
        Assert.That(builder.AvailableRefs, Is.EqualTo(4));

        builder.StoreRef(new Cell());
        Assert.That(builder.AvailableRefs, Is.EqualTo(3));
    }

    [Test]
    public void Test_TooManyRefs_ThrowsException()
    {
        Builder builder = Builder.BeginCell();
        builder.StoreRef(new Cell());
        builder.StoreRef(new Cell());
        builder.StoreRef(new Cell());
        builder.StoreRef(new Cell());

        InvalidOperationException? ex = Assert.Throws<InvalidOperationException>(() => builder.StoreRef(new Cell()));
        Assert.That(ex!.Message, Does.Contain("Too many references"));
    }

    [Test]
    public void Test_StoreMaybeUint()
    {
        Builder builder = Builder.BeginCell();
        builder.StoreMaybeUint(123L, 32);
        builder.StoreMaybeUint(null, 32);

        Slice slice = builder.AsSlice();
        Assert.That(slice.LoadMaybeUint(32), Is.EqualTo(123));
        Assert.That(slice.LoadMaybeUint(32), Is.Null);
    }

    [Test]
    public void Test_StoreMaybeRef()
    {
        Cell innerCell = Builder.BeginCell().StoreUint(789, 32).EndCell();

        Builder builder = Builder.BeginCell();
        builder.StoreMaybeRef(innerCell);
        builder.StoreMaybeRef((Cell?)null);

        Slice slice = builder.AsSlice();
        Cell? ref1 = slice.LoadMaybeRef();
        Assert.That(ref1, Is.Not.Null);
        Assert.That(ref1!.BeginParse().LoadUint(32), Is.EqualTo(789));

        Cell? ref2 = slice.LoadMaybeRef();
        Assert.That(ref2, Is.Null);
    }

    [Test]
    public void Test_RoundTrip_ComplexData()
    {
        Address address = Address.Parse("EQAs9VlT6S776tq3unJcP5Ogsj-ELLunLXuOb1EKcOQi4wJB");

        Builder builder = Builder.BeginCell()
            .StoreUint(100, 32)
            .StoreInt(-50, 32)
            .StoreCoins(1000000)
            .StoreAddress(address)
            .StoreRef(Builder.BeginCell().StoreUint(999, 16).EndCell());

        Slice slice = builder.AsSlice();
        Assert.Multiple(() =>
        {
            Assert.That(slice.LoadUint(32), Is.EqualTo(100));
            Assert.That(slice.LoadInt(32), Is.EqualTo(-50));
            Assert.That((long)slice.LoadCoins(), Is.EqualTo(1000000));
        });
        Address? loadedAddr = slice.LoadAddress();
        Assert.That(loadedAddr, Is.Not.Null);
        Assert.That(loadedAddr.ToString(), Is.EqualTo(address.ToString()));
        Cell ref1 = slice.LoadRef();
        Assert.That(ref1.BeginParse().LoadUint(16), Is.EqualTo(999));
    }
}