using System.Text;
using Ton.Crypto.Ed25519;
using Ton.Crypto.Primitives;

namespace Ton.Crypto.Mnemonic;

/// <summary>
///     TON mnemonic phrase utilities following BIP39 standard.
/// </summary>
public static class Mnemonic
{
    const int PbkdfIterations = 100000;

    /// <summary>
    ///     Generates a new random mnemonic phrase.
    /// </summary>
    /// <param name="wordsCount">Number of words (default: 24).</param>
    /// <param name="password">Optional password for additional security.</param>
    /// <returns>Array of mnemonic words.</returns>
    public static string[] New(int wordsCount = 24, string? password = null)
    {
        while (true)
        {
            // Generate random mnemonic
            string[] mnemonicArray = new string[wordsCount];
            for (int i = 0; i < wordsCount; i++)
            {
                int index = SecureRandom.GetNumber(0, Wordlist.Words.Length);
                mnemonicArray[i] = Wordlist.Words[index];
            }

            // Check password conformance
            if (!string.IsNullOrEmpty(password))
                if (!IsPasswordNeeded(mnemonicArray))
                    continue;

            // Check if basic seed correct
            if (!IsBasicSeed(ToEntropy(mnemonicArray, password)))
                continue;

            return mnemonicArray;
        }
    }

    /// <summary>
    ///     Validates a mnemonic phrase.
    /// </summary>
    /// <param name="mnemonicArray">Mnemonic words to validate.</param>
    /// <param name="password">Optional password.</param>
    /// <returns>True if the mnemonic is valid.</returns>
    public static bool Validate(string[] mnemonicArray, string? password = null)
    {
        // Normalize
        mnemonicArray = Normalize(mnemonicArray);

        // Validate mnemonic words
        foreach (string word in mnemonicArray)
            if (Array.IndexOf(Wordlist.Words, word) < 0)
                return false;

        // Check password
        if (!string.IsNullOrEmpty(password))
            if (!IsPasswordNeeded(mnemonicArray))
                return false;

        // Validate seed
        return IsBasicSeed(ToEntropy(mnemonicArray, password));
    }

    /// <summary>
    ///     Converts mnemonic phrase to entropy.
    /// </summary>
    /// <param name="mnemonicArray">Mnemonic words.</param>
    /// <param name="password">Optional password.</param>
    /// <returns>64-byte entropy.</returns>
    public static byte[] ToEntropy(string[] mnemonicArray, string? password = null)
    {
        // https://github.com/ton-blockchain/ton/blob/24dc184a2ea67f9c47042b4104bbb4d82289fac1/tonlib/tonlib/keys/Mnemonic.cpp#L52
        string mnemonicStr = string.Join(" ", mnemonicArray);
        string passwordStr = !string.IsNullOrEmpty(password) ? password : "";
        return HmacSha512.Hash(Encoding.UTF8.GetBytes(mnemonicStr), Encoding.UTF8.GetBytes(passwordStr));
    }

    /// <summary>
    ///     Converts mnemonic phrase to seed.
    /// </summary>
    /// <param name="mnemonicArray">Mnemonic words.</param>
    /// <param name="seed">Seed string (e.g., "TON default seed").</param>
    /// <param name="password">Optional password.</param>
    /// <returns>64-byte seed.</returns>
    public static byte[] ToSeed(string[] mnemonicArray, string seed, string? password = null)
    {
        // https://github.com/ton-blockchain/ton/blob/24dc184a2ea67f9c47042b4104bbb4d82289fac1/tonlib/tonlib/keys/Mnemonic.cpp#L58
        byte[] entropy = ToEntropy(mnemonicArray, password);
        return Pbkdf2Sha512.DeriveKey(entropy, Encoding.UTF8.GetBytes(seed), PbkdfIterations, 64);
    }

    /// <summary>
    ///     Extracts private key from mnemonic phrase.
    /// </summary>
    /// <param name="mnemonicArray">Mnemonic words.</param>
    /// <param name="password">Optional password.</param>
    /// <returns>Ed25519 key pair.</returns>
    public static KeyPair ToPrivateKey(string[] mnemonicArray, string? password = null)
    {
        // https://github.com/ton-blockchain/ton/blob/24dc184a2ea67f9c47042b4104bbb4d82289fac1/tonlib/tonlib/keys/Mnemonic.cpp#L64
        mnemonicArray = Normalize(mnemonicArray);
        byte[] seed = ToSeed(mnemonicArray, "TON default seed", password);
        byte[] keySeed = new byte[32];
        Array.Copy(seed, 0, keySeed, 0, 32);
        return Ed25519.Ed25519.KeyPairFromSeed(keySeed);
    }

    /// <summary>
    ///     Converts mnemonic to wallet key pair.
    /// </summary>
    /// <param name="mnemonicArray">Mnemonic words.</param>
    /// <param name="password">Optional password.</param>
    /// <returns>Ed25519 key pair.</returns>
    public static KeyPair ToWalletKey(string[] mnemonicArray, string? password = null)
    {
        KeyPair seedPk = ToPrivateKey(mnemonicArray, password);
        byte[] seedSecret = new byte[32];
        Array.Copy(seedPk.SecretKey, 0, seedSecret, 0, 32);
        return Ed25519.Ed25519.KeyPairFromSeed(seedSecret);
    }

    /// <summary>
    ///     Converts mnemonic to HD seed.
    /// </summary>
    /// <param name="mnemonicArray">Mnemonic words.</param>
    /// <param name="password">Optional password.</param>
    /// <returns>64-byte HD seed.</returns>
    public static byte[] ToHdSeed(string[] mnemonicArray, string? password = null)
    {
        mnemonicArray = Normalize(mnemonicArray);
        return ToSeed(mnemonicArray, "TON HD Keys seed", password);
    }

    /// <summary>
    ///     Generates deterministic mnemonic from seed.
    /// </summary>
    /// <param name="seed">Random seed buffer.</param>
    /// <param name="wordsCount">Number of words (default: 24).</param>
    /// <param name="password">Optional password.</param>
    /// <returns>Array of mnemonic words.</returns>
    public static string[] FromRandomSeed(byte[] seed, int wordsCount = 24, string? password = null)
    {
        int bytesLength = (int)Math.Ceiling(wordsCount * 11 / 8.0);
        byte[] currentSeed = seed;

        while (true)
        {
            // Create entropy
            byte[] entropy = Pbkdf2Sha512.DeriveKey(
                currentSeed,
                Encoding.UTF8.GetBytes("TON mnemonic seed"),
                Math.Max(1, PbkdfIterations / 256),
                bytesLength
            );

            // Create mnemonics
            string[] mnemonics = BytesToMnemonics(entropy, wordsCount);

            // Check if mnemonics are valid
            if (Validate(mnemonics, password))
                return mnemonics;

            currentSeed = entropy;
        }
    }

    /// <summary>
    ///     Converts bytes to mnemonic words (could be invalid for TON).
    /// </summary>
    /// <param name="src">Source buffer.</param>
    /// <param name="wordsCount">Number of words.</param>
    /// <returns>Array of mnemonic words.</returns>
    public static string[] BytesToMnemonics(byte[] src, int wordsCount)
    {
        int[] indexes = BytesToMnemonicIndexes(src, wordsCount);
        string[] result = new string[indexes.Length];
        for (int i = 0; i < indexes.Length; i++) result[i] = Wordlist.Words[indexes[i]];

        return result;
    }

    /// <summary>
    ///     Converts bytes to mnemonic indexes.
    /// </summary>
    /// <param name="src">Source buffer.</param>
    /// <param name="wordsCount">Number of words.</param>
    /// <returns>Array of word indexes.</returns>
    public static int[] BytesToMnemonicIndexes(byte[] src, int wordsCount)
    {
        string bits = BytesToBits(src);
        int[] indexes = new int[wordsCount];
        for (int i = 0; i < wordsCount; i++)
        {
            string slice = bits.Substring(i * 11, 11);
            indexes[i] = Convert.ToInt32(slice, 2);
        }

        return indexes;
    }

    /// <summary>
    ///     Converts mnemonic indexes to bytes with zero padding.
    /// </summary>
    /// <param name="indexes">Source indexes.</param>
    /// <returns>Buffer.</returns>
    public static byte[] MnemonicIndexesToBytes(int[] indexes)
    {
        StringBuilder res = new();
        foreach (int index in indexes)
        {
            if (index is < 0 or >= 2048)
                throw new ArgumentException("Invalid input");

            res.Append(Convert.ToString(index, 2).PadLeft(11, '0'));
        }

        while (res.Length % 8 != 0) res.Append('0');

        return BitsToBytes(res.ToString());
    }

    #region Private Methods

    static string[] Normalize(string[] src)
    {
        return src.Select(v => v.ToLower().Trim()).ToArray();
    }

    static bool IsPasswordNeeded(string[] mnemonicArray)
    {
        byte[] passlessEntropy = ToEntropy(mnemonicArray);
        return IsPasswordSeed(passlessEntropy) && !IsBasicSeed(passlessEntropy);
    }

    static bool IsBasicSeed(byte[] entropy)
    {
        // https://github.com/ton-blockchain/ton/blob/24dc184a2ea67f9c47042b4104bbb4d82289fac1/tonlib/tonlib/keys/Mnemonic.cpp#L68
        byte[] seed = Pbkdf2Sha512.DeriveKey(
            entropy,
            Encoding.UTF8.GetBytes("TON seed version"),
            Math.Max(1, PbkdfIterations / 256),
            64
        );
        return seed[0] == 0;
    }

    static bool IsPasswordSeed(byte[] entropy)
    {
        // https://github.com/ton-blockchain/ton/blob/24dc184a2ea67f9c47042b4104bbb4d82289fac1/tonlib/tonlib/keys/Mnemonic.cpp#L75
        byte[] seed = Pbkdf2Sha512.DeriveKey(
            entropy,
            Encoding.UTF8.GetBytes("TON fast seed version"),
            1,
            64
        );
        return seed[0] == 1;
    }

    static string BytesToBits(byte[] bytes)
    {
        StringBuilder res = new();
        foreach (byte b in bytes) res.Append(Convert.ToString(b, 2).PadLeft(8, '0'));

        return res.ToString();
    }

    static byte[] BitsToBytes(string src)
    {
        if (src.Length % 8 != 0)
            throw new ArgumentException("Uneven bits");

        List<byte> result = [];
        while (src.Length > 0)
        {
            result.Add(Convert.ToByte(src.Substring(0, 8), 2));
            src = src.Substring(8);
        }

        return result.ToArray();
    }

    #endregion
}