using Ton.Core.Addresses;
using Ton.Core.Boc;
using Ton.Core.Types;

namespace Ton.Core.Tests;

public class MessageTests
{
    [Test]
    public void Test_Message_EdgeCase_ExtraCurrency()
    {
        // Matching JS test: "should handle edge case with extra currency"
        const string tx =
            "te6cckEBBwEA3QADs2gB7ix8WDhQdzzFOCf6hmZ2Dzw2vFNtbavUArvbhXqqqmEAMpuMhx8zp7O3wqMokkuyFkklKpftc4Dh9_5bvavmCo-UXR6uVOIGMkCwAAAAAAC3GwLLUHl_4AYCAQCA_____________________________________________________________________________________gMBPAUEAwFDoAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAOAUACAAAAAAAAAANoAAAAAEIDF-r-4Q";

        // Convert URL-safe base64 to standard base64
        string standardBase64 = tx.Replace('-', '+').Replace('_', '/');
        // Add padding if needed
        int padLen = (4 - standardBase64.Length % 4) % 4;
        if (padLen > 0) standardBase64 += new string('=', padLen);
        Cell cell = Cell.FromBoc(Convert.FromBase64String(standardBase64))[0];
        Message message = Message.Load(cell.BeginParse());

        Builder stored = Builder.BeginCell();
        message.Store(stored);
        Cell storedCell = stored.EndCell();

        Assert.That(storedCell.Equals(cell), Is.True);
    }

    [Test]
    public void Test_Message_Internal_Roundtrip()
    {
        // Create internal message
        Address srcAddr = new(0, new byte[32]);
        Address destAddr = new(-1, new byte[32]);

        CommonMessageInfo info = new CommonMessageInfo.Internal(
            true,
            true,
            false,
            srcAddr,
            destAddr,
            new CurrencyCollection(1000000000),
            0,
            1000,
            12345,
            1234567890
        );

        Cell body = Builder.BeginCell().StoreUint(123, 32).EndCell();

        Message message = new(info, body);

        // Serialize
        Builder builder = Builder.BeginCell();
        message.Store(builder);
        Cell cell = builder.EndCell();

        // Deserialize
        Message message2 = Message.Load(cell.BeginParse());

        // Serialize again
        Builder builder2 = Builder.BeginCell();
        message2.Store(builder2);
        Cell cell2 = builder2.EndCell();

        Assert.Multiple(() =>
        {
            // Should be equal
            Assert.That(cell.Equals(cell2), Is.True);
            Assert.That(message2.Info, Is.InstanceOf<CommonMessageInfo.Internal>());
            Assert.That(message2.Init, Is.Null);
            Assert.That(message2.Body.Bits.Length, Is.EqualTo(32));
        });
    }

    [Test]
    public void Test_Message_WithStateInit_Roundtrip()
    {
        // Create message with StateInit
        Address destAddr = new(0, new byte[32]);

        CommonMessageInfo info = new CommonMessageInfo.ExternalIn(
            null,
            destAddr,
            0
        );

        StateInit stateInit = new(
            Builder.BeginCell().StoreUint(1, 8).EndCell(),
            Builder.BeginCell().StoreUint(2, 8).EndCell()
        );

        Cell body = Builder.BeginCell().StoreUint(999, 32).EndCell();

        Message message = new(info, body, stateInit);

        // Serialize
        Builder builder = Builder.BeginCell();
        message.Store(builder);
        Cell cell = builder.EndCell();

        // Deserialize
        Message message2 = Message.Load(cell.BeginParse());

        // Serialize again
        Builder builder2 = Builder.BeginCell();
        message2.Store(builder2);
        Cell cell2 = builder2.EndCell();

        Assert.Multiple(() =>
        {
            // Should be equal
            Assert.That(cell.Equals(cell2), Is.True);
            Assert.That(message2.Info, Is.InstanceOf<CommonMessageInfo.ExternalIn>());
            Assert.That(message2.Init, Is.Not.Null);
        });
        Assert.Multiple(() =>
        {
            Assert.That(message2.Init!.Code, Is.Not.Null);
            Assert.That(message2.Init!.Data, Is.Not.Null);
        });
    }

    [Test]
    public void Test_Message_EmptyBody_Roundtrip()
    {
        // Create message with empty body
        Address srcAddr = new(0, new byte[32]);
        Address destAddr = new(0, new byte[32]);

        CommonMessageInfo info = new CommonMessageInfo.Internal(
            true,
            false,
            false,
            srcAddr,
            destAddr,
            new CurrencyCollection(0),
            0,
            0,
            0,
            0
        );

        Cell body = Builder.BeginCell().EndCell(); // Empty cell

        Message message = new(info, body);

        // Serialize
        Builder builder = Builder.BeginCell();
        message.Store(builder);
        Cell cell = builder.EndCell();

        // Deserialize
        Message message2 = Message.Load(cell.BeginParse());

        // Serialize again
        Builder builder2 = Builder.BeginCell();
        message2.Store(builder2);
        Cell cell2 = builder2.EndCell();

        Assert.Multiple(() =>
        {
            // Should be equal
            Assert.That(cell.Equals(cell2), Is.True);
            Assert.That(message2.Body.Bits.Length, Is.EqualTo(0));
        });
    }

    [Test]
    public void Test_Message_ForceRef()
    {
        // Create message and force refs
        Address srcAddr = new(0, new byte[32]);
        Address destAddr = new(0, new byte[32]);

        CommonMessageInfo info = new CommonMessageInfo.Internal(
            true,
            true,
            false,
            srcAddr,
            destAddr,
            new CurrencyCollection(1000),
            0,
            0,
            0,
            0
        );

        StateInit stateInit = new(
            Builder.BeginCell().StoreUint(1, 8).EndCell(),
            Builder.BeginCell().StoreUint(2, 8).EndCell()
        );

        Cell body = Builder.BeginCell().StoreUint(123, 32).EndCell();

        Message message = new(info, body, stateInit);

        // Serialize with forceRef
        Builder builder = Builder.BeginCell();
        message.Store(builder, true);
        Cell cell = builder.EndCell();

        // Should have refs for init and body
        Assert.That(cell.Refs, Has.Length.GreaterThanOrEqualTo(2));

        // Should still deserialize correctly
        Message message2 = Message.Load(cell.BeginParse());
        Assert.Multiple(() =>
        {
            Assert.That(message2.Init, Is.Not.Null);
            Assert.That(message2.Body.Bits.Length, Is.EqualTo(32));
        });
    }
}