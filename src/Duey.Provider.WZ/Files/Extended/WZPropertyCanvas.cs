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
    
    protected override DataBitmap Resolve(WZReader reader)
    {
        var width = reader.ReadCompressedInt();
        var height = reader.ReadCompressedInt();
        var format = reader.ReadCompressedInt() switch
        {
            0x002 => DataBitmapFormat.Rgba32,
            0x201 => DataBitmapFormat.Rgba16,
            _ => DataBitmapFormat.Unknown
        };
        var scale = reader.ReadByte();

        reader.BaseStream.Position += 4;

        var length = reader.ReadInt32();
        var header = reader.ReadBytes(3);
        var data = reader.ReadBytes(length - 3);

        using var stream0 = new MemoryStream(data, false);
        using var stream1 = new DeflateStream(stream0, CompressionMode.Decompress);
        using var stream2 = new MemoryStream();
        
        width >>= scale;
        height >>= scale;
        stream1.CopyTo(stream2);
        
        return new DataBitmap((ushort)width, (ushort)height, format, stream2.ToArray());
    }
}
