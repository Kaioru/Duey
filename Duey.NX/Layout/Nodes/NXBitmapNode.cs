using System.Runtime.InteropServices;

namespace Duey.NX.Layout.Nodes
{
    [StructLayout(LayoutKind.Explicit, Size = 8, Pack = 2)]
    internal struct NXBitmapNode
    {
        [FieldOffset(0)] internal readonly uint BitmapID;
        [FieldOffset(4)] internal readonly ushort Width;
        [FieldOffset(6)] internal readonly ushort Height;
    }
}