using System.IO.MemoryMappedFiles;
using System.Text;
using Duey.Provider.WZ.Crypto;

namespace Duey.Provider.WZ.Tests;

/// <summary>
/// Shared binary-building helpers used across all WZ test classes.
/// </summary>
internal static class WZTestHelpers
{
    /// <summary>
    /// Wraps a byte array in an anonymous memory-mapped file for use as a WZ view.
    /// </summary>
    internal static MemoryMappedFile CreateMemoryMappedFile(byte[] data)
    {
        var mmf = MemoryMappedFile.CreateNew(null, data.Length);
        using var stream = mmf.CreateViewStream(0, data.Length, MemoryMappedFileAccess.Write);
        stream.Write(data, 0, data.Length);
        return mmf;
    }

    /// <summary>
    /// Writes a WZ inline string block (type 0x00) with ASCII encoding.
    ///
    /// Encoding pipeline (reverse of WZReader decryption):
    ///   plaintext → cipher XOR keystream → XOR incrementing mask (0xAA+i) → raw bytes
    /// </summary>
    internal static void WriteInlineStringBlock(BinaryWriter writer, string value, XORCipher cipher)
    {
        writer.Write((byte)0x00); // string block type: inline

        if (string.IsNullOrEmpty(value))
        {
            writer.Write((sbyte)0);
            return;
        }

        var plaintext = Encoding.ASCII.GetBytes(value);
        writer.Write((sbyte)(-plaintext.Length)); // negative length = ASCII encoding

        var encrypted = new byte[plaintext.Length];
        Buffer.BlockCopy(plaintext, 0, encrypted, 0, plaintext.Length);
        cipher.Transform(encrypted);

        byte mask = 0xAA;
        for (var i = 0; i < encrypted.Length; i++)
        {
            encrypted[i] ^= mask;
            mask++;
        }

        writer.Write(encrypted);
    }

    /// <summary>
    /// Writes a WZ compressed integer:
    ///   values in [-127, 127] → 1 byte (sbyte)
    ///   values outside that range → 0x80 sentinel + 4-byte LE int
    /// </summary>
    internal static void WriteCompressedInt(BinaryWriter writer, int value)
    {
        if (value >= -127 && value <= 127)
            writer.Write((sbyte)value);
        else
        {
            writer.Write((sbyte)(-128));
            writer.Write(value);
        }
    }

    /// <summary>
    /// Pads a byte array to at least <paramref name="minSize"/> bytes so that
    /// memory-mapped view streams do not read past the end of the buffer.
    /// </summary>
    internal static byte[] PadToMinimumSize(byte[] data, int minSize)
    {
        if (data.Length >= minSize) return data;
        var padded = new byte[minSize];
        Buffer.BlockCopy(data, 0, padded, 0, data.Length);
        return padded;
    }
}
