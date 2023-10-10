namespace Duey.Provider.WZ.Crypto;

public class XORCipher
{
    private readonly byte[] _key;
    
    public XORCipher(AESCipher aes, IReadOnlyList<byte> iv)
    {
        _key = new[]
        {
            iv[0], iv[1], iv[2], iv[3], 
            iv[0], iv[1], iv[2], iv[3], 
            iv[0], iv[1], iv[2], iv[3], 
            iv[0], iv[1], iv[2], iv[3]
        };
        _key = aes.Transform(_key);
    }

    public void Transform(byte[] input)
    {
        var inputLength = input.Length;
        
        const int bigChunkSize = sizeof(ulong);

        unsafe
        {
            fixed (byte* dataPtr = input)
            fixed (byte* xorPtr = _key)
            {
                var currentInputByte = dataPtr;
                var currentXorByte = xorPtr;

                var i = 0;

                var intBlocks = inputLength / bigChunkSize;
                for (; i < intBlocks; ++i)
                {
                    *(ulong*) currentInputByte ^= *(ulong*) currentXorByte;
                    currentInputByte += bigChunkSize;
                    currentXorByte += bigChunkSize;
                }

                i *= bigChunkSize;

                for (; i < inputLength; i++)
                {
                    *currentInputByte++ ^= *currentXorByte++;
                }
            }
        }
    }
}
