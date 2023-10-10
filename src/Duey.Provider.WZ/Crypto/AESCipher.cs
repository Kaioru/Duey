using System.Security.Cryptography;

namespace Duey.Provider.WZ.Crypto;

public class AESCipher
{
    private readonly ICryptoTransform _transformer;

    public AESCipher() : this(new byte[] { 0x13, 0x08, 0x06, 0xb4, 0x1b, 0x0f, 0x33, 0x52 })
    {
    }

    public AESCipher(ReadOnlySpan<byte> userKey)
    {
        var expandedKey = new byte[userKey.Length * 4];
        var cipher = Aes.Create();

        for (var i = 0; i < userKey.Length; i++)
            expandedKey[i * 4] = userKey[i];

        cipher.KeySize = 256;
        cipher.Key = expandedKey;
        cipher.Mode = CipherMode.ECB;
        _transformer = cipher.CreateEncryptor();
    }

    public byte[] Transform(byte[] input)
        => _transformer.TransformFinalBlock(input, 0, input.Length);
}
