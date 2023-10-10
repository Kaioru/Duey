using Duey.Abstractions.Types;

namespace Duey.Abstractions;

public static class DataExtensions
{
    public static IDataNode? ResolvePath(this IDataNode node, string? path = null)
    {
        if (string.IsNullOrEmpty(path)) return node;

        var forwardSlashPosition = path.IndexOf('/');
        var backSlashPosition = path.IndexOf('\\', 0, forwardSlashPosition == -1
            ? path.Length
            : forwardSlashPosition);
        int firstSlash;

        if (forwardSlashPosition == -1) firstSlash = backSlashPosition;
        else if (backSlashPosition == -1) firstSlash = forwardSlashPosition;
        else firstSlash = Math.Min(forwardSlashPosition, backSlashPosition);

        if (firstSlash == -1) firstSlash = path.Length;

        var childName = path[..firstSlash];

        if (childName is ".." or ".")
            return node.Parent.ResolvePath(path[Math.Min(firstSlash + 1, path.Length)..]);

        var child = node.Children.FirstOrDefault(
            c => c.Name.Equals(childName)
        );

        return child?.ResolvePath(path[Math.Min(firstSlash + 1, path.Length)..]);
    }
    
    public static long? ResolveLong(this IDataNode node, string? path = null)
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
