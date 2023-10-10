namespace Duey.Provider.NX.Headers;

public enum NXNodeType : ushort
{
    None = 0x0,
    Int64 = 0x1,
    Double = 0x2,
    String = 0x3,
    Vector = 0x4,
    Bitmap = 0x5,
    Audio = 0x6
}