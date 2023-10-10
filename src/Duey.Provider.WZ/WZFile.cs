using System.IO.MemoryMappedFiles;
using Duey.Abstractions;
using Duey.Provider.WZ.Codecs;
using Duey.Provider.WZ.Crypto;
using Duey.Provider.WZ.Exceptions;

namespace Duey.Provider.WZ;

public class WZFile : AbstractWZNode, IDataFile
{
    private readonly IDataNode _root;

    public WZFile(string path, XORCipher? cipher = null, IDataNode? parent = null) : this(
        MemoryMappedFile.CreateFromFile(
            File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read),
            null,
            0,
            MemoryMappedFileAccess.Read,
            HandleInheritability.None,
            false
        ),
        cipher ?? new XORCipher(),
        0,
        0,
        Path.GetFileName(path),
        parent
    )
    {
        
    }
    
    public WZFile(MemoryMappedFile view, XORCipher cipher, int start, int offset, string name, IDataNode? parent = null)
    {
        using var stream = view.CreateViewStream(offset, 0, MemoryMappedFileAccess.Read);
        using var reader = new WZReader(stream, cipher, start);

        if (reader.ReadStringBlock() != "Property") throw new WZPackageException("Loaded file is not a property");

        _root = new WZPropertyFile(view, cipher, (int)stream.Position, offset, name, parent);
    }

    public override string Name => _root.Name;
    public override IDataNode Parent => _root.Parent;

    public override IEnumerable<IDataNode> Children => _root.Children;
}
