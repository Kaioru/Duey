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
    }
    
    public string Name { get; }
    public IDataNode Parent { get; }
    
    public IEnumerable<IDataNode> Children 
    {
        get
        {
            foreach (var file in Directory.GetFiles(_path, "*.nx"))
                yield return new NXPackage(file);
        }
    }

    public IEnumerator<IDataNode> GetEnumerator()
        => Children.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();
}