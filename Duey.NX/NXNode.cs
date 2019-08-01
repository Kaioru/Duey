using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using Duey.NX.Exceptions;
using Duey.NX.Layout;
using Duey.NX.Layout.Nodes;

namespace Duey.NX
{
    public class NXNode : INXNode
    {
        public NXNodeType Type => Header.Type;
        public string Name => File.StringOffsetTable.Get(Header.StringID);
        public INXNode Parent { get; }

        public IEnumerable<INXNode> Children
        {
            get
            {
                for (var i = 0; i < Header.ChildCount; i++)
                    yield return new NXNode(File, this, File.Header.NodeBlock + (Header.ChildID + i) * 20);
            }
        }

        internal readonly NXFile File;
        internal readonly NXNodeHeader Header;
        internal readonly long Start;

        internal NXNode(NXFile file, INXNode parent, long start)
        {
            File = file;
            Parent = parent;

            File.Accessor.Read(start, out Header);
            Start = start + Marshal.SizeOf<NXNodeHeader>();
        }

        public INXNode ResolveAll()
            => new NXResolutionNode(this);
        
        public void ResolveAll(Action<INXNode> context)
            => context.Invoke(ResolveAll());
        
        public object Resolve()
        {
            switch (Type)
            {
                case NXNodeType.None:
                    return null;
                case NXNodeType.Int64:
                {
                    File.Accessor.Read(Start, out NXInt64Node node);
                    return node.Data;
                }
                case NXNodeType.Double:
                {
                    File.Accessor.Read(Start, out NXDoubleNode node);
                    return node.Data;
                }
                case NXNodeType.String:
                {
                    File.Accessor.Read(Start, out NXStringNode node);
                    return File.StringOffsetTable.Get(node);
                }
                case NXNodeType.Vector:
                {
                    File.Accessor.Read(Start, out NXVectorNode node);
                    return new Point(node.X, node.Y);
                }
                case NXNodeType.Bitmap:
                {
                    File.Accessor.Read(Start, out NXBitmapNode node);
                    return File.BitmapOffsetTable.Get(node);
                }
                case NXNodeType.Audio:
                {
                    File.Accessor.Read(Start, out NXAudioNode node);
                    return File.AudioOffsetTable.Get(node);
                }
                default:
                    throw new NXFileException($"Tried to resolve an unsupported node type {Header.Type}");
            }
        }

        public INXNode ResolvePath(string path = null)
        {
            if (string.IsNullOrEmpty(path)) return this;

            var forwardSlashPosition = path.IndexOf('/');
            var backSlashPosition = path.IndexOf('\\', 0, forwardSlashPosition == -1
                ? path.Length
                : forwardSlashPosition);
            int firstSlash;

            if (forwardSlashPosition == -1) firstSlash = backSlashPosition;
            else if (backSlashPosition == -1) firstSlash = forwardSlashPosition;
            else firstSlash = Math.Min(forwardSlashPosition, backSlashPosition);

            if (firstSlash == -1) firstSlash = path.Length;

            var childName = path.Substring(0, firstSlash);

            if (childName == ".." || childName == ".")
                return Parent.ResolvePath(path.Substring(Math.Min(firstSlash + 1, path.Length)));

            var child = Children.FirstOrDefault(
                c => c.Name.Equals(childName, StringComparison.CurrentCultureIgnoreCase)
            );

            return child?.ResolvePath(path.Substring(Math.Min(firstSlash + 1, path.Length)));
        }

        public T? Resolve<T>(string path = null) where T : struct
        {
            var res = ResolvePath(path)?.Resolve();
            if (res is IConvertible)
                return (T) Convert.ChangeType(res, typeof(T));
            return null;
        }

        public T ResolveOrDefault<T>(string path = null) where T : class
            => (T) ResolvePath(path)?.Resolve();

        public IEnumerator<INXNode> GetEnumerator()
            => Children.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }
}