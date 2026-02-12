using System.IO.MemoryMappedFiles;
using System.Text;
using Duey.Provider.WZ.Crypto;
using Duey.Provider.WZ.Files;
using Duey.Provider.WZ.Types;
using Xunit;

namespace Duey.Provider.WZ.Tests;

/// <summary>
/// Tests that verify correct byte consumption for WZ variant types.
///
/// When a variant type consumes the wrong number of bytes, the stream
/// desynchronizes and subsequent ReadStringBlock() calls encounter
/// invalid type bytes, causing "Unknown string type when reading string block"
/// exceptions. This is the root cause of Canvas/Property parsing failures
/// in archives like Map.wz.
/// </summary>
public class VariantParsingTests
{
    /// <summary>
    /// BoolVariant (type 11) must consume exactly 1 byte, not 2.
    ///
    /// When incorrectly grouped with Int16Variant and read via ReadInt16() (2 bytes),
    /// the stream advances 1 byte too far. The next property's string block type byte
    /// becomes 0xFF (the negative ASCII length marker of the following name), which is
    /// not a valid string block type (0x00, 0x73, 0x01, 0x1B), causing
    /// WZPackageException: "Unknown string type when reading string block".
    ///
    /// Binary layout:
    ///   [header 3B] [empty name 2B] [type 0x0B 1B] [value 0x01 1B]
    ///                                               ↑ correct: 1 byte
    ///                                               ↑ bug: 2 bytes (reads into next name)
    ///   [name "x" 4B] [type 0x09 1B] [size 4B] [Canvas string block 8B] [pad]
    ///    ↑ offset 7 (correct) or offset 8 (bug: hits 0xFF → unknown type)
    /// </summary>
    [Fact]
    public void BoolVariant_ConsumesOneByte_CanvasEntryParsedCorrectly()
    {
        var cipher = new XORCipher(WZImageIV.GMS);
        var data = BuildBoolVariantTestData(cipher);

        using var mmf = CreateMemoryMappedFile(data);
        var prop = new WZPropertyFile(mmf, cipher, start: 0, offset: 0, name: "test");

        var children = prop.Children.ToList();

        Assert.Equal(2, children.Count);
    }

    /// <summary>
    /// CYVariant (type 6) must consume exactly 8 bytes (fixed Int64), not a
    /// compressed long (1 or 9 bytes).
    ///
    /// The test value 0xFF00 has little-endian bytes [0x00, 0xFF, ...].
    /// With ReadCompressedLong (bug): reads sbyte 0x00 (not -128), returns 0,
    /// consumes only 1 byte → 7-byte misalignment → next string block type is
    /// 0xFF → "Unknown string type".
    /// With ReadInt64 (fix): consumes all 8 bytes → stream stays aligned.
    /// </summary>
    [Fact]
    public void CYVariant_ConsumesEightFixedBytes_NextPropertyParsedCorrectly()
    {
        var cipher = new XORCipher(WZImageIV.GMS);
        var data = Build64BitVariantTestData(cipher, WZVariantType.CYVariant);

        using var mmf = CreateMemoryMappedFile(data);
        var prop = new WZPropertyFile(mmf, cipher, start: 0, offset: 0, name: "test");

        var children = prop.Children.ToList();

        Assert.Equal(2, children.Count);
    }

    /// <summary>
    /// Int64Variant (type 20) must consume exactly 8 bytes (fixed Int64),
    /// not a compressed long.
    /// </summary>
    [Fact]
    public void Int64Variant_ConsumesEightFixedBytes_NextPropertyParsedCorrectly()
    {
        var cipher = new XORCipher(WZImageIV.GMS);
        var data = Build64BitVariantTestData(cipher, WZVariantType.Int64Variant);

        using var mmf = CreateMemoryMappedFile(data);
        var prop = new WZPropertyFile(mmf, cipher, start: 0, offset: 0, name: "test");

        var children = prop.Children.ToList();

        Assert.Equal(2, children.Count);
    }

    /// <summary>
    /// Uint64Variant (type 21) must consume exactly 8 bytes (fixed Int64),
    /// not a compressed long.
    /// </summary>
    [Fact]
    public void Uint64Variant_ConsumesEightFixedBytes_NextPropertyParsedCorrectly()
    {
        var cipher = new XORCipher(WZImageIV.GMS);
        var data = Build64BitVariantTestData(cipher, WZVariantType.Uint64Variant);

        using var mmf = CreateMemoryMappedFile(data);
        var prop = new WZPropertyFile(mmf, cipher, start: 0, offset: 0, name: "test");

        var children = prop.Children.ToList();

        Assert.Equal(2, children.Count);
    }

    // ── Helpers ──────────────────────────────────────────────────────────

    private static MemoryMappedFile CreateMemoryMappedFile(byte[] data)
    {
        var mmf = MemoryMappedFile.CreateNew(null, data.Length);
        using var stream = mmf.CreateViewStream(0, data.Length, MemoryMappedFileAccess.Write);
        stream.Write(data, 0, data.Length);
        return mmf;
    }

    /// <summary>
    /// Builds: [BoolVariant, value=1] + [DispatchVariant/Canvas].
    /// A 2-byte BoolVariant read shifts the stream so the next string block
    /// type byte lands on 0xFF, triggering "Unknown string type".
    /// </summary>
    private static byte[] BuildBoolVariantTestData(XORCipher cipher)
    {
        using var ms = new MemoryStream();
        using var writer = new BinaryWriter(ms);

        // Property header
        writer.Write(false); // first boolean
        writer.Write(false); // second boolean (false → don't skip 2 bytes)
        writer.Write((sbyte)2); // compressed int count = 2

        // Property 0: BoolVariant with empty name
        WriteInlineStringBlock(writer, "", cipher);
        writer.Write((byte)WZVariantType.BoolVariant);
        writer.Write((byte)1); // value = true (1 byte)

        // Property 1: DispatchVariant("Canvas") with name "x"
        // Name "x" starts with 0x00 (inline type) then 0xFF (-1 as sbyte = ASCII 1 char).
        // Off-by-one bug: reader lands on the 0xFF byte and treats it as string block type.
        WriteInlineStringBlock(writer, "x", cipher);
        writer.Write((byte)WZVariantType.DispatchVariant);

        var sizePosition = ms.Position;
        writer.Write(0); // size placeholder
        var contentStart = ms.Position;

        WriteInlineStringBlock(writer, "Canvas", cipher);

        var contentSize = (int)(ms.Position - contentStart);

        // Padding so position + size lands in valid area
        var padding = new byte[64];
        writer.Write(padding);

        // Patch size field
        ms.Position = sizePosition;
        writer.Write(contentSize + padding.Length);

        return PadToMinimumSize(ms.ToArray(), 256);
    }

    /// <summary>
    /// Builds: [64-bit variant, value=0xFF00] + [Int32Variant, value=42].
    /// Value 0xFF00 in little-endian is [0x00, 0xFF, 0x00, ...].
    /// ReadCompressedLong reads sbyte 0x00 (not -128) → consumes only 1 byte → 7-byte drift.
    /// The reader then hits 0xFF as string block type → "Unknown string type".
    /// </summary>
    private static byte[] Build64BitVariantTestData(XORCipher cipher, WZVariantType variantType)
    {
        using var ms = new MemoryStream();
        using var writer = new BinaryWriter(ms);

        // Property header
        writer.Write(false);
        writer.Write(false);
        writer.Write((sbyte)2); // count = 2

        // Property 0: 64-bit variant with empty name
        WriteInlineStringBlock(writer, "", cipher);
        writer.Write((byte)variantType);
        writer.Write(0xFF00L); // 8 bytes LE: [0x00, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00]

        // Property 1: Int32Variant with name "x"
        WriteInlineStringBlock(writer, "x", cipher);
        writer.Write((byte)WZVariantType.Int32Variant);
        writer.Write((sbyte)42); // compressed int value = 42

        return PadToMinimumSize(ms.ToArray(), 256);
    }

    /// <summary>
    /// Writes an inline string block (type 0x00) with ASCII encoding.
    /// Encrypts the string content using the same XOR mask + cipher pipeline
    /// that WZReader.ReadString/ReadStringASCII uses for decryption.
    /// </summary>
    private static void WriteInlineStringBlock(BinaryWriter writer, string value, XORCipher cipher)
    {
        writer.Write((byte)0x00); // string block type: inline

        if (string.IsNullOrEmpty(value))
        {
            writer.Write((sbyte)0); // length = 0 → empty string
            return;
        }

        var plaintext = Encoding.ASCII.GetBytes(value);
        writer.Write((sbyte)(-plaintext.Length)); // negative length = ASCII

        // Encrypt: reverse of WZReader decryption pipeline
        // Decryption: raw → XOR mask (0xAA+i) → cipher XOR keystream → plaintext
        // Encryption: plaintext → cipher XOR keystream → XOR mask (0xAA+i) → raw
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

    private static byte[] PadToMinimumSize(byte[] data, int minSize)
    {
        if (data.Length >= minSize)
            return data;

        var padded = new byte[minSize];
        Buffer.BlockCopy(data, 0, padded, 0, data.Length);
        return padded;
    }
}
