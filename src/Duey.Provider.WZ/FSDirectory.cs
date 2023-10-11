using System.Collections;
using System.IO.MemoryMappedFiles;
using Duey.Abstractions;
using Duey.Provider.WZ.Crypto;
using Duey.Provider.WZ.Files;

namespace Duey.Provider.WZ;

public class FSDirectory : AbstractWZNode, IDataDirectory
{
    private readonly string _path;
    private readonly XORCipher? _cipher;
    
    public FSDirectory(string path, XORCipher? cipher = null, IDataNode? parent = null)
    {
        _path = path;
        _cipher = cipher;
        Name = Path.GetFileName(path);
        Parent = parent ?? this;
    }
    
    public override string Name { get; }
    public override IDataNode Parent { get; }

    public override IEnumerable<IDataNode> Children
    {
        get
        {
            foreach (var directory in Directory.GetDirectories(_path))
                yield return new FSDirectory(directory, _cipher, this);
            foreach (var file in Directory.GetFiles(_path, "*.img"))
                yield return new WZImage(file, _cipher, this);
        }
    }
}
