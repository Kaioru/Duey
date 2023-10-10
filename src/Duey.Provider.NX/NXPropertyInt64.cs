using Duey.Abstractions;
using Duey.Provider.NX.Headers;
using Duey.Provider.NX.Headers.Properties;

namespace Duey.Provider.NX;

public class NXPropertyInt64 : NXNode, IDataProperty<long>
{
    private readonly NXPropertyInt64Header _dataHeader;
    
    internal NXPropertyInt64(
        NXPackage package, 
        NXNodeHeader header, 
        IDataNode? parent,
        NXPropertyInt64Header dataHeader
    ) : base(package, header, parent) 
        => _dataHeader = dataHeader;

    public long Resolve() => _dataHeader.Data;
}
