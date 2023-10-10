using System.Runtime.InteropServices;

namespace Duey.Provider.NX.Headers.Properties;

[StructLayout(LayoutKind.Explicit, Size = 8, Pack = 2)]
internal struct NXPropertyDoubleHeader
{
    [FieldOffset(0)] internal readonly double Data;
}
