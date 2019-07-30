using System.IO;
using Duey.NX.Exceptions;
using Duey.NX.Layout.Nodes;
using K4os.Compression.LZ4.Streams;

namespace Duey.NX.Tables
{
    internal class NXBitmapOffsetTable : AbstractNXOffsetTable<Stream, NXBitmapNode>
    {
        public NXBitmapOffsetTable(
            NXFile file, 
            uint count, 
            long offset
        ) : base(file, count, offset)
        {
        }

        public override Stream Get(NXBitmapNode data)
        {
            var id = data.BitmapID;
            
            if (id > Count) throw new NXFileException("Index out of bounds of bitmap offset table");

            var offset = File.Accessor.ReadInt64(Offset + id * 8);
            var length = File.Accessor.ReadUInt32(offset);
            
            using (var source = File.View.CreateViewStream(offset + 4, length))
            using (var target = LZ4Stream.Decode(source))
                return target;
        }
    }
}