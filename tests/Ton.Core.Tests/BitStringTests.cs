using Ton.Core.Boc;

namespace Ton.Core.Tests;

[TestFixture]
public class BitStringTests
{
    [Test]
    public void Test_ReadBits()
    {
        BitString bs = new([0b10101010], 0, 8);
        Assert.Multiple(() =>
        {
            Assert.That(bs.At(0), Is.True);
            Assert.That(bs.At(1), Is.False);
            Assert.That(bs.At(2), Is.True);
            Assert.That(bs.At(3), Is.False);
            Assert.That(bs.At(4), Is.True);
            Assert.That(bs.At(5), Is.False);
            Assert.That(bs.At(6), Is.True);
            Assert.That(bs.At(7), Is.False);
            Assert.That(bs.ToString(), Is.EqualTo("AA"));
        });
    }

    [Test]
    public void Test_Equals()
    {
        BitString a = new([0b10101010], 0, 8);
        BitString b = new([0b10101010], 0, 8);
        BitString c = new([0, 0b10101010], 8, 8);

        Assert.Multiple(() =>
        {
            Assert.That(a, Is.EqualTo(b));
            Assert.That(b, Is.EqualTo(a));
            Assert.That(a, Is.EqualTo(c));
            Assert.That(c, Is.EqualTo(a));
            Assert.That(a.ToString(), Is.EqualTo("AA"));
            Assert.That(b.ToString(), Is.EqualTo("AA"));
            Assert.That(c.ToString(), Is.EqualTo("AA"));
        });
    }

    [Test]
    public void Test_FormatStrings()
    {
        Assert.Multiple(() =>
        {
            Assert.That(new BitString([0b00000000], 0, 1).ToString(), Is.EqualTo("4_"));
            Assert.That(new BitString([0b10000000], 0, 1).ToString(), Is.EqualTo("C_"));
            Assert.That(new BitString([0b11000000], 0, 2).ToString(), Is.EqualTo("E_"));
            Assert.That(new BitString([0b11100000], 0, 3).ToString(), Is.EqualTo("F_"));
            Assert.That(new BitString([0b11100000], 0, 4).ToString(), Is.EqualTo("E"));
            Assert.That(new BitString([0b11101000], 0, 5).ToString(), Is.EqualTo("EC_"));
        });
    }

    [Test]
    public void Test_Subbuffers()
    {
        BitString bs = new([1, 2, 3, 4, 5, 6, 7, 8], 0, 64);
        byte[]? bs2 = bs.Subbuffer(0, 16);
        Assert.That(bs2!, Has.Length.EqualTo(2));
    }

    [Test]
    public void Test_Substrings()
    {
        BitString bs = new([1, 2, 3, 4, 5, 6, 7, 8], 0, 64);
        BitString bs2 = bs.Substring(0, 16);
        Assert.That(bs2.Length, Is.EqualTo(16));
    }

    [Test]
    public void Test_EmptySubstringsWithLength0()
    {
        BitString bs = new([1, 2, 3, 4, 5, 6, 7, 8], 0, 64);
        BitString bs2 = bs.Substring(bs.Length, 0);
        Assert.That(bs2.Length, Is.EqualTo(0));
    }

    [Test]
    public void Test_OOB_WhenSubstringOffsetOutOfBounds()
    {
        BitString bs = new([1, 2, 3, 4, 5, 6, 7, 8], 0, 64);

        ArgumentOutOfRangeException? ex1 =
            Assert.Throws<ArgumentOutOfRangeException>(() => bs.Substring(bs.Length + 1, 0));
        Assert.That(ex1!.Message, Does.Contain("out of bounds"));

        ArgumentOutOfRangeException? ex2 = Assert.Throws<ArgumentOutOfRangeException>(() => bs.Substring(-1, 0));
        Assert.That(ex2!.Message, Does.Contain("out of bounds"));
    }

    [Test]
    public void Test_OOB_WhenSubbufferOffsetOutOfBounds()
    {
        BitString bs = new([1, 2, 3, 4, 5, 6, 7, 8], 0, 64);

        ArgumentOutOfRangeException? ex1 =
            Assert.Throws<ArgumentOutOfRangeException>(() => bs.Subbuffer(bs.Length + 1, 0));
        Assert.That(ex1!.Message, Does.Contain("out of bounds"));

        ArgumentOutOfRangeException? ex2 = Assert.Throws<ArgumentOutOfRangeException>(() => bs.Subbuffer(-1, 0));
        Assert.That(ex2!.Message, Does.Contain("out of bounds"));
    }

    [Test]
    public void Test_OOB_WhenOffsetAtEndAndLengthGreaterThan0()
    {
        BitString bs = new([1, 2, 3, 4, 5, 6, 7, 8], 0, 64);

        ArgumentOutOfRangeException? ex = Assert.Throws<ArgumentOutOfRangeException>(() => bs.Substring(bs.Length, 1));
        Assert.That(ex!.Message, Does.Contain("out of bounds"));
    }

    [Test]
    public void Test_EmptySubbuffersWithLength0()
    {
        BitString bs = new([1, 2, 3, 4, 5, 6, 7, 8], 0, 64);
        byte[]? bs2 = bs.Subbuffer(bs.Length, 0);
        Assert.That(bs2!.Length, Is.EqualTo(0));
    }

    [Test]
    public void Test_MonkeyStrings()
    {
        (string bits, string expected)[] cases =
        [
            ("001110101100111010", "3ACEA_"),
            ("01001", "4C_"),
            ("000000110101101010", "035AA_"),
            ("1000011111100010111110111", "87E2FBC_"),
            ("0111010001110010110", "7472D_"),
            ("", ""),
            ("0101", "5"),
            ("010110111010100011110101011110", "5BA8F57A_"),
            ("00110110001101", "3636_"),
            ("1110100", "E9_"),
            ("010111000110110", "5C6D_"),
            ("01", "6_"),
            ("1000010010100", "84A4_"),
            ("010000010", "414_"),
            ("110011111", "CFC_"),
            ("11000101001101101", "C536C_"),
            ("011100111", "73C_"),
            ("11110011", "F3"),
            ("011001111011111000", "67BE2_"),
            ("10101100000111011111", "AC1DF"),
            ("0100001000101110", "422E"),
            ("000110010011011101", "19376_"),
            ("10111001", "B9"),
            ("011011000101000001001001110000", "6C5049C2_"),
            ("0100011101", "476_"),
            ("01001101000001", "4D06_"),
            ("00010110101", "16B_"),
            ("01011011110", "5BD_"),
            ("1010101010111001011101", "AAB976_"),
            ("00011", "1C_"),
            ("11011111111001111100", "DFE7C"),
            ("1110100100110111001101011111000", "E93735F1_"),
            ("10011110010111100110100000", "9E5E682_"),
            ("00100111110001100111001110", "27C673A_"),
            ("01010111011100000000001110000", "57700384_"),
            ("010000001011111111111000", "40BFF8"),
            ("0011110001111000110101100001", "3C78D61"),
            ("101001011011000010", "A5B0A_"),
            ("1111", "F"),
            ("10101110", "AE"),
            ("1001", "9"),
            ("001010010", "294_"),
            ("110011", "CE_"),
            ("10000000010110", "805A_"),
            ("11000001101000100", "C1A24_"),
            ("1", "C_"),
            ("0100101010000010011101111", "4A8277C_"),
            ("10", "A_"),
            ("1010110110110110110100110010110", "ADB6D32D_"),
            ("010100000000001000111101011001", "50023D66_")
        ];

        foreach ((string bits, string expected) in cases)
        {
            // Build string
            BitBuilder builder = new();
            foreach (char c in bits) builder.WriteBit(c == '1');
            BitString result = builder.Build();

            // Check that string is valid
            for (int i = 0; i < bits.Length; i++) Assert.That(result.At(i), Is.EqualTo(bits[i] == '1'));

            // Check toString
            Assert.That(result.ToString(), Is.EqualTo(expected));
        }
    }
}