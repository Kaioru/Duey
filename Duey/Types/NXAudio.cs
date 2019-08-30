namespace Duey.Types
{
    public class NXAudio
    {
        public byte[] Data { get; }
        
        public NXAudio(byte[] data)
        {
            Data = data;
        }
    }
}