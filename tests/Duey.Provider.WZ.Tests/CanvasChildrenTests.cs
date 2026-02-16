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

namespace Duey.Provider.WZ.Tests;

/// <summary>
/// Tests for WZPropertyCanvas.Children override that correctly handles the
/// Canvas hasChildren flag.
///
/// WZ Canvas binary layout after the "Canvas" string block:
///   [1B padding] [1B hasChildren]
///   if hasChildren=1: [2B skip] [compressed int: count] [count property entries]
///   [bitmap: width(CI) height(CI) format(CI) scale(1B) 4B-skip length(I32) header(3B) deflate(length-3B)]
///
/// Bug: WZPropertyFile.Children (inherited by Canvas) always reads a compressed int
/// count after the 2-byte header. For childless canvases (hasChildren=0), this
/// misreads bitmap width as a property count, then crashes with
/// "Unknown string type when reading string block" when parsing bitmap bytes as properties.
///
/// Fix: WZPropertyCanvas overrides Children to check hasChildren before reading
/// the property block. When hasChildren=0, it yields nothing and sets _startDeferred
/// to the bitmap data position.
/// </summary>
public class CanvasChildrenTests
{
    // ── Childless Canvas Tests ───────────────────────────────────────────

    /// <summary>
    /// A childless canvas (hasChildren=0) must return empty Children without throwing.
    /// Before the fix, this would throw WZPackageException because the bitmap width
    /// was misread as a property count.
    /// </summary>
    [Fact]
    public void ChildlessCanvas_Children_ReturnsEmpty()
    {
        var cipher = new XORCipher(WZImageIV.GMS);
        var data = BuildChildlessCanvasData(cipher, width: 4, height: 4);

        using var mmf = CreateMemoryMappedFile(data);
        var canvas = new WZPropertyCanvas(mmf, cipher, start: 0, offset: 0, name: "canvas");

        var children = canvas.Children.ToList();

        Assert.Empty(children);
    }

    /// <summary>
    /// A childless canvas must be resolvable — Resolve() must return a valid DataBitmap.
    /// Before the fix, Resolve() called Children.ToList() which threw, making the
    /// canvas bitmap irrecoverable.
    /// </summary>
    [Fact]
    public void ChildlessCanvas_Resolve_ReturnsValidBitmap()
    {
        var cipher = new XORCipher(WZImageIV.GMS);
        const int w = 4, h = 4;
        var data = BuildChildlessCanvasData(cipher, width: w, height: h);

        using var mmf = CreateMemoryMappedFile(data);
        var canvas = new WZPropertyCanvas(mmf, cipher, start: 0, offset: 0, name: "canvas");

        // Resolve via the IDataProperty<DataBitmap> interface (same path as ResolveBitmap extension)
        var bitmap = ((IDataProperty<DataBitmap>)canvas).Resolve();

        Assert.Equal(w, bitmap.Width);
        Assert.Equal(h, bitmap.Height);
        Assert.Equal(DataBitmapFormat.Rgba32, bitmap.Format);
        Assert.Equal(w * h * 4, bitmap.Data.Length);
    }

    /// <summary>
    /// Calling Children multiple times on a childless canvas must be idempotent.
    /// </summary>
    [Fact]
    public void ChildlessCanvas_Children_IdempotentMultipleCalls()
    {
        var cipher = new XORCipher(WZImageIV.GMS);
        var data = BuildChildlessCanvasData(cipher, width: 2, height: 2);

        using var mmf = CreateMemoryMappedFile(data);
        var canvas = new WZPropertyCanvas(mmf, cipher, start: 0, offset: 0, name: "canvas");

        // Call multiple times — each should return empty without error
        Assert.Empty(canvas.Children.ToList());
        Assert.Empty(canvas.Children.ToList());
        Assert.Empty(canvas.Children.ToList());
    }

    // ── Canvas with Children Tests ───────────────────────────────────────

    /// <summary>
    /// A canvas with hasChildren=1 must correctly parse its sub-properties.
    /// This is a regression test: the refactoring must not break canvases that
    /// have sub-properties (like origin vectors, _inlink strings, etc.).
    /// </summary>
    [Fact]
    public void CanvasWithChildren_Children_ReturnsSubProperties()
    {
        var cipher = new XORCipher(WZImageIV.GMS);
        var data = BuildCanvasWithChildrenData(cipher, width: 4, height: 4);

        using var mmf = CreateMemoryMappedFile(data);
        var canvas = new WZPropertyCanvas(mmf, cipher, start: 0, offset: 0, name: "canvas");

        var children = canvas.Children.ToList();

        Assert.Equal(2, children.Count);
        Assert.Equal("w", children[0].Name);
        Assert.Equal("h", children[1].Name);
    }

    /// <summary>
    /// A canvas with sub-properties must still be resolvable — the bitmap data
    /// follows after all sub-properties.
    /// </summary>
    [Fact]
    public void CanvasWithChildren_Resolve_ReturnsValidBitmap()
    {
        var cipher = new XORCipher(WZImageIV.GMS);
        const int w = 4, h = 4;
        var data = BuildCanvasWithChildrenData(cipher, width: w, height: h);

        using var mmf = CreateMemoryMappedFile(data);
        var canvas = new WZPropertyCanvas(mmf, cipher, start: 0, offset: 0, name: "canvas");

        var bitmap = ((IDataProperty<DataBitmap>)canvas).Resolve();

        Assert.Equal(w, bitmap.Width);
        Assert.Equal(h, bitmap.Height);
        Assert.Equal(DataBitmapFormat.Rgba32, bitmap.Format);
        Assert.Equal(w * h * 4, bitmap.Data.Length);
    }

    /// <summary>
    /// Sub-property values must be readable and correct.
    /// </summary>
    [Fact]
    public void CanvasWithChildren_SubPropertyValues_AreCorrect()
    {
        var cipher = new XORCipher(WZImageIV.GMS);
        var data = BuildCanvasWithChildrenData(cipher, width: 4, height: 4);

        using var mmf = CreateMemoryMappedFile(data);
        var canvas = new WZPropertyCanvas(mmf, cipher, start: 0, offset: 0, name: "canvas");

        var children = canvas.Children.ToList();
        var wVal = ((IDataProperty<long>)children[0]).Resolve();
        var hVal = ((IDataProperty<long>)children[1]).Resolve();

        Assert.Equal(4, wVal);
        Assert.Equal(4, hVal);
    }

    // ── Regression: WZPropertyFile parsing unchanged after refactoring ───

    /// <summary>
    /// Regular WZPropertyFile parsing must still work after extracting
    /// ReadPropertyEntries into a separate method. Tests all commonly-used
    /// variant types in a single property block.
    /// </summary>
    [Fact]
    public void PropertyFile_MultipleVariantTypes_AllParsedCorrectly()
    {
        var cipher = new XORCipher(WZImageIV.GMS);
        var data = BuildMultiVariantPropertyData(cipher);

        using var mmf = CreateMemoryMappedFile(data);
        var prop = new WZPropertyFile(mmf, cipher, start: 0, offset: 0, name: "test");

        var children = prop.Children.ToList();

        // 8 properties: empty (no yield), uint8, int16, bool, int32, float64, bstr, date
        // EmptyVariant does not yield, so 7 children
        Assert.Equal(7, children.Count);

        // Uint8Variant
        Assert.Equal("a", children[0].Name);
        Assert.Equal(0xABL, ((IDataProperty<long>)children[0]).Resolve());

        // Int16Variant
        Assert.Equal("b", children[1].Name);
        Assert.Equal(-100L, ((IDataProperty<long>)children[1]).Resolve());

        // BoolVariant (1 byte)
        Assert.Equal("c", children[2].Name);
        Assert.Equal(1L, ((IDataProperty<long>)children[2]).Resolve());

        // Int32Variant (compressed)
        Assert.Equal("d", children[3].Name);
        Assert.Equal(42L, ((IDataProperty<long>)children[3]).Resolve());

        // Float64Variant
        Assert.Equal("e", children[4].Name);
        Assert.InRange(((IDataProperty<double>)children[4]).Resolve(), 3.13, 3.15);

        // BStrVariant
        Assert.Equal("f", children[5].Name);
        Assert.Equal("hello", ((IDataProperty<string>)children[5]).Resolve());

        // DateVariant (8 bytes fixed)
        Assert.Equal("g", children[6].Name);
        Assert.Equal(1234567890L, ((IDataProperty<long>)children[6]).Resolve());
    }

    /// <summary>
    /// A property block with zero entries must return empty Children.
    /// Regression test for ReadPropertyEntries with count=0.
    /// </summary>
    [Fact]
    public void PropertyFile_ZeroEntries_ReturnsEmpty()
    {
        var cipher = new XORCipher(WZImageIV.GMS);

        using var ms = new MemoryStream();
        using var writer = new BinaryWriter(ms);

        // Property header: 2 booleans + count = 0
        writer.Write(false);
        writer.Write(false);
        writer.Write((sbyte)0);

        var data = PadToMinimumSize(ms.ToArray(), 64);
        using var mmf = CreateMemoryMappedFile(data);
        var prop = new WZPropertyFile(mmf, cipher, start: 0, offset: 0, name: "test");

        Assert.Empty(prop.Children.ToList());
    }

    /// <summary>
    /// Dispatch/Canvas entries created through a parent WZPropertyFile must
    /// still produce valid WZPropertyCanvas nodes. Regression test for the
    /// ReadPropertyEntries extraction not breaking Dispatch type handling.
    /// </summary>
    [Fact]
    public void PropertyFile_DispatchCanvas_YieldsCanvasNode()
    {
        var cipher = new XORCipher(WZImageIV.GMS);
        var data = BuildPropertyWithCanvasChild(cipher, width: 2, height: 2);

        using var mmf = CreateMemoryMappedFile(data);
        var prop = new WZPropertyFile(mmf, cipher, start: 0, offset: 0, name: "root");

        var children = prop.Children.ToList();

        Assert.Single(children);
        Assert.Equal("img", children[0].Name);
        Assert.IsType<WZPropertyCanvas>(children[0]);

        // The canvas child should be resolvable
        var canvas = (IDataProperty<DataBitmap>)children[0];
        var bitmap = canvas.Resolve();
        Assert.Equal(2, bitmap.Width);
        Assert.Equal(2, bitmap.Height);
    }

    // ── Binary Data Builders ─────────────────────────────────────────────

    /// <summary>
    /// Builds Canvas binary data with hasChildren=0.
    /// Layout: [padding] [hasChildren=0] [bitmap data]
    /// </summary>
    private static byte[] BuildChildlessCanvasData(XORCipher cipher, int width, int height)
    {
        using var ms = new MemoryStream();
        using var writer = new BinaryWriter(ms);

        // Canvas header
        writer.Write((byte)0x00); // padding
        writer.Write((byte)0x00); // hasChildren = false

        // Bitmap data
        WriteBitmapData(writer, width, height);

        return PadToMinimumSize(ms.ToArray(), 256);
    }

    /// <summary>
    /// Builds Canvas binary data with hasChildren=1 and two Int32 sub-properties.
    /// Layout: [padding] [hasChildren=1] [skip 2B] [count=2]
    ///         [prop "w" Int32=width] [prop "h" Int32=height] [bitmap data]
    /// </summary>
    private static byte[] BuildCanvasWithChildrenData(XORCipher cipher, int width, int height)
    {
        using var ms = new MemoryStream();
        using var writer = new BinaryWriter(ms);

        // Canvas header
        writer.Write((byte)0x00); // padding
        writer.Write((byte)0x01); // hasChildren = true
        writer.Write((short)0);   // 2 bytes skipped when hasChildren=true

        // Property block: count + entries
        writer.Write((sbyte)2); // compressed int count = 2

        // Property 0: "w" = width (Int32Variant, compressed)
        WriteInlineStringBlock(writer, "w", cipher);
        writer.Write((byte)WZVariantType.Int32Variant);
        WriteCompressedInt(writer, width);

        // Property 1: "h" = height (Int32Variant, compressed)
        WriteInlineStringBlock(writer, "h", cipher);
        writer.Write((byte)WZVariantType.Int32Variant);
        WriteCompressedInt(writer, height);

        // Bitmap data follows after all sub-properties
        WriteBitmapData(writer, width, height);

        return PadToMinimumSize(ms.ToArray(), 512);
    }

    /// <summary>
    /// Builds a multi-variant WZPropertyFile block to test all common types.
    /// </summary>
    private static byte[] BuildMultiVariantPropertyData(XORCipher cipher)
    {
        using var ms = new MemoryStream();
        using var writer = new BinaryWriter(ms);

        // Property header
        writer.Write(false);    // bool 1
        writer.Write(false);    // bool 2 (no skip)
        writer.Write((sbyte)8); // count = 8

        // 0: EmptyVariant (yields nothing, but consumes 0 value bytes)
        WriteInlineStringBlock(writer, "z", cipher);
        writer.Write((byte)WZVariantType.EmptyVariant);

        // 1: Uint8Variant = 0xAB
        WriteInlineStringBlock(writer, "a", cipher);
        writer.Write((byte)WZVariantType.Uint8Variant);
        writer.Write((byte)0xAB);

        // 2: Int16Variant = -100
        WriteInlineStringBlock(writer, "b", cipher);
        writer.Write((byte)WZVariantType.Int16Variant);
        writer.Write((short)-100);

        // 3: BoolVariant = 1 (must be 1 byte, not 2)
        WriteInlineStringBlock(writer, "c", cipher);
        writer.Write((byte)WZVariantType.BoolVariant);
        writer.Write((byte)1);

        // 4: Int32Variant = 42 (compressed)
        WriteInlineStringBlock(writer, "d", cipher);
        writer.Write((byte)WZVariantType.Int32Variant);
        WriteCompressedInt(writer, 42);

        // 5: Float64Variant = 3.14
        WriteInlineStringBlock(writer, "e", cipher);
        writer.Write((byte)WZVariantType.Float64Variant);
        writer.Write(3.14d);

        // 6: BStrVariant = "hello"
        WriteInlineStringBlock(writer, "f", cipher);
        writer.Write((byte)WZVariantType.BStrVariant);
        WriteInlineStringBlock(writer, "hello", cipher);

        // 7: DateVariant = 1234567890 (8 bytes fixed)
        WriteInlineStringBlock(writer, "g", cipher);
        writer.Write((byte)WZVariantType.DateVariant);
        writer.Write(1234567890L);

        return PadToMinimumSize(ms.ToArray(), 512);
    }

    /// <summary>
    /// Builds a WZPropertyFile block containing one DispatchVariant("Canvas") child.
    /// The child canvas is childless and should be resolvable.
    /// </summary>
    private static byte[] BuildPropertyWithCanvasChild(XORCipher cipher, int width, int height)
    {
        using var ms = new MemoryStream();
        using var writer = new BinaryWriter(ms);

        // Property header
        writer.Write(false);    // bool 1
        writer.Write(false);    // bool 2
        writer.Write((sbyte)1); // count = 1

        // Property 0: DispatchVariant("Canvas") named "img"
        WriteInlineStringBlock(writer, "img", cipher);
        writer.Write((byte)WZVariantType.DispatchVariant);

        var sizePos = ms.Position;
        writer.Write(0); // size placeholder
        var contentStart = ms.Position;

        // "Canvas" type string
        WriteInlineStringBlock(writer, "Canvas", cipher);

        // Canvas data starts here (this is where WZPropertyCanvas._start will point)
        // Childless canvas: [padding] [hasChildren=0] [bitmap]
        writer.Write((byte)0x00); // padding
        writer.Write((byte)0x00); // hasChildren = false
        WriteBitmapData(writer, width, height);

        var contentSize = (int)(ms.Position - contentStart);

        // Patch size field
        ms.Position = sizePos;
        writer.Write(contentSize);

        return PadToMinimumSize(ms.ToArray(), 512);
    }

    // ── Low-Level Helpers ────────────────────────────────────────────────

    /// <summary>
    /// Writes bitmap data: width, height, format, scale, 4-byte skip, length, 3-byte header, deflate data.
    /// Produces a valid RGBA32 bitmap that WZPropertyCanvas.Resolve() can decompress.
    /// </summary>
    private static void WriteBitmapData(BinaryWriter writer, int width, int height)
    {
        // Pixel data: RGBA32, solid color
        var pixels = new byte[width * height * 4];
        for (int i = 0; i < pixels.Length; i += 4)
        {
            pixels[i + 0] = 0xFF; // R
            pixels[i + 1] = 0x00; // G
            pixels[i + 2] = 0x00; // B
            pixels[i + 3] = 0xFF; // A
        }

        // Deflate compress
        byte[] deflated;
        using (var deflateMs = new MemoryStream())
        {
            using (var deflater = new DeflateStream(deflateMs, CompressionMode.Compress, leaveOpen: true))
                deflater.Write(pixels, 0, pixels.Length);
            deflated = deflateMs.ToArray();
        }

        int totalLength = 3 + deflated.Length; // 3-byte header + deflate data

        WriteCompressedInt(writer, width);
        WriteCompressedInt(writer, height);
        WriteCompressedInt(writer, 0x002);     // format = Rgba32
        writer.Write((byte)0);                 // scale = 0
        writer.Write(0);                       // 4-byte skip
        writer.Write(totalLength);             // length (includes 3-byte header)
        writer.Write(new byte[3]);             // 3-byte header (ignored by Resolve)
        writer.Write(deflated);                // deflate-compressed pixel data
    }

    private static void WriteCompressedInt(BinaryWriter writer, int value)
    {
        if (value >= -127 && value <= 127)
        {
            writer.Write((sbyte)value);
        }
        else
        {
            writer.Write((sbyte)(-128));
            writer.Write(value);
        }
    }

    private static void WriteInlineStringBlock(BinaryWriter writer, string value, XORCipher cipher)
    {
        writer.Write((byte)0x00); // string block type: inline

        if (string.IsNullOrEmpty(value))
        {
            writer.Write((sbyte)0);
            return;
        }

        var plaintext = Encoding.ASCII.GetBytes(value);
        writer.Write((sbyte)(-plaintext.Length));

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

    private static MemoryMappedFile CreateMemoryMappedFile(byte[] data)
    {
        var mmf = MemoryMappedFile.CreateNew(null, data.Length);
        using var stream = mmf.CreateViewStream(0, data.Length, MemoryMappedFileAccess.Write);
        stream.Write(data, 0, data.Length);
        return mmf;
    }

    private static byte[] PadToMinimumSize(byte[] data, int minSize)
    {
        if (data.Length >= minSize) return data;
        var padded = new byte[minSize];
        Buffer.BlockCopy(data, 0, padded, 0, data.Length);
        return padded;
    }
}
