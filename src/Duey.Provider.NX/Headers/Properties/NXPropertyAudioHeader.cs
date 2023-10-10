using System.Runtime.InteropServices;

namespace Duey.Provider.NX.Headers.Properties;

[StructLayout(LayoutKind.Explicit, Size = 8, Pack = 2)]
internal struct NXPropertyAudioHeader
{
    [FieldOffset(0)] internal readonly uint AudioID;
    [FieldOffset(4)] internal readonly uint Length;
}