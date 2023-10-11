using System.Collections;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using Duey.Abstractions;
using Duey.Provider.NX.Exceptions;
using Duey.Provider.NX.Headers;
using Duey.Provider.NX.Tables;

namespace Duey.Provider.NX;

public class NXPackage : IDataFile
{
    public NXPackage(string path) : this(MemoryMappedFile.CreateFromFile(
        File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read),
        null,
        0,
        MemoryMappedFileAccess.Read,
        HandleInheritability.None,
        false),
        Path.GetFileNameWithoutExtension(path)
    )
    {
    }

    public NXPackage(MemoryMappedFile view, string name)
    {
        View = view;
        Accessor = View.CreateViewAccessor(0, 0, MemoryMappedFileAccess.Read);
        Name = name;

        Accessor.Read(0, out Header);

        if (Header.Magic != 0x34474B50) throw new NXPackageException("Invalid magic value");
        if (Header.NodeCount == 0) throw new NXPackageException("Node count cannot be 0");
        if (Header.NodeBlock % 4 != 0) throw new NXPackageException("Node block offset not divisible by 4");
        if (Header.StringCount == 0) throw new NXPackageException("String count cannot be 0");
        if (Header.StringOffsetTable % 8 != 0)
            throw new NXPackageException("String offset table offset not divisible by 8");
        if (Header.BitmapCount > 0 &&
            Header.BitmapOffsetTable % 8 != 0)
            throw new NXPackageException("Bitmap offset table offset not divisible by 8");
        if (Header.AudioCount > 0 &&
            Header.AudioOffsetTable % 8 != 0)
            throw new NXPackageException("Audio offset table offset not divisible by 8");

        Accessor.Read(Header.NodeBlock, out NXNodeHeader header);
        Root = new NXNode(this, header);

        StringOffsetTable = new NXStringOffsetTable(this, Header.StringCount, Header.StringOffsetTable);
        BitmapOffsetTable = new NXBitmapOffsetTable(this, Header.BitmapCount, Header.BitmapOffsetTable);
        AudioOffsetTable = new NXAudioOffsetTable(this, Header.AudioCount, Header.AudioOffsetTable);
    }

    internal readonly IDataNode Root;

    internal readonly MemoryMappedFile View;
    internal readonly UnmanagedMemoryAccessor Accessor;
    internal readonly NXPackageHeader Header;

    internal readonly NXStringOffsetTable StringOffsetTable;
    internal readonly NXBitmapOffsetTable BitmapOffsetTable;
    internal readonly NXAudioOffsetTable AudioOffsetTable;

    public string Name { get; }

    public IDataNode Parent => Root;
    public IEnumerable<IDataNode> Children => Root.Children;

    public IEnumerator<IDataNode> GetEnumerator()
        => Root.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();
}
