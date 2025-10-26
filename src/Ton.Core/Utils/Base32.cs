namespace Ton.Core.Utils;

/// <summary>
/// Provides Base32 encoding and decoding functionality.
/// </summary>
public static class Base32
{
    const string Alphabet = "abcdefghijklmnopqrstuvwxyz234567";

    /// <summary>
    /// Encodes a byte array to a Base32 string.
    /// </summary>
    /// <param name="data">The data to encode.</param>
    /// <returns>The Base32-encoded string.</returns>
    public static string Encode(byte[] data)
    {
        int length = data.Length;
        int bits = 0;
        int value = 0;
        string output = "";

        for (int i = 0; i < length; i++)
        {
            value = (value << 8) | data[i];
            bits += 8;

            while (bits >= 5)
            {
                output += Alphabet[(value >> (bits - 5)) & 31];
                bits -= 5;
            }
        }
        
        if (bits > 0)
        {
            output += Alphabet[(value << (5 - bits)) & 31];
        }
        
        return output;
    }

    /// <summary>
    /// Decodes a Base32 string to a byte array.
    /// </summary>
    /// <param name="input">The Base32-encoded string to decode.</param>
    /// <returns>The decoded byte array.</returns>
    /// <exception cref="ArgumentException">Thrown when an invalid character is found.</exception>
    public static byte[] Decode(string input)
    {
        string cleanedInput = input.ToLower();
        int length = cleanedInput.Length;

        int bits = 0;
        int value = 0;
        int index = 0;
        byte[] output = new byte[(length * 5) / 8];

        for (int i = 0; i < length; i++)
        {
            int charValue = ReadChar(cleanedInput[i]);
            value = (value << 5) | charValue;
            bits += 5;

            if (bits >= 8)
            {
                output[index++] = (byte)((value >> (bits - 8)) & 255);
                bits -= 8;
            }
        }

        return output;
    }

    static int ReadChar(char ch)
    {
        int idx = Alphabet.IndexOf(ch);
        
        if (idx == -1)
            throw new ArgumentException($"Invalid character found: {ch}");
        
        return idx;
    }
}

