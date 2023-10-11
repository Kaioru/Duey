using Duey.Abstractions;
using Duey.Provider.WZ.Crypto;
using Duey.Provider.WZ.Exceptions;

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

    public override IEnumerable<IDataNode> Children => Directory
        .GetFiles(_path, "*.wz")
        .Select(f =>
        {
            try
            {
                return new WZPackage(f, _key, _cipher);
            }
            catch (WZPackageException)
            {
                return null;
            }
        })
        .Where(n => n != null)
        .Select(n => n!);
}