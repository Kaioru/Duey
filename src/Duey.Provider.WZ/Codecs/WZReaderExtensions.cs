using Duey.Provider.WZ.Exceptions;

namespace Duey.Provider.WZ.Codecs;

public static class WZReaderExtensions
{
    public static byte[] ApplyXOR(this byte[] input, bool unicode)
    {
        var length = input.Length;
        
        if (unicode)
            if (length % 2 != 0)
                throw new WZPackageException("Input string is not power of two");

        var bytes = new byte[length];
        
        Buffer.BlockCopy(input, 0, bytes, 0, length);
        
        if (unicode)
        {
            ushort mask = 0xAAAA;
            for (var i = 0; i < length; i += 2)
            {
                bytes[i + 0] ^= (byte)(mask & 0xFF);
                bytes[i + 1] ^= (byte)(mask >> 8 & 0xFF);
                mask++;
            }
        }
        else
        {
            byte mask = 0xAA;

            for (var i = 0; i < length; i++)
            {
                bytes[i] ^= mask;
                mask++;
            }
        }

        return bytes;
    }
    
    
    public static int ReadCompressedInt(this WZReader reader)
    {
        var x = reader.ReadSByte();
        return x == -128 ? reader.ReadInt32() : x;
    }
    
    public static long ReadCompressedLong(this WZReader reader)
    {
        var x = reader.ReadSByte();
        return x == -128 ? reader.ReadInt64() : x;
    }
}
