namespace Ton.Core.Boc;

/// <summary>
///     Level mask for cells - used to track cell levels in Merkle structures.
/// </summary>
public class LevelMask
{
    /// <summary>
    ///     Creates a new level mask.
    /// </summary>
    /// <param name="mask">Mask value.</param>
    public LevelMask(int mask = 0)
    {
        Value = mask;
        HashIndex = CountSetBits(Value);
        HashCount = HashIndex + 1;
    }

    /// <summary>
    ///     Gets the mask value.
    /// </summary>
    public int Value { get; }

    /// <summary>
    ///     Gets the level (highest bit set).
    /// </summary>
    public int Level => 32 - LeadingZeroCount(Value);

    /// <summary>
    ///     Gets the hash index.
    /// </summary>
    public int HashIndex { get; }

    /// <summary>
    ///     Gets the hash count.
    /// </summary>
    public int HashCount { get; }

    /// <summary>
    ///     Apply mask to specific level.
    /// </summary>
    /// <param name="level">Level to apply.</param>
    /// <returns>New level mask.</returns>
    public LevelMask Apply(int level)
    {
        return new LevelMask(Value & ((1 << level) - 1));
    }

    /// <summary>
    ///     Check if level is significant.
    /// </summary>
    /// <param name="level">Level to check.</param>
    /// <returns>True if significant.</returns>
    public bool IsSignificant(int level)
    {
        return level == 0 || (Value >> (level - 1)) % 2 != 0;
    }

    static int CountSetBits(int n)
    {
        n = n - ((n >> 1) & 0x55555555);
        n = (n & 0x33333333) + ((n >> 2) & 0x33333333);
        return (((n + (n >> 4)) & 0xF0F0F0F) * 0x1010101) >> 24;
    }

    static int LeadingZeroCount(int value)
    {
        if (value == 0) return 32;

        int count = 0;
        if ((value & 0xFFFF0000) == 0)
        {
            count += 16;
            value <<= 16;
        }

        if ((value & 0xFF000000) == 0)
        {
            count += 8;
            value <<= 8;
        }

        if ((value & 0xF0000000) == 0)
        {
            count += 4;
            value <<= 4;
        }

        if ((value & 0xC0000000) == 0)
        {
            count += 2;
            value <<= 2;
        }

        if ((value & 0x80000000) == 0) count += 1;

        return count;
    }
}