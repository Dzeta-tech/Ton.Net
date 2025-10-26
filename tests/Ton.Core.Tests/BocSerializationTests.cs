using System.Numerics;
using NUnit.Framework;
using Ton.Core.Addresses;
using Ton.Core.Boc;

namespace Ton.Core.Tests;

[TestFixture]
public class BocSerializationTests
{
    [Test]
    public void Test_SerializeDeserialize_EmptyCell()
    {
        Cell empty = Cell.Empty;
        byte[] boc = empty.ToBoc();
        Cell[] restored = Cell.FromBoc(boc);

        Assert.That(restored.Length, Is.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(restored[0].Bits.Length, Is.EqualTo(0));
            Assert.That(restored[0].Refs.Length, Is.EqualTo(0));
            Assert.That(restored[0].Hash().SequenceEqual(empty.Hash()), Is.True);
        });
    }

    [Test]
    public void Test_SerializeDeserialize_SimpleCell()
    {
        Cell cell = Builder.BeginCell()
            .StoreUint(123, 32)
            .StoreUint(456, 32)
            .EndCell();

        byte[] boc = cell.ToBoc();
        Cell[] restored = Cell.FromBoc(boc);

        Assert.That(restored.Length, Is.EqualTo(1));
        Assert.That(restored[0].Hash().SequenceEqual(cell.Hash()), Is.True);

        Slice slice = restored[0].BeginParse();
        Assert.Multiple(() =>
        {
            Assert.That(slice.LoadUint(32), Is.EqualTo(123UL));
            Assert.That(slice.LoadUint(32), Is.EqualTo(456UL));
        });
    }

    [Test]
    public void Test_SerializeDeserialize_CellWithRefs()
    {
        Cell child1 = Builder.BeginCell().StoreUint(111, 32).EndCell();
        Cell child2 = Builder.BeginCell().StoreUint(222, 32).EndCell();

        Cell parent = Builder.BeginCell()
            .StoreUint(999, 32)
            .StoreRef(child1)
            .StoreRef(child2)
            .EndCell();

        byte[] boc = parent.ToBoc();
        Cell[] restored = Cell.FromBoc(boc);

        Assert.That(restored.Length, Is.EqualTo(1));
        Assert.That(restored[0].Hash().SequenceEqual(parent.Hash()), Is.True);

        Slice slice = restored[0].BeginParse();
        Assert.That(slice.LoadUint(32), Is.EqualTo(999UL));
        Assert.That(slice.LoadRef().BeginParse().LoadUint(32), Is.EqualTo(111UL));
        Assert.That(slice.LoadRef().BeginParse().LoadUint(32), Is.EqualTo(222UL));
    }

    [Test]
    public void Test_SerializeDeserialize_NestedCells()
    {
        Cell level3 = Builder.BeginCell().StoreUint(3, 8).EndCell();
        Cell level2 = Builder.BeginCell().StoreUint(2, 8).StoreRef(level3).EndCell();
        Cell level1 = Builder.BeginCell().StoreUint(1, 8).StoreRef(level2).EndCell();

        byte[] boc = level1.ToBoc();
        Cell[] restored = Cell.FromBoc(boc);

        Assert.That(restored.Length, Is.EqualTo(1));
        Assert.That(restored[0].Hash().SequenceEqual(level1.Hash()), Is.True);

        Slice s1 = restored[0].BeginParse();
        Assert.That(s1.LoadUint(8), Is.EqualTo(1UL));

        Slice s2 = s1.LoadRef().BeginParse();
        Assert.That(s2.LoadUint(8), Is.EqualTo(2UL));

        Slice s3 = s2.LoadRef().BeginParse();
        Assert.That(s3.LoadUint(8), Is.EqualTo(3UL));
    }

    [Test]
    public void Test_SerializeDeserialize_WithAddress()
    {
        Address addr = new(0, new byte[32]);
        Cell cell = Builder.BeginCell()
            .StoreAddress(addr)
            .StoreUint(42, 32)
            .EndCell();

        byte[] boc = cell.ToBoc();
        Cell[] restored = Cell.FromBoc(boc);

        Assert.That(restored.Length, Is.EqualTo(1));

        Slice slice = restored[0].BeginParse();
        Address? restoredAddr = slice.LoadAddress();
        Assert.That(restoredAddr?.ToString(), Is.EqualTo(addr.ToString()));
        Assert.That(slice.LoadUint(32), Is.EqualTo(42UL));
    }

    [Test]
    public void Test_SerializeDeserialize_WithCoins()
    {
        BigInteger coins = BigInteger.Parse("12345678901234567890");
        Cell cell = Builder.BeginCell()
            .StoreCoins(coins)
            .StoreUint(999, 16)
            .EndCell();

        byte[] boc = cell.ToBoc();
        Cell[] restored = Cell.FromBoc(boc);

        Slice slice = restored[0].BeginParse();
        Assert.Multiple(() =>
        {
            Assert.That(slice.LoadCoins(), Is.EqualTo(coins));
            Assert.That(slice.LoadUint(16), Is.EqualTo(999UL));
        });
    }

    [Test]
    public void Test_SerializeDeserialize_NonByteAlignedBits()
    {
        // Test cells with non-byte-aligned bit lengths
        for (int bits = 1; bits <= 32; bits++)
        {
            ulong maxValue = bits == 64 ? ulong.MaxValue : (1UL << bits) - 1;
            Cell cell = Builder.BeginCell()
                .StoreUint(maxValue, bits)
                .EndCell();

            byte[] boc = cell.ToBoc();
            Cell[] restored = Cell.FromBoc(boc);

            Assert.That(restored.Length, Is.EqualTo(1));
            Assert.That(restored[0].Bits.Length, Is.EqualTo(bits));
            Assert.That(restored[0].Hash().SequenceEqual(cell.Hash()), Is.True);
        }
    }

    [Test]
    public void Test_SerializeDeserialize_MaxRefs()
    {
        Cell ref1 = Builder.BeginCell().StoreUint(1, 8).EndCell();
        Cell ref2 = Builder.BeginCell().StoreUint(2, 8).EndCell();
        Cell ref3 = Builder.BeginCell().StoreUint(3, 8).EndCell();
        Cell ref4 = Builder.BeginCell().StoreUint(4, 8).EndCell();

        Cell cell = Builder.BeginCell()
            .StoreUint(0, 8)
            .StoreRef(ref1)
            .StoreRef(ref2)
            .StoreRef(ref3)
            .StoreRef(ref4)
            .EndCell();

        byte[] boc = cell.ToBoc();
        Cell[] restored = Cell.FromBoc(boc);

        Assert.That(restored.Length, Is.EqualTo(1));
        Assert.That(restored[0].Refs.Length, Is.EqualTo(4));

        Slice slice = restored[0].BeginParse();
        slice.LoadUint(8);
        Assert.Multiple(() =>
        {
            Assert.That(slice.LoadRef().BeginParse().LoadUint(8), Is.EqualTo(1UL));
            Assert.That(slice.LoadRef().BeginParse().LoadUint(8), Is.EqualTo(2UL));
            Assert.That(slice.LoadRef().BeginParse().LoadUint(8), Is.EqualTo(3UL));
            Assert.That(slice.LoadRef().BeginParse().LoadUint(8), Is.EqualTo(4UL));
        });
    }

    [Test]
    public void Test_SerializeDeserialize_SharedRefs()
    {
        // Create a diamond structure: parent -> (child1, child2) -> shared
        Cell shared = Builder.BeginCell().StoreUint(42, 32).EndCell();
        Cell child1 = Builder.BeginCell().StoreUint(1, 8).StoreRef(shared).EndCell();
        Cell child2 = Builder.BeginCell().StoreUint(2, 8).StoreRef(shared).EndCell();
        Cell parent = Builder.BeginCell()
            .StoreRef(child1)
            .StoreRef(child2)
            .EndCell();

        byte[] boc = parent.ToBoc();
        Cell[] restored = Cell.FromBoc(boc);

        Assert.That(restored.Length, Is.EqualTo(1));
        Assert.That(restored[0].Hash().SequenceEqual(parent.Hash()), Is.True);

        Slice parentSlice = restored[0].BeginParse();
        Cell restoredChild1 = parentSlice.LoadRef();
        Cell restoredChild2 = parentSlice.LoadRef();

        Cell restoredShared1 = restoredChild1.BeginParse().Skip(8).LoadRef();
        Cell restoredShared2 = restoredChild2.BeginParse().Skip(8).LoadRef();

        // Verify the shared cell is actually the same reference
        Assert.That(restoredShared1.Hash().SequenceEqual(restoredShared2.Hash()), Is.True);
        Assert.That(restoredShared1.BeginParse().LoadUint(32), Is.EqualTo(42UL));
    }

    [Test]
    public void Test_Serialize_WithAndWithoutCRC()
    {
        Cell cell = Builder.BeginCell().StoreUint(12345, 32).EndCell();

        byte[] bocWithCRC = cell.ToBoc(hasIdx: true, hasCrc32C: true);
        byte[] bocWithoutCRC = cell.ToBoc(hasIdx: true, hasCrc32C: false);

        // BOC with CRC should be 4 bytes longer
        Assert.That(bocWithCRC.Length, Is.EqualTo(bocWithoutCRC.Length + 4));

        // Both should deserialize to the same cell
        Cell[] restored1 = Cell.FromBoc(bocWithCRC);
        Cell[] restored2 = Cell.FromBoc(bocWithoutCRC);

        Assert.That(restored1[0].Hash().SequenceEqual(restored2[0].Hash()), Is.True);
    }

    [Test]
    public void Test_Serialize_WithAndWithoutIndex()
    {
        // Create a cell with multiple refs to test index size impact
        Cell child = Builder.BeginCell().StoreUint(1, 8).EndCell();
        Cell parent = Builder.BeginCell()
            .StoreRef(child)
            .StoreRef(child)
            .StoreRef(child)
            .EndCell();

        byte[] bocWithIdx = parent.ToBoc(hasIdx: true, hasCrc32C: false);
        byte[] bocWithoutIdx = parent.ToBoc(hasIdx: false, hasCrc32C: false);

        // BOC with index should be longer
        Assert.That(bocWithIdx.Length, Is.GreaterThan(bocWithoutIdx.Length));

        // Both should deserialize to the same cell
        Cell[] restored1 = Cell.FromBoc(bocWithIdx);
        Cell[] restored2 = Cell.FromBoc(bocWithoutIdx);

        Assert.That(restored1[0].Hash().SequenceEqual(restored2[0].Hash()), Is.True);
    }

    [Test]
    public void Test_SerializeDeserialize_ComplexStructure()
    {
        // Build a complex nested structure with proper ref limits
        Cell leaf1 = Builder.BeginCell().StoreUint(1, 32).EndCell();
        Cell leaf2 = Builder.BeginCell().StoreUint(2, 32).EndCell();
        Cell leaf3 = Builder.BeginCell().StoreUint(3, 32).EndCell();

        Cell level2a = Builder.BeginCell()
            .StoreRef(leaf1)
            .StoreRef(leaf2)
            .EndCell();

        Cell level2b = Builder.BeginCell()
            .StoreRef(leaf3)
            .StoreRef(leaf1) // Shared ref
            .EndCell();

        Cell root = Builder.BeginCell()
            .StoreUint(999, 32)
            .StoreRef(level2a)
            .StoreRef(level2b)
            .EndCell();

        byte[] boc = root.ToBoc();
        Cell[] restored = Cell.FromBoc(boc);

        Assert.That(restored.Length, Is.EqualTo(1));
        Assert.That(restored[0].Hash().SequenceEqual(root.Hash()), Is.True);
    }

    [Test]
    public void Test_SerializeDeserialize_RoundTrip_Multiple()
    {
        // Test serialization/deserialization multiple times to ensure consistency
        Cell original = Builder.BeginCell()
            .StoreUint(0xDEADBEEF, 32)
            .StoreRef(Builder.BeginCell().StoreUint(0xCAFEBABE, 32).EndCell())
            .EndCell();

        byte[] boc1 = original.ToBoc();
        Cell[] restored1 = Cell.FromBoc(boc1);
        byte[] boc2 = restored1[0].ToBoc();
        Cell[] restored2 = Cell.FromBoc(boc2);
        byte[] boc3 = restored2[0].ToBoc();

        // All BOCs should be identical
        Assert.Multiple(() =>
        {
            Assert.That(boc1.SequenceEqual(boc2), Is.True);
            Assert.That(boc2.SequenceEqual(boc3), Is.True);
            Assert.That(restored2[0].Hash().SequenceEqual(original.Hash()), Is.True);
        });
    }

    [Test]
    public void Test_InvalidBOC_ThrowsException()
    {
        byte[] invalidBoc = [0x00, 0x01, 0x02, 0x03, 0x04];
        Assert.Throws<InvalidOperationException>(() => Cell.FromBoc(invalidBoc));
    }

    [Test]
    public void Test_EmptyBOC_ThrowsException()
    {
        byte[] emptyBoc = [];
        Assert.Throws<ArgumentOutOfRangeException>(() => Cell.FromBoc(emptyBoc));
    }
}

