namespace Duey.Provider.NX.Tables;

internal abstract class AbstractNXOffsetTable<TReturn, TData>
    where TData : struct
{
    internal readonly NXPackage Package;
    internal readonly uint Count;
    internal readonly long Offset;

    public AbstractNXOffsetTable(
        NXPackage package,
        uint count,
        long offset
    )
    {
        Package = package;
        Count = count;
        Offset = offset;
    }

    public abstract TReturn Get(TData data);
}