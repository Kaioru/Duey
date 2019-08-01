namespace Duey.NX.Types
{
    public class NXBitmap
    {
        public ushort Height { get; }
        public ushort Width { get; }
        public byte[] Data { get; }
        
        public NXBitmap(ushort height, ushort width, byte[] data)
        {
            Height = height;
            Width = width;
            Data = data;
        }
    }
}