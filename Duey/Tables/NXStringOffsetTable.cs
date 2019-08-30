using System.Text;
using Duey.Exceptions;
using Duey.Layout.Nodes;

namespace Duey.Tables
{
    internal class NXStringOffsetTable : AbstractNXOffsetTable<string, NXStringNode>
    {
        public NXStringOffsetTable(
            NXFile file,
            uint count,
            long offset
        ) : base(file, count, offset)
        {
        }

        public override string Get(NXStringNode data)
            => Get(data.StringID);

        public string Get(uint id)
        {
            if (id > Count) throw new NXFileException("Index out of bounds of string offset table");

            var offset = File.Accessor.ReadInt64(Offset + id * 8);
            var stringLength = File.Accessor.ReadUInt16(offset);
            var stringData = new byte[stringLength];

            File.Accessor.ReadArray(offset + 2, stringData, 0, stringLength);
            return Encoding.UTF8.GetString(stringData);
        }
    }
}