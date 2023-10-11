using Duey.Provider.WZ.Crypto;

namespace Duey.Provider.WZ;

public class FSNamespace : FSDirectory
{
    public FSNamespace(string path, XORCipher? cipher = null) : base(path, cipher)
    {
    }
}