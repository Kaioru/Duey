namespace Duey.Abstractions.Types;

public struct DataAudio
{
    public DataAudio(byte[] data) => Data = data;

    public byte[] Data { get; }
}
