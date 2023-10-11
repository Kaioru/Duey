using Duey.Abstractions;

namespace Duey.Provider.WZ.Files;

public class WZPropertyData<T> : AbstractWZNode, IDataProperty<T>
{
    private readonly T _value;

    public WZPropertyData(string name, IDataNode parent, T value)
    {
        Name = name;
        Parent = parent;
        _value = value;
    }
    
    public override string Name { get; }
    public override IDataNode Parent { get; }
    public override IEnumerable<IDataNode> Children => Array.Empty<IDataNode>();

    public T Resolve() => _value;
}
