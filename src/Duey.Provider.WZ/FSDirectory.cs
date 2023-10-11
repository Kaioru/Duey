using System.Collections;
using System.IO.MemoryMappedFiles;
using Duey.Abstractions;
using Duey.Provider.WZ.Crypto;
using Duey.Provider.WZ.Files;

namespace Duey.Provider.WZ;

public class FSDirectory : AbstractWZNode, IDataNodeCached, IDataDirectory
{
    private readonly string _path;
    private readonly XORCipher? _cipher;
    
    public FSDirectory(string path, XORCipher? cipher = null, IDataNode? parent = null)
    {
        _path = path;
        _cipher = cipher;
        Name = Path.GetFileName(path);
        Parent = parent ?? this;
        Cached = Directory
            .GetDirectories(_path)
            .Select(d => new FSDirectory(d, _cipher, this))
            .Concat<IDataNode>(Directory
                .GetFiles(_path, "*.img")
                .Select(f => new WZImage(f, _cipher, this)))
            .ToDictionary(n => n.Name, n => n);
    }
    
    public override string Name { get; }
    public override IDataNode Parent { get; }

    public override IEnumerable<IDataNode> Children => Cached.Values;

    public IDictionary<string, IDataNode> Cached { get; }
}
