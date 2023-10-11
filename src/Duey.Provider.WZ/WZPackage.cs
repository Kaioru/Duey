using System.IO.MemoryMappedFiles;
using Duey.Abstractions;
using Duey.Provider.WZ.Codecs;
using Duey.Provider.WZ.Crypto;
using Duey.Provider.WZ.Exceptions;
using Duey.Provider.WZ.Files;

namespace Duey.Provider.WZ;

public class WZPackage : AbstractWZNode, IDataDirectory
{
    internal readonly MemoryMappedFile View;
    internal readonly XORCipher Cipher;

    internal readonly int Start;
    
    internal readonly uint InternalKey;
    internal readonly byte InternalHash;
    
    public WZPackage(string path, string key, XORCipher? cipher = null, IDataNode? parent = null) : this(
        MemoryMappedFile.CreateFromFile(
            File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read),
            null,
            0,
            MemoryMappedFileAccess.Read,
            HandleInheritability.None,
            false
        ),
        Path.GetFileNameWithoutExtension(path),
        key,
        cipher ?? new XORCipher(WZImageIV.GMS),
        parent
    )
    {
    }
    
    private readonly IDataNode _root;
    
    public WZPackage(MemoryMappedFile view, string name, string key, XORCipher cipher, IDataNode? parent = null)
    {
        View = view;
        Cipher = cipher;

        var stream = view.CreateViewStream(0, 0, MemoryMappedFileAccess.Read);
        var reader = new WZReader(stream, cipher, 0);
        
        if (reader.ReadInt32() != 0x31474B50) throw new WZPackageException("Invalid magic value");

        reader.ReadInt32(); // Size
        reader.ReadInt32(); // Check

        Start = reader.ReadInt32();

        reader.BaseStream.Position = Start;

        var hash = reader.ReadByte();
        var keyNew = (uint)key.Aggregate(0, (current, c) => ' ' * current + c + 1);
        var keyHash = (byte)0xFF;
        
        keyHash ^= (byte)(keyNew >> 24 & 0xFF);
        keyHash ^= (byte)(keyNew >> 16 & 0xFF);
        keyHash ^= (byte)(keyNew >> 8 & 0xFF);
        keyHash ^= (byte)(keyNew >> 0 & 0xFF);

        if (keyHash != hash && keyHash != ~hash) throw new WZPackageException("Hash and key mismatch");

        InternalKey = keyNew;
        InternalHash = keyHash;

        _root = new WZDirectory(this, Start + 2, name, parent ?? this);
    }

    public override string Name => _root.Name;
    public override IDataNode Parent => _root.Parent;

    public override IEnumerable<IDataNode> Children => _root.Children;
}
