using Ton.Core.Addresses;
using Ton.Core.Boc;

namespace Ton.Core.Tests;

[TestFixture]
public class BuilderTests
{
    [Test]
    public void Test_BeginCell()
    {
        var builder = Builder.BeginCell();
        Assert.That(builder.Bits, Is.EqualTo(0));
        Assert.That(builder.Refs, Is.EqualTo(0));
    }

    [Test]
    public void Test_StoreBit()
    {
        var builder = Builder.BeginCell();
        builder.StoreBit(true);
        builder.StoreBit(false);
        
        Assert.That(builder.Bits, Is.EqualTo(2));
        
        var slice = builder.AsSlice();
        Assert.That(slice.LoadBit(), Is.True);
        Assert.That(slice.LoadBit(), Is.False);
    }

    [Test]
    public void Test_StoreUint()
    {
        var builder = Builder.BeginCell();
        builder.StoreUint(12345, 32);
        
        var slice = builder.AsSlice();
        Assert.That(slice.LoadUint(32), Is.EqualTo(12345));
    }

    [Test]
    public void Test_StoreInt()
    {
        var builder = Builder.BeginCell();
        builder.StoreInt(-12345, 32);
        
        var slice = builder.AsSlice();
        Assert.That(slice.LoadInt(32), Is.EqualTo(-12345));
    }

    [Test]
    public void Test_StoreCoins()
    {
        var builder = Builder.BeginCell();
        builder.StoreCoins(1000000000);
        
        var slice = builder.AsSlice();
        Assert.That((long)slice.LoadCoins(), Is.EqualTo(1000000000));
    }

    [Test]
    public void Test_StoreAddress()
    {
        var address = Address.Parse("EQAs9VlT6S776tq3unJcP5Ogsj-ELLunLXuOb1EKcOQi4wJB");
        
        var builder = Builder.BeginCell();
        builder.StoreAddress(address);
        
        var slice = builder.AsSlice();
        var loaded = slice.LoadAddress();
        Assert.That(loaded, Is.Not.Null);
        Assert.That(loaded.ToString(), Is.EqualTo(address.ToString()));
    }

    [Test]
    public void Test_StoreRef()
    {
        var innerCell = Builder.BeginCell()
            .StoreUint(123, 32)
            .EndCell();
        
        var builder = Builder.BeginCell();
        builder.StoreRef(innerCell);
        
        Assert.That(builder.Refs, Is.EqualTo(1));
        
        var slice = builder.AsSlice();
        var ref1 = slice.LoadRef();
        Assert.That(ref1.BeginParse().LoadUint(32), Is.EqualTo(123));
    }

    [Test]
    public void Test_StoreSlice()
    {
        var source = Builder.BeginCell()
            .StoreUint(111, 32)
            .StoreUint(222, 32)
            .EndCell();
        
        var builder = Builder.BeginCell();
        builder.StoreSlice(source.BeginParse());
        
        var slice = builder.AsSlice();
        Assert.That(slice.LoadUint(32), Is.EqualTo(111));
        Assert.That(slice.LoadUint(32), Is.EqualTo(222));
    }

    [Test]
    public void Test_EndCell()
    {
        var cell = Builder.BeginCell()
            .StoreUint(12345, 32)
            .EndCell();
        
        Assert.That(cell, Is.Not.Null);
        Assert.That(cell.Type, Is.EqualTo(CellType.Ordinary));
    }

    [Test]
    public void Test_AvailableBits()
    {
        var builder = Builder.BeginCell();
        Assert.That(builder.AvailableBits, Is.EqualTo(1023));
        
        builder.StoreUint(0, 100);
        Assert.That(builder.AvailableBits, Is.EqualTo(923));
    }

    [Test]
    public void Test_AvailableRefs()
    {
        var builder = Builder.BeginCell();
        Assert.That(builder.AvailableRefs, Is.EqualTo(4));
        
        builder.StoreRef(new Cell());
        Assert.That(builder.AvailableRefs, Is.EqualTo(3));
    }

    [Test]
    public void Test_TooManyRefs_ThrowsException()
    {
        var builder = Builder.BeginCell();
        builder.StoreRef(new Cell());
        builder.StoreRef(new Cell());
        builder.StoreRef(new Cell());
        builder.StoreRef(new Cell());
        
        var ex = Assert.Throws<InvalidOperationException>(() => builder.StoreRef(new Cell()));
        Assert.That(ex!.Message, Does.Contain("Too many references"));
    }

    [Test]
    public void Test_StoreMaybeUint()
    {
        var builder = Builder.BeginCell();
        builder.StoreMaybeUint(123L, 32);
        builder.StoreMaybeUint(null, 32);
        
        var slice = builder.AsSlice();
        Assert.That(slice.LoadMaybeUint(32), Is.EqualTo(123));
        Assert.That(slice.LoadMaybeUint(32), Is.Null);
    }

    [Test]
    public void Test_StoreMaybeRef()
    {
        var innerCell = Builder.BeginCell().StoreUint(789, 32).EndCell();
        
        var builder = Builder.BeginCell();
        builder.StoreMaybeRef(innerCell);
        builder.StoreMaybeRef((Cell?)null);
        
        var slice = builder.AsSlice();
        var ref1 = slice.LoadMaybeRef();
        Assert.That(ref1, Is.Not.Null);
        Assert.That(ref1!.BeginParse().LoadUint(32), Is.EqualTo(789));
        
        var ref2 = slice.LoadMaybeRef();
        Assert.That(ref2, Is.Null);
    }

    [Test]
    public void Test_RoundTrip_ComplexData()
    {
        var address = Address.Parse("EQAs9VlT6S776tq3unJcP5Ogsj-ELLunLXuOb1EKcOQi4wJB");
        
        var builder = Builder.BeginCell()
            .StoreUint(100, 32)
            .StoreInt(-50, 32)
            .StoreCoins(1000000)
            .StoreAddress(address)
            .StoreRef(Builder.BeginCell().StoreUint(999, 16).EndCell());
        
        var slice = builder.AsSlice();
        Assert.That(slice.LoadUint(32), Is.EqualTo(100));
        Assert.That(slice.LoadInt(32), Is.EqualTo(-50));
        Assert.That((long)slice.LoadCoins(), Is.EqualTo(1000000));
        var loadedAddr = slice.LoadAddress();
        Assert.That(loadedAddr, Is.Not.Null);
        Assert.That(loadedAddr.ToString(), Is.EqualTo(address.ToString()));
        var ref1 = slice.LoadRef();
        Assert.That(ref1.BeginParse().LoadUint(16), Is.EqualTo(999));
    }
}

