using System;
using NUnit.Framework;
using Ton.Core.Boc;

namespace Ton.Core.Tests;

[TestFixture]
public class CellExoticTests
{
    static Cell CreatePrunedBranchCell(int bitsLength)
    {
        int bytes = (bitsLength + 7) / 8;
        byte[] data = new byte[bytes];
        // Set exotic type = 1 (PrunedBranch) in the first 8 bits
        data[0] = 0x01;
        BitString bits = new BitString(data, 0, bitsLength);
        return new Cell(bits, Array.Empty<Cell>(), exotic: true);
    }

    [Test]
    public void PrunedBranch_Level1_Special_280Bits_ShouldValidate()
    {
        Cell c = CreatePrunedBranchCell(280);
        Assert.That(c.Type, Is.EqualTo(CellType.PrunedBranch));
        Assert.That(c.Refs.Length, Is.EqualTo(0));
        Assert.That(c.Bits.Length, Is.EqualTo(280));
    }

    [Test]
    public void PrunedBranch_Level1_Standard_288Bits_ShouldValidate()
    {
        Cell c = CreatePrunedBranchCell(288);
        Assert.That(c.Type, Is.EqualTo(CellType.PrunedBranch));
        Assert.That(c.Refs.Length, Is.EqualTo(0));
        Assert.That(c.Bits.Length, Is.EqualTo(288));
    }

    [Test]
    public void PrunedBranch_Level2_560Bits_ShouldValidate()
    {
        Cell c = CreatePrunedBranchCell(560);
        Assert.That(c.Type, Is.EqualTo(CellType.PrunedBranch));
        Assert.That(c.Refs.Length, Is.EqualTo(0));
        Assert.That(c.Bits.Length, Is.EqualTo(560));
    }

    [Test]
    public void PrunedBranch_Level3_832Bits_ShouldValidate()
    {
        Cell c = CreatePrunedBranchCell(832);
        Assert.That(c.Type, Is.EqualTo(CellType.PrunedBranch));
        Assert.That(c.Refs.Length, Is.EqualTo(0));
        Assert.That(c.Bits.Length, Is.EqualTo(832));
    }
}


