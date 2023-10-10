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
        NXPropertyAudioHeader dataHeader
    ) : base(package, header) 
        => _dataHeader = dataHeader;

    public DataAudio Resolve() => Package.AudioOffsetTable.Get(_dataHeader);
}
