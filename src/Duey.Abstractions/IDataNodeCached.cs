namespace Duey.Abstractions;

public interface IDataNodeCached : IDataNode
{
    IDictionary<string, IDataNode> Cached { get; }
}
