using System.Collections;

namespace Duey.Abstractions;

public class DataNodeCached : IDataNodeCached
{
    public IDataNode Node { get; }
    public IDictionary<string, IDataNode> Cached { get; }
    
    public DataNodeCached(IDataNode node)
    {
        Node = node;
        Cached = Node.ToDictionary(n => n.Name, n => n);
    }
    
    public string Name => Node.Name;
    public IDataNode Parent => Node.Parent;
    public IEnumerable<IDataNode> Children => Cached.Values;
    
    public IEnumerator<IDataNode> GetEnumerator()
        => Cached.Values.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();
}
