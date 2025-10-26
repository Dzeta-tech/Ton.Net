using Ton.Core.Boc;
using Ton.Core.Types;

namespace Ton.Core.Tests;

public class StateInitTests
{
    [Test]
    public void Test_StateInit_Serialize_Golden1()
    {
        // Serialize - matching JS test "should serialize to match golden-1"
        StateInit stateInit = new(
            Builder.BeginCell().StoreUint(1, 8).EndCell(),
            Builder.BeginCell().StoreUint(2, 8).EndCell()
        );

        Builder builder = Builder.BeginCell();
        stateInit.Store(builder);
        Cell boc = builder.EndCell();

        byte[] bocBytes = boc.ToBoc(false);
        string bocBase64 = Convert.ToBase64String(bocBytes);

        // Should match JS golden value
        Assert.That(bocBase64, Is.EqualTo("te6cckEBAwEACwACATQBAgACAQACAoN/wQo="));
    }

    [Test]
    public void Test_StateInit_Parse_Golden1()
    {
        // Parse the golden BOC
        byte[] bocBytes = Convert.FromBase64String("te6cckEBAwEACwACATQBAgACAQACAoN/wQo=");
        Cell[] cells = Cell.FromBoc(bocBytes);
        StateInit parsed = StateInit.Load(cells[0].BeginParse());

        // Verify fields
        Assert.Multiple(() =>
        {
            Assert.That(parsed.Libraries, Is.Null);
            Assert.That(parsed.Special, Is.Null);
            Assert.That(parsed.SplitDepth, Is.Null);
            Assert.That(parsed.Code, Is.Not.Null);
            Assert.That(parsed.Data, Is.Not.Null);
        });

        // Verify code
        Slice codeSlice = parsed.Code!.BeginParse();
        long codeValue = codeSlice.LoadUint(8);
        Assert.That(codeValue, Is.EqualTo(1));

        // Verify data
        Slice dataSlice = parsed.Data!.BeginParse();
        long dataValue = dataSlice.LoadUint(8);
        Assert.That(dataValue, Is.EqualTo(2));
    }

    [Test]
    public void Test_StateInit_RoundTrip()
    {
        // Create a StateInit with various fields
        StateInit original = new(
            Builder.BeginCell().StoreUint(123, 32).EndCell(),
            Builder.BeginCell().StoreUint(456, 32).EndCell(),
            5
        );

        // Serialize
        Builder builder = Builder.BeginCell();
        original.Store(builder);
        Cell cell = builder.EndCell();

        // Parse back
        StateInit parsed = StateInit.Load(cell.BeginParse());

        // Verify
        Assert.Multiple(() =>
        {
            Assert.That(parsed.SplitDepth, Is.EqualTo(5));
            Assert.That(parsed.Special, Is.Null);
            Assert.That(parsed.Libraries, Is.Null);
            Assert.That(parsed.Code, Is.Not.Null);
            Assert.That(parsed.Data, Is.Not.Null);
        });

        Assert.Multiple(() =>
        {
            // Verify code and data
            Assert.That(parsed.Code!.BeginParse().LoadUint(32), Is.EqualTo(123));
            Assert.That(parsed.Data!.BeginParse().LoadUint(32), Is.EqualTo(456));
        });
    }

    [Test]
    public void Test_StateInit_With_Special()
    {
        // Create StateInit with tick-tock
        StateInit original = new(
            Builder.BeginCell().StoreUint(1, 8).EndCell(),
            special: new TickTock(true, false)
        );

        // Serialize and parse
        Builder builder = Builder.BeginCell();
        original.Store(builder);
        Cell cell = builder.EndCell();
        StateInit parsed = StateInit.Load(cell.BeginParse());

        // Verify
        Assert.Multiple(() =>
        {
            Assert.That(parsed.Special, Is.Not.Null);
            Assert.That(parsed.Special!.Tick, Is.True);
            Assert.That(parsed.Special!.Tock, Is.False);
        });
    }

    [Test]
    public void Test_StateInit_Empty()
    {
        // Create empty StateInit
        StateInit original = new();

        // Serialize and parse
        Builder builder = Builder.BeginCell();
        original.Store(builder);
        Cell cell = builder.EndCell();
        StateInit parsed = StateInit.Load(cell.BeginParse());

        // Verify all fields are null/empty
        Assert.Multiple(() =>
        {
            Assert.That(parsed.SplitDepth, Is.Null);
            Assert.That(parsed.Special, Is.Null);
            Assert.That(parsed.Code, Is.Null);
            Assert.That(parsed.Data, Is.Null);
            Assert.That(parsed.Libraries, Is.Null);
        });
    }

    [Test]
    public void Test_TickTock_RoundTrip()
    {
        TickTock original = new(true, true);

        Builder builder = Builder.BeginCell();
        original.Store(builder);
        Cell cell = builder.EndCell();

        TickTock parsed = TickTock.Load(cell.BeginParse());

        Assert.Multiple(() =>
        {
            Assert.That(parsed.Tick, Is.EqualTo(original.Tick));
            Assert.That(parsed.Tock, Is.EqualTo(original.Tock));
        });
    }

    [Test]
    public void Test_SimpleLibrary_RoundTrip()
    {
        Cell libRoot = Builder.BeginCell().StoreUint(999, 32).EndCell();
        SimpleLibrary original = new(true, libRoot);

        Builder builder = Builder.BeginCell();
        original.Store(builder);
        Cell cell = builder.EndCell();

        SimpleLibrary parsed = SimpleLibrary.Load(cell.BeginParse());

        Assert.Multiple(() =>
        {
            Assert.That(parsed.Public, Is.EqualTo(original.Public));
            Assert.That(parsed.Root.Hash(), Is.EqualTo(original.Root.Hash()));
        });
    }
}