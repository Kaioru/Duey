using System.Text;
using Duey.Provider.NX.Exceptions;
using Duey.Provider.NX.Headers.Properties;

namespace Duey.Provider.NX.Tables;

internal class NXStringOffsetTable : AbstractNXOffsetTable<string, NXPropertyStringHeader>
{
    public NXStringOffsetTable(
        NXPackage package,
        uint count,
        long offset
    ) : base(package, count, offset)
    {
    }

    public override string Get(NXPropertyStringHeader data)
        => Get(data.StringID);

    public string Get(uint id)
    {
        if (id > Count) throw new NXPackageException("Index out of bounds of string offset table");

        var offset = Package.Accessor.ReadInt64(Offset + id * 8);
        var stringLength = Package.Accessor.ReadUInt16(offset);
        var stringData = new byte[stringLength];

        Package.Accessor.ReadArray(offset + 2, stringData, 0, stringLength);
        return Encoding.UTF8.GetString(stringData);
    }
}