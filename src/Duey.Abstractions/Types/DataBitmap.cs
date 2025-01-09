namespace Duey.Abstractions.Types;

public record struct DataBitmap(
    ushort Width, 
    ushort Height, 
    DataBitmapFormat Format,
    Memory<byte> Data
);