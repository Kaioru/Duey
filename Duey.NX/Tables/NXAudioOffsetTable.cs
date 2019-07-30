using System.IO;
using Duey.NX.Exceptions;
using Duey.NX.Layout.Nodes;

namespace Duey.NX.Tables
{
    internal class NXAudioOffsetTable : AbstractNXOffsetTable<Stream, NXAudioNode>
    {
        public NXAudioOffsetTable(
            NXFile file, 
            uint count, 
            long offset
        ) : base(file, count, offset)
        {
        }

        public override Stream Get(NXAudioNode data)
        {
            var id = data.AudioID;
            
            if (id > Count) throw new NXFileException("Index out of bounds of audio offset table");

            var offset = File.Accessor.ReadInt64(Offset + id * 8);
            var length = data.Length;
            
            using (var source = File.View.CreateViewStream(offset + 4, length))
                return source;
        }
    }
}