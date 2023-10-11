using Duey.Abstractions;
using Duey.Abstractions.Types;
using Duey.Provider.NX.Headers;
using Duey.Provider.NX.Headers.Properties;

namespace Duey.Provider.NX;

public class NXPropertyVector : NXNode, IDataProperty<DataVector>
{
    private readonly NXPropertyVectorHeader _dataHeader;
    
    internal NXPropertyVector(
        NXPackage package, 
        NXNodeHeader header, 
        IDataNode? parent,
        NXPropertyVectorHeader dataHeader
    ) : base(package, header, parent) 
        => _dataHeader = dataHeader;

    public DataVector Resolve() => new(_dataHeader.X, _dataHeader.Y);
}
