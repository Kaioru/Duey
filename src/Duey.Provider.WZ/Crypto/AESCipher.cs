using Duey.Provider.WZ.Files;
using System.Security.Cryptography;

namespace Duey.Provider.WZ.Crypto;

public class AESCipher
{
    private const int DefaultKeySize = 256;
    private const int DefaultBlockSize = 16;
    private readonly ICryptoTransform _transformer;

    public AESCipher() : this(WZImageKey.Default)
    {
    }

    public AESCipher(ReadOnlySpan<byte> userKey)
    {
        var expandedKey = new byte[userKey.Length * 4];
        var cipher = Aes.Create() ?? throw new InvalidOperationException("AES provider not available");

        for (var i = 0; i < userKey.Length; i++)
            expandedKey[i * 4] = userKey[i];

        cipher.KeySize = DefaultKeySize;
        cipher.Key = expandedKey;
        cipher.Mode = CipherMode.ECB;
        cipher.Padding = PaddingMode.None;
        _transformer = cipher.CreateEncryptor();
    }

    public byte[] Transform(byte[] input)
    {
        if (input.Length % DefaultBlockSize != 0)
            throw new ArgumentException($"Input length must be a multiple of {DefaultBlockSize} bytes", nameof(input));

        var output = new byte[input.Length];
        for (var offset = 0; offset < input.Length; offset += DefaultBlockSize)
            _transformer.TransformBlock(input, offset, DefaultBlockSize, output, offset);

        return output;
    }
}
