using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using Duey.NX.Exceptions;
using Duey.NX.Layout;
using Duey.NX.Layout.Nodes;

namespace Duey.NX
{
    public class NXNode
    {
        public string Name => File.StringOffsetTable.Get(Header.StringID);
        public NXNode Parent { get; }

        public IEnumerable<NXNode> Children
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

        internal NXNode(NXFile file, NXNode parent, long start)
        {
            File = file;
            Parent = parent;

            File.Accessor.Read(start, out Header);
            Start = start + Marshal.SizeOf<NXNodeHeader>();
        }

        private object InternalResolve()
        {
            switch (Header.Type)
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
                /*
                case NXNodeType.Bitmap:
                    // TODO: Bitmap
                    return null;
                case NXNodeType.Audio:
                    // TODO: Audio
                    return null;
                */
                default:
                    throw new NXFileException($"Tried to resolve an unsupported node type {Header.Type}");
            }
        }

        public NXNode Resolve(string path = null)
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
                return Parent.Resolve(path.Substring(Math.Min(firstSlash + 1, path.Length)));

            var child = Children.FirstOrDefault(
                c => c.Name.Equals(childName, StringComparison.CurrentCultureIgnoreCase)
            );

            return child?.Resolve(path.Substring(Math.Min(firstSlash + 1, path.Length)));
        }

        public T? Resolve<T>(string path = null) where T : struct
        {
            var res = Resolve(path)?.InternalResolve();
            if (res is IConvertible)
                return (T) Convert.ChangeType(res, typeof(T));
            return null;
        }

        public T ResolveOrDefault<T>(string path = null) where T : class
            => (T) Resolve(path)?.InternalResolve() ?? null;
    }
}