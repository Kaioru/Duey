using System.Runtime.InteropServices;

namespace Duey.NX.Layout
{
    [StructLayout(LayoutKind.Explicit, Size = 12, Pack = 2)]
    internal struct NXNodeHeader
    {
        [FieldOffset(0)] internal readonly uint StringID;
        [FieldOffset(4)] internal readonly uint ChildID;
        [FieldOffset(8)] internal readonly ushort ChildCount;
        [FieldOffset(10)] internal readonly NXNodeType Type;
    }
}