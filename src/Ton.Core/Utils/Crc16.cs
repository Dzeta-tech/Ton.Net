namespace Ton.Core.Utils;

/// <summary>
///     Provides CRC-16 checksum computation for TON addresses.
/// </summary>
public static class Crc16
{
    /// <summary>
    ///     Computes the CRC-16 checksum for the given data using the XMODEM polynomial (0x1021).
    /// </summary>
    /// <param name="data">The data to compute the checksum for.</param>
    /// <returns>A 2-byte array containing the checksum.</returns>
    public static byte[] Compute(byte[] data)
    {
        const int poly = 0x1021;
        int reg = 0;

        byte[] message = new byte[data.Length + 2];
        Array.Copy(data, message, data.Length);

        foreach (byte b in message)
        {
            int mask = 0x80;
            while (mask > 0)
            {
                reg <<= 1;
                if ((b & mask) != 0) reg += 1;
                mask >>= 1;
                if (reg > 0xffff)
                {
                    reg &= 0xffff;
                    reg ^= poly;
                }
            }
        }

        return [(byte)(reg / 256), (byte)(reg % 256)];
    }
}