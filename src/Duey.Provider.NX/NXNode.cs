using System.Collections;
using System.Runtime.InteropServices;
using Duey.Abstractions;
using Duey.Provider.NX.Headers;
using Duey.Provider.NX.Headers.Properties;

namespace Duey.Provider.NX;

public class NXNode : IDataNode
{
    internal NXNode(NXPackage package, NXNodeHeader header)
    {
        Package = package;
        Header = header;
    }

    internal readonly NXPackage Package;
    internal readonly NXNodeHeader Header;
    
    public string Name => Package.StringOffsetTable.Get(Header.StringID);
    
    public IDataNode Parent => this;
    public IEnumerable<IDataNode> Children 
    {
        get
        {
            for (var i = 0; i < Header.ChildCount; i++)
            {
                var start = Package.Header.NodeBlock + (Header.ChildID + i) * 20;
                
                Package.Accessor.Read(start, out NXNodeHeader header);
                start += Marshal.SizeOf<NXNodeHeader>();

                switch (header.Type)
                {
                    case NXNodeType.None:
                        Package.Accessor.Read(start, out NXPropertyNoneHeader _);
                        yield return new NXNode(Package, header);
                        break;
                    case NXNodeType.Int64:
                    {
                        Package.Accessor.Read(start, out NXPropertyInt64Header data);
                        yield return new NXPropertyInt64(Package, header, data);
                        break;
                    }
                    case NXNodeType.Double:
                    {
                        Package.Accessor.Read(start, out NXPropertyDoubleHeader data);
                        yield return new NXPropertyDouble(Package, header, data);
                        break;
                    }
                    case NXNodeType.String:
                    {
                        Package.Accessor.Read(start, out NXPropertyStringHeader data);
                        yield return new NXPropertyString(Package, header, data);
                        break;
                    }
                    case NXNodeType.Vector:
                    {
                        Package.Accessor.Read(start, out NXPropertyVectorHeader data);
                        yield return new NXPropertyVector(Package, header, data);
                        break;
                    }
                    case NXNodeType.Bitmap:
                    {
                        Package.Accessor.Read(start, out NXPropertyBitmapHeader data);
                        yield return new NXPropertyBitmap(Package, header, data);
                        break;
                    }
                    case NXNodeType.Audio:
                    {
                        Package.Accessor.Read(start, out NXPropertyAudioHeader data);
                        yield return new NXPropertyAudio(Package, header, data);
                        break;
                    }
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }

    public IDataNode? ResolvePath(string path)
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

        var childName = path[..firstSlash];

        if (childName is ".." or ".")
            return Parent.ResolvePath(path[Math.Min(firstSlash + 1, path.Length)..]);

        var child = Children.FirstOrDefault(
            c => c.Name.Equals(childName)
        );

        return child?.ResolvePath(path[Math.Min(firstSlash + 1, path.Length)..]);
    }

    public IEnumerator<IDataNode> GetEnumerator()
        => Children.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();
}