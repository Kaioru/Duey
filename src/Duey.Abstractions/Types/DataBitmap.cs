namespace Duey.Abstractions.Types;

public struct DataBitmap
{
    public DataBitmap(ushort width, ushort height, byte[] data)
    {
        Width = width;
        Height = height;
        Data = data;
    }
    
    public ushort Width { get; }
    public ushort Height { get; }
    public byte[] Data { get; }
}
