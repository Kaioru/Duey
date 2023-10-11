using System.Collections;

namespace Duey.Abstractions;

public class ResolutionNode : IDataNode
{
    internal readonly IDataNode Node;
    internal readonly Dictionary<string, IDataNode> Cached;
    
    public ResolutionNode(IDataNode node)
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
