using Duey.NX.Exceptions;
using Duey.NX.Layout.Nodes;
using Duey.NX.Types;
using K4os.Compression.LZ4;

namespace Duey.NX.Tables
{
    internal class NXBitmapOffsetTable : AbstractNXOffsetTable<NXBitmap, NXBitmapNode>
    {
        public NXBitmapOffsetTable(
            NXFile file,
            uint count,
            long offset
        ) : base(file, count, offset)
        {
        }

        public override NXBitmap Get(NXBitmapNode data)
        {
            var id = data.BitmapID;

            if (id > Count) throw new NXFileException("Index out of bounds of bitmap offset table");

            var offset = File.Accessor.ReadInt64(Offset + id * 8);
            var sourceLength = File.Accessor.ReadInt32(offset);
            var source = new byte[sourceLength];
            var target = new byte[data.Height * data.Width * 4];

            File.Accessor.ReadArray(offset + 4, source, 0, sourceLength);
            LZ4Codec.Decode(source, target);

            return new NXBitmap(data.Width, data.Height, target);
        }
    }
}