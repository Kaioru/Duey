using System.Runtime.InteropServices;

namespace Duey.NX.Layout.Nodes
{
    [StructLayout(LayoutKind.Explicit, Size = 8, Pack = 2)]
    internal struct NXStringNode
    {
        [FieldOffset(0)] internal readonly uint StringID;
    }
}