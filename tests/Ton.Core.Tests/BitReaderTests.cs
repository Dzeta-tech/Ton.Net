using Ton.Core.Addresses;
using Ton.Core.Boc;

namespace Ton.Core.Tests;

[TestFixture]
public class BitReaderTests
{
    [Test]
    public void Test_ReadUintsFromBuilder()
    {
        Random random = new(42);
        for (int i = 0; i < 100; i++)
        {
            long a = random.NextInt64(0, 281474976710655);
            long b = random.NextInt64(0, 281474976710655);

            BitBuilder builder = new();
            builder.WriteUint(a, 48);
            builder.WriteUint(b, 48);
            BitString bits = builder.Build();

            BitReader reader = new(bits);
            Assert.Multiple(() =>
            {
                Assert.That(reader.PreloadUint(48), Is.EqualTo(a));
                Assert.That(reader.LoadUint(48), Is.EqualTo(a));
            });
            Assert.Multiple(() =>
            {
                Assert.That(reader.PreloadUint(48), Is.EqualTo(b));
                Assert.That(reader.LoadUint(48), Is.EqualTo(b));
            });
        }
    }

    [Test]
    public void Test_ReadIntsFromBuilder()
    {
        Random random = new(43);
        for (int i = 0; i < 100; i++)
        {
            long a = random.NextInt64(-281474976710655, 281474976710655);
            long b = random.NextInt64(-281474976710655, 281474976710655);

            BitBuilder builder = new();
            builder.WriteInt(a, 49);
            builder.WriteInt(b, 49);
            BitString bits = builder.Build();

            BitReader reader = new(bits);
            Assert.Multiple(() =>
            {
                Assert.That(reader.PreloadInt(49), Is.EqualTo(a));
                Assert.That(reader.LoadInt(49), Is.EqualTo(a));
            });
            Assert.Multiple(() =>
            {
                Assert.That(reader.PreloadInt(49), Is.EqualTo(b));
                Assert.That(reader.LoadInt(49), Is.EqualTo(b));
            });
        }
    }

    [Test]
    public void Test_ReadVarUintsFromBuilder()
    {
        Random random = new(44);
        for (int i = 0; i < 100; i++)
        {
            int sizeBits = random.Next(4, 9);
            long a = random.NextInt64(0, 281474976710655);
            long b = random.NextInt64(0, 281474976710655);

            BitBuilder builder = new();
            builder.WriteVarUint(a, sizeBits);
            builder.WriteVarUint(b, sizeBits);
            BitString bits = builder.Build();

            BitReader reader = new(bits);
            Assert.That(reader.LoadVarUint(sizeBits), Is.EqualTo(a));
            Assert.That(reader.LoadVarUint(sizeBits), Is.EqualTo(b));
        }
    }

    [Test]
    public void Test_ReadVarIntsFromBuilder()
    {
        Random random = new(45);
        for (int i = 0; i < 100; i++)
        {
            int sizeBits = random.Next(4, 9);
            long a = random.NextInt64(-281474976710655, 281474976710655);
            long b = random.NextInt64(-281474976710655, 281474976710655);

            BitBuilder builder = new();
            builder.WriteVarInt(a, sizeBits);
            builder.WriteVarInt(b, sizeBits);
            BitString bits = builder.Build();

            BitReader reader = new(bits);
            Assert.That(reader.LoadVarInt(sizeBits), Is.EqualTo(a));
            Assert.That(reader.LoadVarInt(sizeBits), Is.EqualTo(b));
        }
    }

    [Test]
    public void Test_ReadCoinsFromBuilder()
    {
        Random random = new(46);
        for (int i = 0; i < 100; i++)
        {
            long a = random.NextInt64(0, 281474976710655);
            long b = random.NextInt64(0, 281474976710655);

            BitBuilder builder = new();
            builder.WriteCoins(a);
            builder.WriteCoins(b);
            BitString bits = builder.Build();

            BitReader reader = new(bits);
            Assert.That((long)reader.LoadCoins(), Is.EqualTo(a));
            Assert.That((long)reader.LoadCoins(), Is.EqualTo(b));
        }
    }

    [Test]
    public void Test_ReadAddressFromBuilder()
    {
        Random random = new(47);
        for (int i = 0; i < 50; i++)
        {
            // Sometimes null address
            Address? a = random.Next(20) == 0 ? null : GenerateTestAddress(random, 0);
            Address b = GenerateTestAddress(random, 0);

            BitBuilder builder = new();
            builder.WriteAddress(a);
            builder.WriteAddress(b);
            BitString bits = builder.Build();

            BitReader reader = new(bits);
            Address? loadedA = reader.LoadAddress();
            if (a != null)
                Assert.That(loadedA!.ToString(), Is.EqualTo(a.ToString()));
            else
                Assert.That(loadedA, Is.Null);

            Address? loadedB = reader.LoadAddress();
            Assert.That(loadedB!.ToString(), Is.EqualTo(b.ToString()));
        }
    }

    [Test]
    public void Test_SkipAndReset()
    {
        BitBuilder builder = new();
        builder.WriteUint(12345, 32);
        builder.WriteUint(67890, 32);
        BitString bits = builder.Build();

        BitReader reader = new(bits);
        Assert.That(reader.Offset, Is.EqualTo(0));

        reader.Skip(16);
        Assert.That(reader.Offset, Is.EqualTo(16));

        reader.Save();
        reader.Skip(16);
        Assert.That(reader.Offset, Is.EqualTo(32));

        reader.Reset();
        Assert.That(reader.Offset, Is.EqualTo(16));

        reader.Reset();
        Assert.That(reader.Offset, Is.EqualTo(0));
    }

    [Test]
    public void Test_LoadBits()
    {
        BitBuilder builder = new();
        builder.WriteBit(true);
        builder.WriteBit(false);
        builder.WriteBit(true);
        builder.WriteBit(true);
        BitString bits = builder.Build();

        BitReader reader = new(bits);
        Assert.Multiple(() =>
        {
            Assert.That(reader.PreloadBit(), Is.True);
            Assert.That(reader.LoadBit(), Is.True);
        });
        Assert.That(reader.LoadBit(), Is.False);
        Assert.That(reader.LoadBit(), Is.True);
        Assert.Multiple(() =>
        {
            Assert.That(reader.LoadBit(), Is.True);
            Assert.That(reader.Remaining, Is.EqualTo(0));
        });
    }

    static Address GenerateTestAddress(Random random, int workchain)
    {
        byte[] hash = new byte[32];
        random.NextBytes(hash);
        return new Address(workchain, hash);
    }
}