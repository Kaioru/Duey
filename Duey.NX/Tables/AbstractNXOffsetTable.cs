using System.IO;

namespace Duey.NX.Tables
{
    internal abstract class AbstractNXOffsetTable<TReturn, TData>
        where TData : struct
    {
        internal readonly UnmanagedMemoryAccessor Accessor;
        internal readonly uint Count;
        internal readonly long Offset;

        public AbstractNXOffsetTable(
            UnmanagedMemoryAccessor accessor,
            uint count,
            long offset
        )
        {
            Accessor = accessor;
            Count = count;
            Offset = offset;
        }

        public abstract TReturn Get(TData data);
    }
}