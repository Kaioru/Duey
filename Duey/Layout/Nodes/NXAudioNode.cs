using System.Runtime.InteropServices;

namespace Duey.Layout.Nodes
{
    [StructLayout(LayoutKind.Explicit, Size = 8, Pack = 2)]
    internal struct NXAudioNode
    {
        [FieldOffset(0)] internal readonly uint AudioID;
        [FieldOffset(4)] internal readonly uint Length;
    }
}