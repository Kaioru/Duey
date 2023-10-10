using Duey.Abstractions;
using Duey.Abstractions.Types;
using Duey.Provider.NX.Headers;
using Duey.Provider.NX.Headers.Properties;

namespace Duey.Provider.NX;

public class NXPropertyAudio : NXNode, IDataProperty<DataAudio>
{
    private readonly NXPropertyAudioHeader _dataHeader;

    internal NXPropertyAudio(
        NXPackage package, 
        NXNodeHeader header, 
        IDataNode? parent,
        NXPropertyAudioHeader dataHeader
    ) : base(package, header, parent) 
        => _dataHeader = dataHeader;

    public DataAudio Resolve() => Package.AudioOffsetTable.Get(_dataHeader);
}
