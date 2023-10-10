using Duey.Abstractions;
using Duey.Abstractions.Types;
using Duey.Provider.NX.Headers;
using Duey.Provider.NX.Headers.Properties;

namespace Duey.Provider.NX;

public class NXPropertyBitmap : NXNode, IDataProperty<DataBitmap>
{
    private readonly NXPropertyBitmapHeader _dataHeader;

    internal NXPropertyBitmap(
        NXPackage package, 
        NXNodeHeader header, 
        IDataNode? parent,
        NXPropertyBitmapHeader dataHeader
    ) : base(package, header, parent) 
        => _dataHeader = dataHeader;

    public DataBitmap Resolve() => Package.BitmapOffsetTable.Get(_dataHeader);
}
