using System.Runtime.InteropServices;

namespace Duey.NX.Layout.Nodes
{
    [StructLayout(LayoutKind.Explicit, Size = 8, Pack = 2)]
    internal struct NXVectorNode
    {
        [FieldOffset(0)] internal readonly int X;
        [FieldOffset(4)] internal readonly int Y;
    }
}