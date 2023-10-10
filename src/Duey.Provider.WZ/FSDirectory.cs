using System.Collections;
using System.IO.MemoryMappedFiles;
using Duey.Abstractions;
using Duey.Provider.WZ.Crypto;

namespace Duey.Provider.WZ;

public class FSDirectory : AbstractWZNode, IDataDirectory
{
    private readonly string _path;
    
    public FSDirectory(string path, IDataNode? parent = null)
    {
        _path = path;
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
                yield return new FSDirectory(directory, this);
            foreach (var file in Directory.GetFiles(_path, "*.img"))
                yield return new WZFile(file, null, this);
        }
    }
}
