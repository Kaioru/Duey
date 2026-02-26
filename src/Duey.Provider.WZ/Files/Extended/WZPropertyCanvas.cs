using System.IO.Compression;
using System.IO.MemoryMappedFiles;
using Duey.Abstractions;
using Duey.Abstractions.Types;
using Duey.Provider.WZ.Codecs;
using Duey.Provider.WZ.Crypto;

namespace Duey.Provider.WZ.Files.Extended;

public class WZPropertyCanvas : WZPropertyDeferred<DataBitmap>
{
    public WZPropertyCanvas(
        MemoryMappedFile view,
        XORCipher cipher,
        int start,
        int offset,
        string name,
        IDataNode? parent = null
    ) : base(view, cipher, start, offset, name, parent)
    {
    }

    /// <summary>
    /// Canvas binary layout after the "Canvas" string block:
    ///   [1B padding] [1B hasChildren]
    ///   if hasChildren: [skip 2B] [property block: count + entries]
    ///   [bitmap data: width, height, format, scale, skip 4, length, header, deflate]
    ///
    /// When hasChildren=0, no property block exists — bitmap data follows immediately.
    /// The base WZPropertyFile.Children unconditionally reads a compressed int count
    /// after the 2-byte header, which would misread bitmap width as a property count.
    /// </summary>
    public override IEnumerable<IDataNode> Children
    {
        get
        {
            using var stream = _view.CreateViewStream(_offset, 0, MemoryMappedFileAccess.Read);
            using var reader = new WZReader(stream, _cipher, _start);

            reader.ReadBoolean(); // padding
            if (reader.ReadBoolean()) // hasChildren
            {
                reader.BaseStream.Position += 2;
                foreach (var child in ReadPropertyEntries(reader))
                    yield return child;
            }

            _startDeferred = (int)reader.BaseStream.Position;
        }
    }

    protected override DataBitmap Resolve(WZReader reader)
    {
        var width = reader.ReadCompressedInt();
        var height = reader.ReadCompressedInt();
        var format = reader.ReadCompressedInt() switch
        {
            0x001 => DataBitmapFormat.Rgba16,
            0x002 => DataBitmapFormat.Rgba32,
            0x201 => DataBitmapFormat.Rgba16,
            _ => DataBitmapFormat.Unknown
        };
        var scale = reader.ReadByte();

        reader.BaseStream.Position += 4;

        var length = reader.ReadInt32();
        var header = reader.ReadBytes(3);
        var data = reader.ReadBytes(length - 3);

        width >>= scale;
        height >>= scale;

        if (header[1] != 0x78)
        {
            // Encrypted path: 8-byte preamble + XOR-encrypted raw deflate
            //   data[0..3]  = constant magic [00 00 EE 32]
            //   data[4..7]  = LE32 compressedSize
            //   data[8..]   = XOR-encrypted raw deflate (no zlib wrapper)
            if (data.Length < 8)
                throw new InvalidDataException("Encrypted canvas preamble is too short.");
            var compressedSize = BitConverter.ToInt32(data, 4);
            if (compressedSize <= 0 || compressedSize > data.Length - 8)
                throw new InvalidDataException(
                    $"Encrypted canvas compressed size {compressedSize} is out of range.");

            var payload = new byte[compressedSize];
            Array.Copy(data, 8, payload, 0, compressedSize);
            _cipher.Transform(payload);

            using var encStream = new MemoryStream(payload, false);
            using var deflate   = new DeflateStream(encStream, CompressionMode.Decompress);
            using var output    = new MemoryStream();
            deflate.CopyTo(output);
            return new DataBitmap((ushort)width, (ushort)height, format, output.ToArray());
        }

        using var stream0 = new MemoryStream(data, false);
        using var stream1 = new DeflateStream(stream0, CompressionMode.Decompress);
        using var stream2 = new MemoryStream();
        stream1.CopyTo(stream2);
        return new DataBitmap((ushort)width, (ushort)height, format, stream2.ToArray());
    }
}
