using System.Runtime.InteropServices;

namespace Duey.Provider.NX.Headers.Properties;

[StructLayout(LayoutKind.Explicit, Size = 8, Pack = 2)]
internal struct NXPropertyVectorHeader
{
    [FieldOffset(0)] internal readonly int X;
    [FieldOffset(4)] internal readonly int Y;
}