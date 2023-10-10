using System.Security.Cryptography;

namespace Duey.Provider.WZ.Crypto;

public class AESCipher
{
    private readonly ICryptoTransform _transformer;

    public AESCipher() : this(WZFileKey.Default)
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
