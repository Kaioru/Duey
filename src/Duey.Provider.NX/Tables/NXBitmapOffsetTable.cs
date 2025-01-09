using Duey.Abstractions.Types;
using Duey.Provider.NX.Exceptions;
using Duey.Provider.NX.Headers.Properties;
using K4os.Compression.LZ4;

namespace Duey.Provider.NX.Tables;

internal class NXBitmapOffsetTable : AbstractNXOffsetTable<DataBitmap, NXPropertyBitmapHeader>
{
    public NXBitmapOffsetTable(
        NXPackage package,
        uint count,
        long offset
    ) : base(package, count, offset)
    {
    }

    public override DataBitmap Get(NXPropertyBitmapHeader data)
    {
        var id = data.BitmapID;

        if (id > Count) throw new NXPackageException("Index out of bounds of bitmap offset table");

        var offset = Package.Accessor.ReadInt64(Offset + id * 8);
        var sourceLength = Package.Accessor.ReadInt32(offset);
        var source = new byte[sourceLength];
        var target = new byte[data.Height * data.Width * 4];

        Package.Accessor.ReadArray(offset + 4, source, 0, sourceLength);
        LZ4Codec.Decode(source, target);

        return new DataBitmap(data.Width, data.Height, DataBitmapFormat.Bgra32, target);
    }
}