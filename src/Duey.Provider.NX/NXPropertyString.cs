using Duey.Abstractions;
using Duey.Provider.NX.Headers;
using Duey.Provider.NX.Headers.Properties;

namespace Duey.Provider.NX;

public class NXPropertyString : NXNode, IDataProperty<string>
{
    private readonly NXPropertyStringHeader _dataHeader;

    internal NXPropertyString(
        NXPackage package, 
        NXNodeHeader header, 
        IDataNode? parent,
        NXPropertyStringHeader dataHeader
    ) : base(package, header, parent) 
        => _dataHeader = dataHeader;

    public string Resolve() => Package.StringOffsetTable.Get(_dataHeader);
}
