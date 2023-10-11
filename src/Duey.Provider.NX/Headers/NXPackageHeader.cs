using System.Runtime.InteropServices;

namespace Duey.Provider.NX.Headers;

[StructLayout(LayoutKind.Explicit, Pack = 4, Size = 52)]
internal struct NXPackageHeader
{
    [FieldOffset(0)] internal readonly uint Magic;
    [FieldOffset(4)] internal readonly uint NodeCount;
    [FieldOffset(8)] internal readonly long NodeBlock;
    [FieldOffset(16)] internal readonly uint StringCount;
    [FieldOffset(20)] internal readonly long StringOffsetTable;
    [FieldOffset(28)] internal readonly uint BitmapCount;
    [FieldOffset(32)] internal readonly long BitmapOffsetTable;
    [FieldOffset(40)] internal readonly uint AudioCount;
    [FieldOffset(44)] internal readonly long AudioOffsetTable;
}