using System.IO;
using Duey.Exceptions;
using Duey.Layout.Nodes;
using Duey.Types;

namespace Duey.Tables
{
    internal class NXAudioOffsetTable : AbstractNXOffsetTable<NXAudio, NXAudioNode>
    {
        public NXAudioOffsetTable(
            NXFile file,
            uint count,
            long offset
        ) : base(file, count, offset)
        {
        }

        public override NXAudio Get(NXAudioNode data)
        {
            var id = data.AudioID;

            if (id > Count) throw new NXFileException("Index out of bounds of audio offset table");

            var offset = File.Accessor.ReadInt64(Offset + id * 8);
            var length = (int) data.Length;
            var target = new byte[length];

            File.Accessor.ReadArray(offset + 4, target, 0, length);
            return new NXAudio(target);
        }
    }
}