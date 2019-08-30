namespace Duey.Types
{
    public class NXBitmap
    {
        public ushort Width { get; }
        public ushort Height { get; }
        public byte[] Data { get; }
        
        public NXBitmap(ushort width, ushort height, byte[] data)
        {
            Width = width;
            Height = height;
            Data = data;
        }
    }
}