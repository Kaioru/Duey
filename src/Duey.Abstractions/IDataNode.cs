namespace Duey.Abstractions;

public interface IDataNode : IEnumerable<IDataNode>
{
    string Name { get; }
    
    IDataNode Parent { get; }
    IEnumerable<IDataNode> Children { get; }
    
    IDataNode? ResolvePath(string path);
}
