namespace Duey.Provider.WZ.Crypto;

public class XORCipher
{
    private AESCipher _aes;
    private byte[] _key;

    public XORCipher(IReadOnlyList<byte> iv) : this(new AESCipher(), iv)
    {
    }
    
    public XORCipher(AESCipher aes, IReadOnlyList<byte> iv)
    {
        _aes = aes;
        _key = new[]
        {
            iv[0], iv[1], iv[2], iv[3], 
            iv[0], iv[1], iv[2], iv[3], 
            iv[0], iv[1], iv[2], iv[3], 
            iv[0], iv[1], iv[2], iv[3]
        };
        _key = aes.Transform(_key);
    }

    private void Prepare(int requiredLength)
    {
        if (_key.Length > requiredLength) return;
        
        var newSize = requiredLength / 16 + 1;
        var previousSize = _key.Length;
        
        newSize *= 2;

        Array.Resize(ref _key, newSize);

        for (var offset = previousSize; offset < newSize; offset += 16)
        {
            var result = _aes.Transform(_key[offset..(offset + 16)]);
            result.CopyTo(_key, offset);
        }
    }

    public void Transform(byte[] input)
    {
        var inputLength = input.Length;
        
        Prepare(inputLength);
        
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
