using Ton.Core.Addresses;
using Ton.Core.Boc;

namespace Ton.Core.Tests;

[TestFixture]
public class SliceTests
{
    [Test]
    public void Test_LoadBit()
    {
        var cell = Builder.BeginCell()
            .StoreBit(true)
            .StoreBit(false)
            .EndCell();
        
        var slice = cell.BeginParse();
        Assert.That(slice.LoadBit(), Is.True);
        Assert.That(slice.LoadBit(), Is.False);
    }

    [Test]
    public void Test_PreloadBit()
    {
        var cell = Builder.BeginCell()
            .StoreBit(true)
            .EndCell();
        
        var slice = cell.BeginParse();
        Assert.That(slice.PreloadBit(), Is.True);
        Assert.That(slice.PreloadBit(), Is.True); // Should not advance
        Assert.That(slice.LoadBit(), Is.True);
    }

    [Test]
    public void Test_LoadUint()
    {
        var cell = Builder.BeginCell()
            .StoreUint(12345, 32)
            .EndCell();
        
        var slice = cell.BeginParse();
        Assert.That(slice.LoadUint(32), Is.EqualTo(12345));
    }

    [Test]
    public void Test_LoadInt()
    {
        var cell = Builder.BeginCell()
            .StoreInt(-12345, 32)
            .EndCell();
        
        var slice = cell.BeginParse();
        Assert.That(slice.LoadInt(32), Is.EqualTo(-12345));
    }

    [Test]
    public void Test_LoadCoins()
    {
        var cell = Builder.BeginCell()
            .StoreCoins(1000000000)
            .EndCell();
        
        var slice = cell.BeginParse();
        Assert.That((long)slice.LoadCoins(), Is.EqualTo(1000000000));
    }

    [Test]
    public void Test_LoadAddress()
    {
        var address = Address.Parse("EQAs9VlT6S776tq3unJcP5Ogsj-ELLunLXuOb1EKcOQi4wJB");
        
        var cell = Builder.BeginCell()
            .StoreAddress(address)
            .EndCell();
        
        var slice = cell.BeginParse();
        var loaded = slice.LoadAddress();
        Assert.That(loaded, Is.Not.Null);
        Assert.That(loaded.ToString(), Is.EqualTo(address.ToString()));
    }

    [Test]
    public void Test_LoadRef()
    {
        var innerCell = Builder.BeginCell()
            .StoreUint(999, 32)
            .EndCell();
        
        var cell = Builder.BeginCell()
            .StoreRef(innerCell)
            .EndCell();
        
        var slice = cell.BeginParse();
        var ref1 = slice.LoadRef();
        Assert.That(ref1.BeginParse().LoadUint(32), Is.EqualTo(999));
    }

    [Test]
    public void Test_RemainingBits()
    {
        var cell = Builder.BeginCell()
            .StoreUint(0, 100)
            .EndCell();
        
        var slice = cell.BeginParse();
        Assert.That(slice.RemainingBits, Is.EqualTo(100));
        
        slice.LoadUint(32);
        Assert.That(slice.RemainingBits, Is.EqualTo(68));
    }

    [Test]
    public void Test_RemainingRefs()
    {
        var cell = Builder.BeginCell()
            .StoreRef(new Cell())
            .StoreRef(new Cell())
            .EndCell();
        
        var slice = cell.BeginParse();
        Assert.That(slice.RemainingRefs, Is.EqualTo(2));
        
        slice.LoadRef();
        Assert.That(slice.RemainingRefs, Is.EqualTo(1));
    }

    [Test]
    public void Test_Skip()
    {
        var cell = Builder.BeginCell()
            .StoreUint(111, 32)
            .StoreUint(222, 32)
            .EndCell();
        
        var slice = cell.BeginParse();
        slice.Skip(32);
        Assert.That(slice.LoadUint(32), Is.EqualTo(222));
    }

    [Test]
    public void Test_EndParse_Empty()
    {
        var cell = Builder.BeginCell()
            .StoreUint(123, 32)
            .EndCell();
        
        var slice = cell.BeginParse();
        slice.LoadUint(32);
        
        Assert.DoesNotThrow(() => slice.EndParse());
    }

    [Test]
    public void Test_EndParse_NotEmpty_ThrowsException()
    {
        var cell = Builder.BeginCell()
            .StoreUint(123, 32)
            .EndCell();
        
        var slice = cell.BeginParse();
        
        var ex = Assert.Throws<InvalidOperationException>(() => slice.EndParse());
        Assert.That(ex!.Message, Does.Contain("Slice is not empty"));
    }

    [Test]
    public void Test_Clone()
    {
        var cell = Builder.BeginCell()
            .StoreUint(100, 32)
            .StoreUint(200, 32)
            .EndCell();
        
        var slice = cell.BeginParse();
        slice.LoadUint(32); // Advance to 200
        
        var clone = slice.Clone();
        Assert.That(clone.LoadUint(32), Is.EqualTo(200));
        Assert.That(slice.LoadUint(32), Is.EqualTo(200)); // Original also at 200
    }

    [Test]
    public void Test_Clone_FromStart()
    {
        var cell = Builder.BeginCell()
            .StoreUint(100, 32)
            .StoreUint(200, 32)
            .EndCell();
        
        var slice = cell.BeginParse();
        slice.LoadUint(32); // Advance to 200
        
        var clone = slice.Clone(fromStart: true);
        Assert.That(clone.LoadUint(32), Is.EqualTo(100)); // Starts from beginning
    }

    [Test]
    public void Test_AsCell()
    {
        var original = Builder.BeginCell()
            .StoreUint(456, 32)
            .EndCell();
        
        var slice = original.BeginParse();
        var cell = slice.AsCell();
        
        Assert.That(cell.BeginParse().LoadUint(32), Is.EqualTo(456));
    }

    [Test]
    public void Test_AsBuilder()
    {
        var original = Builder.BeginCell()
            .StoreUint(789, 32)
            .EndCell();
        
        var slice = original.BeginParse();
        var builder = slice.AsBuilder();
        
        Assert.That(builder.AsSlice().LoadUint(32), Is.EqualTo(789));
    }

    [Test]
    public void Test_LoadMaybeUint()
    {
        var cell = Builder.BeginCell()
            .StoreMaybeUint(555L, 32)
            .StoreMaybeUint(null, 32)
            .EndCell();
        
        var slice = cell.BeginParse();
        Assert.That(slice.LoadMaybeUint(32), Is.EqualTo(555));
        Assert.That(slice.LoadMaybeUint(32), Is.Null);
    }

    [Test]
    public void Test_LoadMaybeRef()
    {
        var innerCell = Builder.BeginCell().StoreUint(111, 16).EndCell();
        
        var cell = Builder.BeginCell()
            .StoreMaybeRef(innerCell)
            .StoreMaybeRef((Cell?)null)
            .EndCell();
        
        var slice = cell.BeginParse();
        var ref1 = slice.LoadMaybeRef();
        Assert.That(ref1, Is.Not.Null);
        Assert.That(ref1!.BeginParse().LoadUint(16), Is.EqualTo(111));
        
        var ref2 = slice.LoadMaybeRef();
        Assert.That(ref2, Is.Null);
    }

    [Test]
    public void Test_OffsetBits()
    {
        var cell = Builder.BeginCell()
            .StoreUint(0, 100)
            .EndCell();
        
        var slice = cell.BeginParse();
        Assert.That(slice.OffsetBits, Is.EqualTo(0));
        
        slice.LoadUint(32);
        Assert.That(slice.OffsetBits, Is.EqualTo(32));
    }

    [Test]
    public void Test_OffsetRefs()
    {
        var cell = Builder.BeginCell()
            .StoreRef(new Cell())
            .StoreRef(new Cell())
            .EndCell();
        
        var slice = cell.BeginParse();
        Assert.That(slice.OffsetRefs, Is.EqualTo(0));
        
        slice.LoadRef();
        Assert.That(slice.OffsetRefs, Is.EqualTo(1));
    }
}

