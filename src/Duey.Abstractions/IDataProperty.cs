namespace Duey.Abstractions;

public interface IDataProperty<out T> : IDataNode
{
    T Resolve();
}
