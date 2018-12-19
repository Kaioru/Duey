using System.Runtime.InteropServices;

namespace Duey.NX.Layout.Nodes
{
    [StructLayout(LayoutKind.Explicit, Size = 8, Pack = 2)]
    internal struct NXInt64Node
    {
        [FieldOffset(0)] internal readonly long Data;
    }
}