using Duey.Abstractions;
using Duey.Provider.WZ.Crypto;

namespace Duey.Provider.WZ;

public class WZNamespace : AbstractWZNode, IDataNamespace
{
    private readonly string _path;
    private readonly string _key;
    private readonly XORCipher? _cipher;
    
    public WZNamespace(string path, string key, XORCipher? cipher = null)
    {
        _path = path;
        _key = key;
        _cipher = cipher;
        Name = Path.GetFileName(path);
        Parent = this;
    }
    
    public override string Name { get; }
    public override IDataNode Parent { get; }

    public override IEnumerable<IDataNode> Children
    {
        get
        {
            foreach (var file in Directory.GetFiles(_path, "*.wz"))
                yield return new WZPackage(file, _key, _cipher);
        }
    }
}