using System.Numerics;
using Ton.Core.Addresses;
using Ton.Core.Boc;
using Ton.Core.Types;

namespace Ton.Core.Tests;

public class CommonMessageInfoTests
{
    [Test]
    public void Test_ExternalIn_Roundtrip()
    {
        // Create external-in message (matching JS test)
        CommonMessageInfo msg = new CommonMessageInfo.ExternalIn(
            new ExternalAddress(0x123456789ABCDEF0, 64),
            new Address(0, new byte[32]),
            0
        );

        // Serialize
        Builder builder = Builder.BeginCell();
        msg.Store(builder);
        Cell cell = builder.EndCell();

        // Deserialize
        CommonMessageInfo msg2 = CommonMessageInfo.Load(cell.BeginParse());

        // Serialize again
        Builder builder2 = Builder.BeginCell();
        msg2.Store(builder2);
        Cell cell2 = builder2.EndCell();

        Assert.Multiple(() =>
        {
            // Should be equal
            Assert.That(cell.Equals(cell2), Is.True);
            Assert.That(msg2, Is.InstanceOf<CommonMessageInfo.ExternalIn>());
        });

        CommonMessageInfo.ExternalIn ei = (CommonMessageInfo.ExternalIn)msg2;
        Assert.Multiple(() =>
        {
            Assert.That(ei.Src, Is.Not.Null);
            Assert.That(ei.Src!.Value, Is.EqualTo((BigInteger)0x123456789ABCDEF0));
            Assert.That(ei.ImportFee, Is.EqualTo((BigInteger)0));
        });
    }

    [Test]
    public void Test_Internal_Roundtrip()
    {
        // Create internal message
        Address srcAddr = new(0, new byte[32]);
        Address destAddr = new(-1, new byte[32]);

        CommonMessageInfo msg = new CommonMessageInfo.Internal(
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

        // Serialize
        Builder builder = Builder.BeginCell();
        msg.Store(builder);
        Cell cell = builder.EndCell();

        // Deserialize
        CommonMessageInfo msg2 = CommonMessageInfo.Load(cell.BeginParse());

        // Serialize again
        Builder builder2 = Builder.BeginCell();
        msg2.Store(builder2);
        Cell cell2 = builder2.EndCell();

        Assert.Multiple(() =>
        {
            // Should be equal
            Assert.That(cell.Equals(cell2), Is.True);
            Assert.That(msg2, Is.InstanceOf<CommonMessageInfo.Internal>());
        });

        CommonMessageInfo.Internal im = (CommonMessageInfo.Internal)msg2;
        Assert.Multiple(() =>
        {
            Assert.That(im.IhrDisabled, Is.True);
            Assert.That(im.Bounce, Is.True);
            Assert.That(im.Bounced, Is.False);
            Assert.That(im.Src, Is.EqualTo(srcAddr));
            Assert.That(im.Dest, Is.EqualTo(destAddr));
            Assert.That(im.Value.Coins, Is.EqualTo((BigInteger)1000000000));
            Assert.That(im.IhrFee, Is.EqualTo((BigInteger)0));
            Assert.That(im.ForwardFee, Is.EqualTo((BigInteger)1000));
            Assert.That(im.CreatedLt, Is.EqualTo((BigInteger)12345));
            Assert.That(im.CreatedAt, Is.EqualTo(1234567890U));
        });
    }

    [Test]
    public void Test_ExternalOut_Roundtrip()
    {
        // Create external-out message
        Address srcAddr = new(0, new byte[32]);
        ExternalAddress destAddr = new(0xDEADBEEF, 32);

        CommonMessageInfo msg = new CommonMessageInfo.ExternalOut(
            srcAddr,
            destAddr,
            99999,
            1234567890 // Must be uint32
        );

        // Serialize
        Builder builder = Builder.BeginCell();
        msg.Store(builder);
        Cell cell = builder.EndCell();

        // Deserialize
        CommonMessageInfo msg2 = CommonMessageInfo.Load(cell.BeginParse());

        // Serialize again
        Builder builder2 = Builder.BeginCell();
        msg2.Store(builder2);
        Cell cell2 = builder2.EndCell();

        Assert.Multiple(() =>
        {
            // Should be equal
            Assert.That(cell.Equals(cell2), Is.True);
            Assert.That(msg2, Is.InstanceOf<CommonMessageInfo.ExternalOut>());
        });

        CommonMessageInfo.ExternalOut eo = (CommonMessageInfo.ExternalOut)msg2;
        Assert.Multiple(() =>
        {
            Assert.That(eo.Src, Is.EqualTo(srcAddr));
            Assert.That(eo.Dest, Is.Not.Null);
            Assert.That(eo.Dest!.Value, Is.EqualTo((BigInteger)0xDEADBEEF));
            Assert.That(eo.CreatedLt, Is.EqualTo((BigInteger)99999));
            Assert.That(eo.CreatedAt, Is.EqualTo(1234567890U));
        });
    }

    [Test]
    public void Test_ExternalIn_WithNullSrc()
    {
        // Create external-in message with null source
        CommonMessageInfo msg = new CommonMessageInfo.ExternalIn(
            null,
            new Address(0, new byte[32]),
            0
        );

        // Serialize and deserialize
        Builder builder = Builder.BeginCell();
        msg.Store(builder);
        Cell cell = builder.EndCell();

        CommonMessageInfo msg2 = CommonMessageInfo.Load(cell.BeginParse());

        Assert.That(msg2, Is.InstanceOf<CommonMessageInfo.ExternalIn>());
        CommonMessageInfo.ExternalIn ei = (CommonMessageInfo.ExternalIn)msg2;
        Assert.That(ei.Src, Is.Null);
    }

    [Test]
    public void Test_ExternalOut_WithNullDest()
    {
        // Create external-out message with null destination
        CommonMessageInfo msg = new CommonMessageInfo.ExternalOut(
            new Address(0, new byte[32]),
            null,
            123,
            456
        );

        // Serialize and deserialize
        Builder builder = Builder.BeginCell();
        msg.Store(builder);
        Cell cell = builder.EndCell();

        CommonMessageInfo msg2 = CommonMessageInfo.Load(cell.BeginParse());

        Assert.That(msg2, Is.InstanceOf<CommonMessageInfo.ExternalOut>());
        CommonMessageInfo.ExternalOut eo = (CommonMessageInfo.ExternalOut)msg2;
        Assert.That(eo.Dest, Is.Null);
    }
}