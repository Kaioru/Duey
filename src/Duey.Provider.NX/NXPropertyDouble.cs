using Duey.Abstractions;
using Duey.Provider.NX.Headers;
using Duey.Provider.NX.Headers.Properties;

namespace Duey.Provider.NX;

public class NXPropertyDouble : NXNode, IDataProperty<double>
{
    private readonly NXPropertyDoubleHeader _dataHeader;
    
    internal NXPropertyDouble(
        NXPackage package, 
        NXNodeHeader header, 
        NXPropertyDoubleHeader dataHeader
    ) : base(package, header) 
        => _dataHeader = dataHeader;

    public double Resolve() => _dataHeader.Data;
}
