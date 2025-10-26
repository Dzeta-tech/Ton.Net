namespace Ton.Core.Utils;

/// <summary>
///     Provides CRC-32C (Castagnoli) checksum computation.
/// </summary>
public static class Crc32C
{
    const uint Poly = 0x82f63b78;

    /// <summary>
    ///     Computes the CRC-32C checksum for the given data.
    /// </summary>
    /// <param name="data">The data to compute the checksum for.</param>
    /// <returns>A 4-byte array containing the checksum in little-endian format.</returns>
    public static byte[] Compute(byte[] data)
    {
        uint crc = 0 ^ 0xffffffff;

        for (int n = 0; n < data.Length; n++)
        {
            crc ^= data[n];
            for (int i = 0; i < 8; i++) crc = (crc & 1) != 0 ? (crc >> 1) ^ Poly : crc >> 1;
        }

        crc = crc ^ 0xffffffff;

        // Convert to little-endian
        byte[] result = new byte[4];
        result[0] = (byte)(crc & 0xFF);
        result[1] = (byte)((crc >> 8) & 0xFF);
        result[2] = (byte)((crc >> 16) & 0xFF);
        result[3] = (byte)((crc >> 24) & 0xFF);

        return result;
    }
}