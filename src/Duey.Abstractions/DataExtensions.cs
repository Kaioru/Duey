using Duey.Abstractions.Types;

namespace Duey.Abstractions;

public static class DataExtensions
{
    public static long? ResolveInt64(this IDataNode node, string? path = null)
        => ((path != null ? node.ResolvePath(path) : node) as IDataProperty<long>)?.Resolve();
    
    public static double? ResolveDouble(this IDataNode node, string? path = null)
        => ((path != null ? node.ResolvePath(path) : node) as IDataProperty<double>)?.Resolve();
    
    public static string? ResolveString(this IDataNode node, string? path = null)
        => ((path != null ? node.ResolvePath(path) : node) as IDataProperty<string>)?.Resolve();
    
    public static (int, int)? ResolveVector(this IDataNode node, string? path = null)
    {
        var vector = ((path != null ? node.ResolvePath(path) : node) as IDataProperty<DataVector>)?.Resolve();
        if (vector == null) return null;
        return (vector.Value.X, vector.Value.Y);
    }
    
    public static DataBitmap? ResolveBitmap(this IDataNode node, string? path = null)
        => ((path != null ? node.ResolvePath(path) : node) as IDataProperty<DataBitmap>)?.Resolve();
    
    public static DataAudio? ResolveAudio(this IDataNode node, string? path = null)
        => ((path != null ? node.ResolvePath(path) : node) as IDataProperty<DataAudio>)?.Resolve();
}
