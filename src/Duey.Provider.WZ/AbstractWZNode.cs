using System.Collections;
using Duey.Abstractions;

namespace Duey.Provider.WZ;

public abstract class AbstractWZNode : IDataNode
{
    public abstract string Name { get; }
    public abstract IDataNode Parent { get; }
    
    public abstract IEnumerable<IDataNode> Children { get; }

    public IEnumerator<IDataNode> GetEnumerator()
        => Children.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();
}
