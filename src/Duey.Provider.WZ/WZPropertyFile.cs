using System.IO.MemoryMappedFiles;
using Duey.Abstractions;
using Duey.Provider.WZ.Codecs;
using Duey.Provider.WZ.Crypto;
using Duey.Provider.WZ.Types;

namespace Duey.Provider.WZ;

public class WZPropertyFile : AbstractWZNode, IDataNode
{
    private readonly MemoryMappedFile _view;
    private readonly XORCipher _cipher;
    private readonly int _start;

    public WZPropertyFile(MemoryMappedFile view, XORCipher cipher, int start, string name, IDataNode? parent = null)
    {
        _view = view;
        _cipher = cipher;
        _start = start;
        Name = name;
        Parent = parent ?? this;
    }

    public override string Name { get; }
    public override IDataNode Parent { get; }

    public override IEnumerable<IDataNode> Children
    {
        get
        {
            using var stream = _view.CreateViewStream(0, 0, MemoryMappedFileAccess.Read);
            using var reader = new WZReader(stream, _cipher, _start);
            
            reader.ReadBoolean();
            reader.ReadByte();

            var count = reader.ReadCompressedInt();
    
            for (var i = 0; i < count; i++)
            {
                var name = reader.ReadStringBlock();
                var type = (WZVariantType)reader.ReadByte();
        
                switch (type)
                {
                    case WZVariantType.EmptyVariant: break;
                    case WZVariantType.Uint8Variant:
                        yield return new WZPropertyData<long>(name, this, reader.ReadByte());
                        break;
                    case WZVariantType.Int8Variant:
                        yield return new WZPropertyData<long>(name, this, reader.ReadSByte());
                        break;
                    case WZVariantType.Uint16Variant:
                        yield return new WZPropertyData<long>(name, this, reader.ReadUInt16());
                        break;
                    case WZVariantType.Int16Variant:
                    case WZVariantType.BoolVariant:
                        yield return new WZPropertyData<long>(name, this, reader.ReadInt16());
                        break;
                    case WZVariantType.Uint32Variant:
                    case WZVariantType.Int32Variant:
                        yield return new WZPropertyData<long>(name, this, reader.ReadCompressedInt());
                        break;
                    case WZVariantType.Float32Variant:
                        yield return new WZPropertyData<double>(name, this, reader.ReadByte() == 0x80 ? reader.ReadSingle() : 0.0);
                        break;
                    case WZVariantType.Float64Variant:
                        yield return new WZPropertyData<double>(name, this, reader.ReadDouble());
                        break;
                    case WZVariantType.BStrVariant:
                        yield return new WZPropertyData<string>(name, this, reader.ReadStringBlock());
                        break;
                    case WZVariantType.DateVariant:
                        yield return new WZPropertyData<long>(name, this, reader.ReadInt64());
                        break;
                    case WZVariantType.CYVariant:
                    case WZVariantType.Int64Variant:
                    case WZVariantType.Uint64Variant:
                        yield return new WZPropertyData<long>(name, this, reader.ReadCompressedLong());
                        break;
                    case WZVariantType.DispatchVariant:
                    case WZVariantType.UnknownVariant:
                    {
                        var size = reader.ReadInt32();
                        var position = reader.BaseStream.Position;
                        
                        switch (reader.ReadStringBlock())
                        {
                            case "Property":
                                yield return new WZPropertyFile(_view, _cipher, (int)reader.BaseStream.Position, name, this);
                                break;
                        }

                        reader.BaseStream.Position = position + size;
                        break;
                    }
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}
