using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using Duey.Exceptions;
using Duey.Layout;
using Duey.Tables;

namespace Duey
{
    public class NXFile : INXNode, IDisposable
    {
        public NXNode Root { get; }

        public NXNodeType Type => Root.Type;
        public string Name => Root.Name;
        public INXNode Parent => null;
        public IEnumerable<INXNode> Children => Root.Children;

        internal readonly MemoryMappedFile View;
        internal readonly UnmanagedMemoryAccessor Accessor;
        internal readonly NXFileHeader Header;

        internal readonly NXStringOffsetTable StringOffsetTable;
        internal readonly NXBitmapOffsetTable BitmapOffsetTable;
        internal readonly NXAudioOffsetTable AudioOffsetTable;

        public NXFile(string path) : this(MemoryMappedFile.CreateFromFile(path))
        {
        }

        public NXFile(MemoryMappedFile view)
        {
            View = view;
            Accessor = View.CreateViewAccessor();

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

            StringOffsetTable = new NXStringOffsetTable(this, Header.StringCount, Header.StringOffsetTable);
            BitmapOffsetTable = new NXBitmapOffsetTable(this, Header.BitmapCount, Header.BitmapOffsetTable);
            AudioOffsetTable = new NXAudioOffsetTable(this, Header.AudioCount, Header.AudioOffsetTable);
            Root = new NXNode(this, null, Header.NodeBlock);
        }

        public INXNode ResolveAll()
            => Root.ResolveAll();

        public void ResolveAll(Action<INXNode> context)
            => Root.ResolveAll(context);

        public object Resolve() => null;

        public INXNode ResolvePath(string path = null)
            => Root.ResolvePath(path);

        public T? Resolve<T>(string path = null) where T : struct
            => Root.Resolve<T>(path);

        public T ResolveOrDefault<T>(string path = null) where T : class
            => Root.ResolveOrDefault<T>(path);

        public void Dispose()
        {
            View?.Dispose();
            Accessor?.Dispose();
        }

        public IEnumerator<INXNode> GetEnumerator()
            => Root.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }
}