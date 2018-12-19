using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using Duey.NX.Exceptions;
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

        public NXFile(string path) : this(MemoryMappedFile.CreateFromFile(path))
        {
        }

        public NXFile(MemoryMappedFile file)
        {
            File = file;
            Accessor = File.CreateViewAccessor();

            Accessor.Read(0, out Header);

            if (Header.Magic != 0x34474B50) throw new NXFileException("Invalid magic value");
            if (Header.NodeCount == 0) throw new NXFileException("Node count cannot be 0");
            if (Header.NodeBlock % 4 != 0) throw new NXFileException("Node block offset not divisible by 4");
            if (Header.StringCount == 0) throw new NXFileException("String count cannot be 0");
            if (Header.StringOffsetTable % 8 != 0)
                throw new NXFileException("String offset table offset not divisible by 8");
            if (Header.BitmapCount > 0 &&
                Header.BitmapOffsetTable % 8 != 0)
                throw new NXFileException("Bitmap offset table offset not divisible by 8");
            if (Header.AudioCount > 0 &&
                Header.AudioOffsetTable % 8 != 0)
                throw new NXFileException("Audio offset table offset not divisible by 8");

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