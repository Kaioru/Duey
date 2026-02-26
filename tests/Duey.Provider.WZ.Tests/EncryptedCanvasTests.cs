using System.IO.Compression;
using System.IO.MemoryMappedFiles;
using System.Text;
using Duey.Abstractions;
using Duey.Abstractions.Types;
using Duey.Provider.WZ.Crypto;
using Duey.Provider.WZ.Files;
using Duey.Provider.WZ.Files.Extended;
using Duey.Provider.WZ.Types;
using Xunit;
using static Duey.Provider.WZ.Tests.WZTestHelpers;

namespace Duey.Provider.WZ.Tests;

/// <summary>
/// Tests for WZPropertyCanvas.Resolve() handling of GMS-encrypted canvas nodes
/// (header[1] == 0x02) and the format 0x001 (BGRA4444) mapping.
///
/// Encrypted canvas binary layout after the "Canvas" block:
///   [1B padding] [1B hasChildren]
///   if hasChildren=1: [2B skip] [count CI] [count property entries]
///   [CI: width] [CI: height] [CI: format] [1B: scale] [4B skip]
///   [I32: totalLength]
///   [byte: 0x00]              header[0]
///   [byte: 0x02]              header[1] — discriminator for the encrypted path
///   [byte: 0x00]              header[2]
///   --- data (totalLength-3 bytes) ---
///   [4 bytes: 00 00 EE 32]    constant magic
///   [I32 LE: compressedSize]  byte count of the XOR-encrypted payload
///   [compressedSize bytes]    XOR-encrypted raw deflate (no zlib wrapper)
///
/// Fix: detect header[1] != 0x78, parse 8-byte preamble, XOR-decrypt with _cipher,
/// then feed to DeflateStream.
///
/// Secondary fix: format 0x001 (BGRA4444, 2 bytes/pixel) maps to DataBitmapFormat.Rgba16.
/// </summary>
public class EncryptedCanvasTests
{
    // ── Encrypted canvas decode tests (the fix) ──────────────────────────

    /// <summary>
    /// Encrypted canvas (header[1]=0x02) must decode pixel data correctly.
    /// Before the fix, DeflateStream received the raw encrypted bytes and threw
    /// InvalidDataException immediately.
    /// </summary>
    [Fact]
    public void EncryptedCanvas_Resolve_DecodesPixelDataCorrectly()
    {
        var cipher = new XORCipher(WZImageIV.GMS);
        const int w = 4, h = 4;
        var expected = GeneratePixels(w * h * 2); // BGRA4444: 2 bytes/pixel
        var data = BuildEncryptedCanvasData(cipher, w, h, 0x001, expected, hasChildren: false);

        using var mmf = CreateMemoryMappedFile(data);
        var canvas = new WZPropertyCanvas(mmf, cipher, start: 0, offset: 0, name: "canvas");

        var bitmap = ((IDataProperty<DataBitmap>)canvas).Resolve();

        Assert.Equal(w, bitmap.Width);
        Assert.Equal(h, bitmap.Height);
        Assert.Equal(DataBitmapFormat.Rgba16, bitmap.Format);
        Assert.Equal(expected, bitmap.Data.ToArray());
    }

    /// <summary>
    /// Decompressed byte count must equal exactly width × height × 2 for BGRA4444.
    /// Verifies that no extra bytes are introduced or lost during the decrypt+decompress.
    /// </summary>
    [Fact]
    public void EncryptedCanvas_Resolve_ExactPixelCountMatchesDimensions()
    {
        var cipher = new XORCipher(WZImageIV.GMS);
        const int w = 8, h = 6;
        var pixels = GeneratePixels(w * h * 2);
        var data = BuildEncryptedCanvasData(cipher, w, h, 0x001, pixels, hasChildren: false);

        using var mmf = CreateMemoryMappedFile(data);
        var canvas = new WZPropertyCanvas(mmf, cipher, start: 0, offset: 0, name: "canvas");

        var bitmap = ((IDataProperty<DataBitmap>)canvas).Resolve();

        Assert.Equal(w * h * 2, bitmap.Data.Length);
    }

    /// <summary>
    /// An encrypted canvas with hasChildren=1 must parse its sub-properties correctly
    /// and remain resolvable — bitmap data follows the property block.
    /// Combined regression for both the encrypted-path fix and the hasChildren fix.
    /// </summary>
    [Fact]
    public void EncryptedCanvas_WithChildren_ResolvesAfterSubProperties()
    {
        var cipher = new XORCipher(WZImageIV.GMS);
        const int w = 4, h = 4;
        var pixels = GeneratePixels(w * h * 2);
        var data = BuildEncryptedCanvasData(cipher, w, h, 0x001, pixels, hasChildren: true);

        using var mmf = CreateMemoryMappedFile(data);
        var canvas = new WZPropertyCanvas(mmf, cipher, start: 0, offset: 0, name: "canvas");

        var children = canvas.Children.ToList();
        Assert.Equal(2, children.Count);
        Assert.Equal("x", children[0].Name);
        Assert.Equal("y", children[1].Name);
        Assert.Equal((long)w, ((IDataProperty<long>)children[0]).Resolve());
        Assert.Equal((long)h, ((IDataProperty<long>)children[1]).Resolve());

        var bitmap = ((IDataProperty<DataBitmap>)canvas).Resolve();

        Assert.Equal(w, bitmap.Width);
        Assert.Equal(h, bitmap.Height);
        Assert.Equal(DataBitmapFormat.Rgba16, bitmap.Format);
        Assert.Equal(w * h * 2, bitmap.Data.Length);
    }

    // ── Format 0x001 mapping tests ────────────────────────────────────────

    /// <summary>
    /// Format field 0x001 must map to DataBitmapFormat.Rgba16 (BGRA4444, 2 bytes/pixel).
    /// Before the fix, 0x001 fell through to DataBitmapFormat.Unknown.
    /// Uses the standard path (header[1]=0x78) to isolate the format mapping from the
    /// encrypted-path fix.
    /// </summary>
    [Fact]
    public void Format0x001_MapsToRgba16()
    {
        var cipher = new XORCipher(WZImageIV.GMS);
        const int w = 4, h = 4;
        var pixels = GeneratePixels(w * h * 2);
        var data = BuildStandardCanvasData(cipher, w, h, 0x001, pixels);

        using var mmf = CreateMemoryMappedFile(data);
        var canvas = new WZPropertyCanvas(mmf, cipher, start: 0, offset: 0, name: "canvas");

        var bitmap = ((IDataProperty<DataBitmap>)canvas).Resolve();

        Assert.Equal(DataBitmapFormat.Rgba16, bitmap.Format);
    }

    /// <summary>
    /// Regression: format 0x201 mapping to Rgba16 must remain unchanged.
    /// </summary>
    [Fact]
    public void Format0x201_StillMapsToRgba16()
    {
        var cipher = new XORCipher(WZImageIV.GMS);
        const int w = 4, h = 4;
        var pixels = GeneratePixels(w * h * 2);
        var data = BuildStandardCanvasData(cipher, w, h, 0x201, pixels);

        using var mmf = CreateMemoryMappedFile(data);
        var canvas = new WZPropertyCanvas(mmf, cipher, start: 0, offset: 0, name: "canvas");

        var bitmap = ((IDataProperty<DataBitmap>)canvas).Resolve();

        Assert.Equal(DataBitmapFormat.Rgba16, bitmap.Format);
    }

    // ── Standard path regression test ────────────────────────────────────

    /// <summary>
    /// Standard canvas (header[1]=0x78) must still decode correctly after the
    /// encrypted-path branch was added. Output must be byte-identical to before.
    /// </summary>
    [Fact]
    public void StandardCanvas_Resolve_StillDecodesCorrectly()
    {
        var cipher = new XORCipher(WZImageIV.GMS);
        const int w = 4, h = 4;
        var expected = GeneratePixels(w * h * 4); // Rgba32: 4 bytes/pixel
        var data = BuildStandardCanvasData(cipher, w, h, 0x002, expected);

        using var mmf = CreateMemoryMappedFile(data);
        var canvas = new WZPropertyCanvas(mmf, cipher, start: 0, offset: 0, name: "canvas");

        var bitmap = ((IDataProperty<DataBitmap>)canvas).Resolve();

        Assert.Equal(w, bitmap.Width);
        Assert.Equal(h, bitmap.Height);
        Assert.Equal(DataBitmapFormat.Rgba32, bitmap.Format);
        Assert.Equal(expected, bitmap.Data.ToArray());
    }

    // ── Binary data builders ──────────────────────────────────────────────

    /// <summary>
    /// Generates a deterministic non-trivial pixel byte sequence to avoid
    /// compressor short-circuits on all-zero or constant data.
    /// </summary>
    private static byte[] GeneratePixels(int count)
    {
        var pixels = new byte[count];
        for (var i = 0; i < count; i++)
            pixels[i] = (byte)((i * 7 + 13) % 256);
        return pixels;
    }

    /// <summary>
    /// Builds a full canvas binary blob using the encrypted path (header[1]=0x02).
    /// Layout:
    ///   [padding] [hasChildren]
    ///   if hasChildren: [2B skip] [count=2] [prop "x"=width] [prop "y"=height]
    ///   [CI width] [CI height] [CI format] [scale] [4B skip]
    ///   [I32 totalLength] [00 02 00] [magic] [LE32 size] [XOR-encrypted deflate]
    /// </summary>
    private static byte[] BuildEncryptedCanvasData(
        XORCipher cipher, int width, int height, int format, byte[] pixels, bool hasChildren)
    {
        using var ms = new MemoryStream();
        using var writer = new BinaryWriter(ms);

        writer.Write((byte)0x00);                            // padding
        writer.Write(hasChildren ? (byte)0x01 : (byte)0x00); // hasChildren

        if (hasChildren)
        {
            writer.Write((short)0); // 2-byte skip
            writer.Write((sbyte)2); // property count = 2

            WriteInlineStringBlock(writer, "x", cipher);
            writer.Write((byte)WZVariantType.Int32Variant);
            WriteCompressedInt(writer, width);

            WriteInlineStringBlock(writer, "y", cipher);
            writer.Write((byte)WZVariantType.Int32Variant);
            WriteCompressedInt(writer, height);
        }

        WriteEncryptedBitmapSection(writer, cipher, width, height, format, pixels);

        return PadToMinimumSize(ms.ToArray(), 512);
    }

    /// <summary>
    /// Builds a full canvas binary blob using the standard path (header[1]=0x78).
    /// Layout:
    ///   [padding] [hasChildren=0]
    ///   [CI width] [CI height] [CI format] [scale] [4B skip]
    ///   [I32 totalLength] [00 78 9C] [raw deflate]
    /// </summary>
    private static byte[] BuildStandardCanvasData(
        XORCipher cipher, int width, int height, int format, byte[] pixels)
    {
        using var ms = new MemoryStream();
        using var writer = new BinaryWriter(ms);

        writer.Write((byte)0x00); // padding
        writer.Write((byte)0x00); // hasChildren = false

        WriteStandardBitmapSection(writer, width, height, format, pixels);

        return PadToMinimumSize(ms.ToArray(), 512);
    }

    /// <summary>
    /// Writes the bitmap fields for the encrypted path:
    ///   [CI width] [CI height] [CI format] [byte scale=0] [4B skip]
    ///   [I32 totalLength] [byte 0x00] [byte 0x02] [byte 0x00]
    ///   [4B magic: 00 00 EE 32] [I32 LE compressedSize] [encrypted deflate]
    ///
    /// XOR-encryption uses the same cipher instance that Resolve() will use to
    /// decrypt — Transform() is always applied from key offset 0.
    /// </summary>
    private static void WriteEncryptedBitmapSection(
        BinaryWriter writer, XORCipher cipher, int width, int height, int format, byte[] pixels)
    {
        byte[] deflated;
        using (var deflateMs = new MemoryStream())
        {
            using (var deflater = new DeflateStream(deflateMs, CompressionMode.Compress, leaveOpen: true))
                deflater.Write(pixels, 0, pixels.Length);
            deflated = deflateMs.ToArray();
        }

        // XOR-encrypt: cipher.Transform always starts from key offset 0,
        // so this is the exact inverse of the Transform call in Resolve().
        var encrypted = new byte[deflated.Length];
        Buffer.BlockCopy(deflated, 0, encrypted, 0, deflated.Length);
        cipher.Transform(encrypted);

        // Preamble: magic [00 00 EE 32] + LE32 compressedSize
        var data = new byte[8 + encrypted.Length];
        data[0] = 0x00; data[1] = 0x00; data[2] = 0xEE; data[3] = 0x32;
        var sizeBytes = BitConverter.GetBytes(encrypted.Length);
        Buffer.BlockCopy(sizeBytes, 0, data, 4, 4);
        Buffer.BlockCopy(encrypted, 0, data, 8, encrypted.Length);

        var totalLength = 3 + data.Length; // 3-byte header + data

        WriteCompressedInt(writer, width);
        WriteCompressedInt(writer, height);
        WriteCompressedInt(writer, format);
        writer.Write((byte)0);    // scale = 0
        writer.Write(0);          // 4-byte skip
        writer.Write(totalLength);
        writer.Write((byte)0x00); // header[0]
        writer.Write((byte)0x02); // header[1] = 0x02 → encrypted path
        writer.Write((byte)0x00); // header[2]
        writer.Write(data);
    }

    /// <summary>
    /// Writes the bitmap fields for the standard path:
    ///   [CI width] [CI height] [CI format] [byte scale=0] [4B skip]
    ///   [I32 totalLength] [byte 0x00] [byte 0x78] [byte 0x9C] [raw deflate]
    /// </summary>
    private static void WriteStandardBitmapSection(
        BinaryWriter writer, int width, int height, int format, byte[] pixels)
    {
        byte[] deflated;
        using (var deflateMs = new MemoryStream())
        {
            using (var deflater = new DeflateStream(deflateMs, CompressionMode.Compress, leaveOpen: true))
                deflater.Write(pixels, 0, pixels.Length);
            deflated = deflateMs.ToArray();
        }

        var totalLength = 3 + deflated.Length;

        WriteCompressedInt(writer, width);
        WriteCompressedInt(writer, height);
        WriteCompressedInt(writer, format);
        writer.Write((byte)0);    // scale = 0
        writer.Write(0);          // 4-byte skip
        writer.Write(totalLength);
        writer.Write((byte)0x00); // header[0]
        writer.Write((byte)0x78); // header[1] = 0x78 → standard path
        writer.Write((byte)0x9C); // header[2]
        writer.Write(deflated);
    }
    }
