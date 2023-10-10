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
        NXPropertyInt64Header dataHeader
    ) : base(package, header) 
        => _dataHeader = dataHeader;

    public long Resolve() => _dataHeader.Data;
}
