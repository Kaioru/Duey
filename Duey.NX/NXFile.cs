using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using Duey.NX.Exceptions;
using Duey.NX.Layout;
using Duey.NX.Tables;

namespace Duey.NX
{
    public class NXFile : IDisposable, IEnumerable<NXNode>
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

        public NXNode Resolve(string path = null)
            => Root.Resolve(path);

        public T? Resolve<T>(string path = null) where T : struct
            => Root.Resolve<T>(path);

        public T ResolveOrDefault<T>(string path = null) where T : class
            => Root.ResolveOrDefault<T>(path);

        public void Dispose()
        {
            File?.Dispose();
            Accessor?.Dispose();
        }

        public IEnumerator<NXNode> GetEnumerator()
            => Root.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }
}