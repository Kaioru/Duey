using System.Runtime.InteropServices;

namespace Duey.Provider.NX.Headers.Properties;

[StructLayout(LayoutKind.Explicit, Size = 8, Pack = 2)]
internal struct NXPropertyInt64Header
{
    [FieldOffset(0)] internal readonly long Data;
}