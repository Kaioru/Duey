using System.IO.MemoryMappedFiles;
using Duey.Abstractions;
using Duey.Provider.WZ.Codecs;
using Duey.Provider.WZ.Exceptions;
using Duey.Provider.WZ.Files;

namespace Duey.Provider.WZ;

public class WZDirectory : AbstractWZNode, IDataDirectory
{
    private readonly WZPackage _package;
    private readonly int _start;
    
    public WZDirectory(WZPackage package, int start, string name, IDataNode parent)
    {
        _package = package;
        _start = start;
        Name = name;
        Parent = parent;
    }
    
    public override string Name { get; }
    public override IDataNode Parent { get; }

    public override IEnumerable<IDataNode> Children
    {
        get
        {
            using var stream = _package.View.CreateViewStream(0, 0, MemoryMappedFileAccess.Read);
            using var reader = new WZReader(stream, _package.Cipher, _start, _package.Start);
            
            var count = reader.ReadCompressedInt();

            for (var i = 0; i < count; i++)
            {
                var type = reader.ReadByte();
                if (type > 4) throw new WZPackageException("Invalid type while parsing directory");
                var isDir = (type & 1) == 1;
                var name = type <= 2
                    ? reader.ReadStringOffset()
                    : reader.ReadString();

                reader.ReadCompressedInt();
                reader.ReadCompressedInt();

                var offset = reader.ReadOffset(_package.Start, _package.InternalKey);

                switch (type)
                {
                    case 3:
                        yield return new WZDirectory(_package, offset, name, this);
                        break;
                    case 4:
                        yield return new WZImage(_package.View, _package.Cipher, 0, offset, name, this);
                        break;
                }
            }
        }
    }
}
