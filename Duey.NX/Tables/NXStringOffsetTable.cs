using System;
using System.IO;
using System.Text;
using Duey.NX.Exceptions;
using Duey.NX.Layout.Nodes;

namespace Duey.NX.Tables
{
    internal class NXStringOffsetTable : AbstractNXOffsetTable<string, NXStringNode>
    {
        public NXStringOffsetTable(
            UnmanagedMemoryAccessor accessor,
            uint count,
            long offset
        ) : base(accessor, count, offset)
        {
        }

        public override string Get(NXStringNode data)
            => Get(data.StringID);

        public string Get(uint id)
        {
            if (id > Count) throw new NXFileException("Index out of bounds of string offset table");

            var offset = Accessor.ReadInt64(Offset + id * 8);
            var stringLength = Accessor.ReadUInt16(offset);
            var stringData = new byte[stringLength];

            Accessor.ReadArray(offset + 2, stringData, 0, stringLength);
            return Encoding.Default.GetString(stringData);
        }
    }
}