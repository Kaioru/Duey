using System.Runtime.InteropServices;

namespace Duey.Layout.Nodes
{
    [StructLayout(LayoutKind.Explicit, Size = 8, Pack = 2)]
    internal struct NXDoubleNode
    {
        [FieldOffset(0)] internal readonly double Data;
    }
}