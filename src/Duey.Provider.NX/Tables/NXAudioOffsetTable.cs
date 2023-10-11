using Duey.Abstractions.Types;
using Duey.Provider.NX.Exceptions;
using Duey.Provider.NX.Headers.Properties;

namespace Duey.Provider.NX.Tables;

internal class NXAudioOffsetTable : AbstractNXOffsetTable<DataAudio, NXPropertyAudioHeader>
{
    public NXAudioOffsetTable(
        NXPackage package,
        uint count,
        long offset
    ) : base(package, count, offset)
    {
    }

    public override DataAudio Get(NXPropertyAudioHeader data)
    {
        var id = data.AudioID;

        if (id > Count) throw new NXPackageException("Index out of bounds of audio offset table");

        var offset = Package.Accessor.ReadInt64(Offset + id * 8);
        var length = (int) data.Length;
        var target = new byte[length];

        Package.Accessor.ReadArray(offset + 4, target, 0, length);
        return new DataAudio(target);
    }
}