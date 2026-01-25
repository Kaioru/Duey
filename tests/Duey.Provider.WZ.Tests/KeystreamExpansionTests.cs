using Duey.Provider.WZ.Crypto;
using Duey.Provider.WZ.Files;
using Xunit;

namespace Duey.Provider.WZ.Tests;

public class KeystreamExpansionTests
{
    private static readonly byte[] FullExpectedKeystream = Convert.FromHexString(
        "96AE3FA448FADD904676056197CE7868" +
        "2BA0448FC1567E32FCE1F5B31414C522" +
        "F5C3682E9DC34A0BFAFE6845538AFB5D" +
        "094F59FCE911129BD90FF2E862693B76" +
        "47881075ACE396D7DB1279CD59E4E00C"
    );

    [Theory]
    [InlineData(64)]
    [InlineData(80)]
    public void Expansion_MatchesExpected(int length)
    {
        var cipher = new XORCipher(WZImageIV.GMS);
        var data = new byte[length];

        cipher.Transform(data);

        var expected = FullExpectedKeystream.AsSpan(0, length);

        Assert.True(data.AsSpan().SequenceEqual(expected), $"Keystream mismatch at length {length}");
    }

    [Fact]
    public void ExpansionDoesNotContainPkcs7PaddingBlock()
    {
        var cipher = new XORCipher(WZImageIV.GMS);
        var data = new byte[64];

        cipher.Transform(data);

        ReadOnlySpan<byte> pkcs7Block = Convert.FromHexString("15BEA7702C0A428F9CBC0C2909C85135");

        var index = data.AsSpan().IndexOf(pkcs7Block);

        Assert.True(index == -1, "The expansion should not contain the PKCS7 padding block.");
    }
}
