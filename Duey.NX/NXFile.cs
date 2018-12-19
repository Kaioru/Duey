using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using Duey.NX.Layout;
using Duey.NX.Tables;

namespace Duey.NX
{
    public class NXFile : IDisposable
    {
        public NXNode Root { get; }

        internal readonly MemoryMappedFile File;
        internal readonly UnmanagedMemoryAccessor Accessor;
        internal readonly NXFileHeader Header;

        internal readonly NXStringOffsetTable StringOffsetTable;

        public NXFile(MemoryMappedFile file)
        {
            File = file;
            Accessor = File.CreateViewAccessor();

            Accessor.Read(0, out Header);

            if (Header.Magic != 0x34474B50) throw new Exception();
            if (Header.NodeCount == 0) throw new Exception();
            if (Header.NodeBlock % 4 != 0) throw new Exception();
            if (Header.StringCount == 0) throw new Exception();
            if (Header.StringOffsetTable % 8 != 0) throw new Exception();
            if (Header.BitmapCount > 0 &&
                Header.BitmapOffsetTable % 8 != 0) throw new Exception();
            if (Header.AudioCount > 0 &&
                Header.AudioOffsetTable % 8 != 0) throw new Exception();

            StringOffsetTable = new NXStringOffsetTable(Accessor, Header.StringCount, Header.StringOffsetTable);
            Root = new NXNode(this, null, Header.NodeBlock);
        }

        public void Dispose()
        {
            File?.Dispose();
            Accessor?.Dispose();
        }
    }
}