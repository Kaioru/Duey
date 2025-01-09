using Duey.Abstractions;
using Duey.Provider.WZ.Crypto;
using Duey.Provider.WZ.Files;

namespace Duey.Provider.WZ;

public class FSDirectory : AbstractWZNode, IDataNodeCached, IDataDirectory
{
    public FSDirectory(string path, XORCipher? cipher = null, IDataNode? parent = null)
    {
        Name = Path.GetFileName(path);
        Parent = parent ?? this;
        Cached = Directory
            .GetDirectories(path)
            .Select(d => new FSDirectory(d, cipher, this))
            .Concat<IDataNode>(Directory
                .GetFiles(path, "*.img")
                .Select(f => new WZImage(f, cipher, this)))
            .ToDictionary(n => n.Name, n => n);
    }
    
    public override string Name { get; }
    public override IDataNode Parent { get; }

    public override IEnumerable<IDataNode> Children => Cached.Values;

    public IDictionary<string, IDataNode> Cached { get; }
}
