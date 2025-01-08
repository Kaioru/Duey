using System.IO.MemoryMappedFiles;
using Duey.Abstractions;
using Duey.Abstractions.Types;
using Duey.Provider.WZ.Codecs;
using Duey.Provider.WZ.Crypto;

namespace Duey.Provider.WZ.Files.Deferred;

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
        var format = reader.ReadCompressedInt();
        var scale = reader.ReadByte();

        reader.BaseStream.Position += 4;

        var length = reader.ReadInt32();

        return new DataBitmap((ushort)width, (ushort)height, new byte[] {});
    }
}
