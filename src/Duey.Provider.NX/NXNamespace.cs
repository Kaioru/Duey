using System.Collections;
using Duey.Abstractions;

namespace Duey.Provider.NX;

public class NXNamespace : IDataNamespace
{
    private readonly string _path;
    
    public NXNamespace(string path)
    {
        _path = path;
        Name = Path.GetFileName(path);
        Parent = this;
        Cached = Directory
            .GetFiles(_path, "*.nx")
            .Select(f => (IDataNode)new NXPackage(f))
            .ToDictionary(n => n.Name, n => n);
    }
    
    public string Name { get; }
    public IDataNode Parent { get; }

    public IEnumerable<IDataNode> Children => Cached.Values;
    
    public IDictionary<string, IDataNode> Cached { get; }

    public IEnumerator<IDataNode> GetEnumerator()
        => Children.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();
}