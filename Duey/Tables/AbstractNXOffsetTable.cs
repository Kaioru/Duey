namespace Duey.Tables
{
    internal abstract class AbstractNXOffsetTable<TReturn, TData>
        where TData : struct
    {
        internal readonly NXFile File;
        internal readonly uint Count;
        internal readonly long Offset;

        public AbstractNXOffsetTable(
            NXFile file,
            uint count,
            long offset
        )
        {
            File = file;
            Count = count;
            Offset = offset;
        }

        public abstract TReturn Get(TData data);
    }
}