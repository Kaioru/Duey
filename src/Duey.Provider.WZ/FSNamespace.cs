using Duey.Abstractions;
using Duey.Provider.WZ.Crypto;

namespace Duey.Provider.WZ;

public class FSNamespace : FSDirectory, IDataNamespace
{
    public FSNamespace(string path, XORCipher? cipher = null) : base(path, cipher)
    {
    }
    
    public IDictionary<string, IDataNode> Cached { get; }
}