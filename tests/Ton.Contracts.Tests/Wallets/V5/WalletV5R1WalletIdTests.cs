using System.Numerics;
using Ton.Contracts.Wallets.V5;
using Ton.Core.Boc;

namespace Ton.Contracts.Tests.Wallets.V5;

[TestFixture]
public class WalletV5R1WalletIdTests
{
    [Test]
    public void ShouldSerializeWalletId()
    {
        WalletIdV5R1 walletId = new(
            -239,
            new WalletIdV5R1ClientContext("v5r1", 0, 0)
        );

        Builder builder = Builder.BeginCell();
        WalletV5R1WalletIdHelper.StoreWalletIdV5R1(walletId)(builder);
        Cell actual = builder.EndCell();

        WalletIdV5R1ClientContext clientContext = (WalletIdV5R1ClientContext)walletId.Context;
        long context = Builder.BeginCell()
            .StoreBit(true)
            .StoreInt(clientContext.Workchain, 8)
            .StoreUint(0, 8)
            .StoreUint((ulong)clientContext.SubwalletNumber, 15)
            .EndCell()
            .BeginParse()
            .LoadInt(32);

        Cell expected = Builder.BeginCell()
            .StoreInt((long)(context ^ (BigInteger)walletId.NetworkGlobalId), 32)
            .EndCell();

        Assert.That(expected.Equals(actual), Is.True);
    }

    [Test]
    public void ShouldDeserializeWalletId()
    {
        WalletIdV5R1 expected = new(
            -239,
            new WalletIdV5R1ClientContext("v5r1", 0, 0)
        );

        WalletIdV5R1ClientContext clientContext = (WalletIdV5R1ClientContext)expected.Context;
        long context = Builder.BeginCell()
            .StoreBit(true)
            .StoreInt(clientContext.Workchain, 8)
            .StoreUint(0, 8)
            .StoreUint((ulong)clientContext.SubwalletNumber, 15)
            .EndCell()
            .BeginParse()
            .LoadInt(32);

        Slice slice = Builder.BeginCell()
            .StoreInt((long)(context ^ (BigInteger)expected.NetworkGlobalId), 32)
            .EndCell()
            .BeginParse();

        WalletIdV5R1 actual = WalletV5R1WalletIdHelper.LoadWalletIdV5R1(slice, expected.NetworkGlobalId);

        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void ShouldDeserializeCorrectlyInAllModes()
    {
        Random random = new(42);

        int GetRandom(int min, int max)
        {
            return random.Next(min, max + 1);
        }

        int subwalletMax = (1 << 15) - 1;
        List<int> randomSubwallets = Enumerable.Range(0, 10)
            .Select(_ => GetRandom(1, subwalletMax - 2))
            .ToList();

        foreach (int networkId in new[] { -239, -3 })
        foreach (int testWc in new[] { 0, -1 })
        foreach (int testSubwallet in new[] { 0, subwalletMax }.Concat(randomSubwallets))
        {
            WalletIdV5R1 expected = new(
                networkId,
                new WalletIdV5R1ClientContext("v5r1", testWc, testSubwallet)
            );

            Builder packBuilder = Builder.BeginCell();
            WalletV5R1WalletIdHelper.StoreWalletIdV5R1(expected)(packBuilder);
            Cell packed = packBuilder.EndCell();

            // Test with Slice
            WalletIdV5R1 unpacked1 = WalletV5R1WalletIdHelper.LoadWalletIdV5R1(packed.BeginParse(), networkId);
            Assert.That(unpacked1, Is.EqualTo(expected));

            // Test with BigInteger
            BigInteger intVal = packed.BeginParse().LoadInt(32);
            WalletIdV5R1 unpacked2 = WalletV5R1WalletIdHelper.LoadWalletIdV5R1(intVal, networkId);
            Assert.That(unpacked2, Is.EqualTo(expected));

            // Test with byte[]
            byte[] buffVal = packed.BeginParse().LoadBuffer(4);
            WalletIdV5R1 unpacked3 = WalletV5R1WalletIdHelper.LoadWalletIdV5R1(buffVal, networkId);
            Assert.That(unpacked3, Is.EqualTo(expected));
        }
    }

    [Test]
    public void ShouldSerializeWalletIdWithCustomContext()
    {
        WalletIdV5R1 walletId = new(
            -3,
            new WalletIdV5R1CustomContext(239239239)
        );

        WalletIdV5R1CustomContext customContext = (WalletIdV5R1CustomContext)walletId.Context;
        long context = Builder.BeginCell()
            .StoreBit(false)
            .StoreUint(customContext.Value, 31)
            .EndCell()
            .BeginParse()
            .LoadInt(32);

        Builder customBuilder = Builder.BeginCell();
        WalletV5R1WalletIdHelper.StoreWalletIdV5R1(walletId)(customBuilder);
        Cell actual = customBuilder.EndCell();

        Cell expected = Builder.BeginCell()
            .StoreInt((long)(context ^ (BigInteger)walletId.NetworkGlobalId), 32)
            .EndCell();

        Assert.That(expected.Equals(actual), Is.True);
    }

    [Test]
    public void ShouldDeserializeWalletIdWithCustomContext()
    {
        WalletIdV5R1 expected = new(
            -3,
            new WalletIdV5R1CustomContext(239239239)
        );

        WalletIdV5R1CustomContext customContext = (WalletIdV5R1CustomContext)expected.Context;
        long context = Builder.BeginCell()
            .StoreBit(false)
            .StoreUint(customContext.Value, 31)
            .EndCell()
            .BeginParse()
            .LoadInt(32);

        Slice slice = Builder.BeginCell()
            .StoreInt((long)(context ^ (BigInteger)expected.NetworkGlobalId), 32)
            .EndCell()
            .BeginParse();

        WalletIdV5R1 actual = WalletV5R1WalletIdHelper.LoadWalletIdV5R1(slice, expected.NetworkGlobalId);

        Assert.That(actual, Is.EqualTo(expected));
    }
}