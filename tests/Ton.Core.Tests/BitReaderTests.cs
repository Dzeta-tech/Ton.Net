using System.Numerics;
using Ton.Core.Addresses;
using Ton.Core.Boc;

namespace Ton.Core.Tests;

[TestFixture]
public class BitReaderTests
{
    [Test]
    public void Test_ReadUintsFromBuilder()
    {
        var random = new Random(42);
        for (int i = 0; i < 100; i++)
        {
            long a = random.NextInt64(0, 281474976710655);
            long b = random.NextInt64(0, 281474976710655);
            
            var builder = new BitBuilder();
            builder.WriteUint(a, 48);
            builder.WriteUint(b, 48);
            var bits = builder.Build();

            var reader = new BitReader(bits);
            Assert.That(reader.PreloadUint(48), Is.EqualTo(a));
            Assert.That(reader.LoadUint(48), Is.EqualTo(a));
            Assert.That(reader.PreloadUint(48), Is.EqualTo(b));
            Assert.That(reader.LoadUint(48), Is.EqualTo(b));
        }
    }

    [Test]
    public void Test_ReadIntsFromBuilder()
    {
        var random = new Random(43);
        for (int i = 0; i < 100; i++)
        {
            long a = random.NextInt64(-281474976710655, 281474976710655);
            long b = random.NextInt64(-281474976710655, 281474976710655);
            
            var builder = new BitBuilder();
            builder.WriteInt(a, 49);
            builder.WriteInt(b, 49);
            var bits = builder.Build();

            var reader = new BitReader(bits);
            Assert.That(reader.PreloadInt(49), Is.EqualTo(a));
            Assert.That(reader.LoadInt(49), Is.EqualTo(a));
            Assert.That(reader.PreloadInt(49), Is.EqualTo(b));
            Assert.That(reader.LoadInt(49), Is.EqualTo(b));
        }
    }

    [Test]
    public void Test_ReadVarUintsFromBuilder()
    {
        var random = new Random(44);
        for (int i = 0; i < 100; i++)
        {
            int sizeBits = random.Next(4, 9);
            long a = random.NextInt64(0, 281474976710655);
            long b = random.NextInt64(0, 281474976710655);
            
            var builder = new BitBuilder();
            builder.WriteVarUint(a, sizeBits);
            builder.WriteVarUint(b, sizeBits);
            var bits = builder.Build();

            var reader = new BitReader(bits);
            Assert.That(reader.LoadVarUint(sizeBits), Is.EqualTo(a));
            Assert.That(reader.LoadVarUint(sizeBits), Is.EqualTo(b));
        }
    }

    [Test]
    public void Test_ReadVarIntsFromBuilder()
    {
        var random = new Random(45);
        for (int i = 0; i < 100; i++)
        {
            int sizeBits = random.Next(4, 9);
            long a = random.NextInt64(-281474976710655, 281474976710655);
            long b = random.NextInt64(-281474976710655, 281474976710655);
            
            var builder = new BitBuilder();
            builder.WriteVarInt(a, sizeBits);
            builder.WriteVarInt(b, sizeBits);
            var bits = builder.Build();

            var reader = new BitReader(bits);
            Assert.That(reader.LoadVarInt(sizeBits), Is.EqualTo(a));
            Assert.That(reader.LoadVarInt(sizeBits), Is.EqualTo(b));
        }
    }

    [Test]
    public void Test_ReadCoinsFromBuilder()
    {
        var random = new Random(46);
        for (int i = 0; i < 100; i++)
        {
            long a = random.NextInt64(0, 281474976710655);
            long b = random.NextInt64(0, 281474976710655);
            
            var builder = new BitBuilder();
            builder.WriteCoins(a);
            builder.WriteCoins(b);
            var bits = builder.Build();

            var reader = new BitReader(bits);
            Assert.That((long)reader.LoadCoins(), Is.EqualTo(a));
            Assert.That((long)reader.LoadCoins(), Is.EqualTo(b));
        }
    }

    [Test]
    public void Test_ReadAddressFromBuilder()
    {
        var random = new Random(47);
        for (int i = 0; i < 50; i++)
        {
            // Sometimes null address
            Address? a = random.Next(20) == 0 ? null : GenerateTestAddress(random, 0);
            Address b = GenerateTestAddress(random, 0);
            
            var builder = new BitBuilder();
            builder.WriteAddress(a);
            builder.WriteAddress(b);
            var bits = builder.Build();

            var reader = new BitReader(bits);
            var loadedA = reader.LoadAddress();
            if (a != null)
            {
                Assert.That(loadedA!.ToString(), Is.EqualTo(a.ToString()));
            }
            else
            {
                Assert.That(loadedA, Is.Null);
            }
            
            var loadedB = reader.LoadAddress();
            Assert.That(loadedB!.ToString(), Is.EqualTo(b.ToString()));
        }
    }

    [Test]
    public void Test_SkipAndReset()
    {
        var builder = new BitBuilder();
        builder.WriteUint(12345, 32);
        builder.WriteUint(67890, 32);
        var bits = builder.Build();

        var reader = new BitReader(bits);
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
        var builder = new BitBuilder();
        builder.WriteBit(true);
        builder.WriteBit(false);
        builder.WriteBit(true);
        builder.WriteBit(true);
        var bits = builder.Build();

        var reader = new BitReader(bits);
        Assert.That(reader.PreloadBit(), Is.True);
        Assert.That(reader.LoadBit(), Is.True);
        Assert.That(reader.LoadBit(), Is.False);
        Assert.That(reader.LoadBit(), Is.True);
        Assert.That(reader.LoadBit(), Is.True);
        Assert.That(reader.Remaining, Is.EqualTo(0));
    }

    static Address GenerateTestAddress(Random random, int workchain)
    {
        byte[] hash = new byte[32];
        random.NextBytes(hash);
        return new Address(workchain, hash);
    }
}

