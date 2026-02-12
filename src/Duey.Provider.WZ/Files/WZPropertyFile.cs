using System.IO.MemoryMappedFiles;
using Duey.Abstractions;
using Duey.Abstractions.Types;
using Duey.Provider.WZ.Codecs;
using Duey.Provider.WZ.Crypto;
using Duey.Provider.WZ.Files.Extended;
using Duey.Provider.WZ.Types;

namespace Duey.Provider.WZ.Files;

public class WZPropertyFile : AbstractWZNode, IDataNode
{
    private static readonly Guid GUIDAudioFormatWav = new("05589f81-c356-11ce-bf01-00aa0055595a");
    private static readonly Guid GUIDAudioFormatNone = new("00000000-0000-0000-0000-000000000000");

    protected readonly MemoryMappedFile _view;
    protected readonly XORCipher _cipher;
    protected readonly int _start;
    protected readonly int _offset;
    
    protected int? _startDeferred;

    public WZPropertyFile(MemoryMappedFile view, XORCipher cipher, int start, int offset, string name, IDataNode? parent = null)
    {
        _view = view;
        _cipher = cipher;
        _start = start;
        _offset = offset;
        Name = name;
        Parent = parent ?? this;
    }

    public override string Name { get; }
    public override IDataNode Parent { get; }

    public override IEnumerable<IDataNode> Children
    {
        get
        {
            using var stream = _view.CreateViewStream(_offset, 0, MemoryMappedFileAccess.Read);
            using var reader = new WZReader(stream, _cipher, _start);
            
            reader.ReadBoolean();
            if (reader.ReadBoolean())
                reader.BaseStream.Position += 2;

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
                        yield return new WZPropertyData<long>(name, this, reader.ReadInt16());
                        break;
                    case WZVariantType.BoolVariant:
                        yield return new WZPropertyData<long>(name, this, reader.ReadByte());
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
                        yield return new WZPropertyData<long>(name, this, reader.ReadInt64());
                        break;
                    case WZVariantType.DispatchVariant:
                    case WZVariantType.UnknownVariant:
                    {
                        var size = reader.ReadInt32();
                        var position = reader.BaseStream.Position;
                        
                        switch (reader.ReadStringBlock())
                        {
                            case "Property":
                                yield return new WZPropertyFile(_view, _cipher, (int)reader.BaseStream.Position, _offset, name, this);
                                break;
                            case "Canvas":
                                yield return new WZPropertyCanvas(_view, _cipher, (int)reader.BaseStream.Position, _offset, name, this);
                                break;
                            case "Shape2D#Vector2D":
                                yield return new WZPropertyData<DataVector>(name, this, new DataVector(
                                    reader.ReadCompressedInt(), 
                                    reader.ReadCompressedInt()
                                ));
                                break;
                            case "Sound_DX8":
                                reader.ReadByte();

                                var length = reader.ReadCompressedInt();
                                var duration = reader.ReadCompressedInt();

                                reader.BaseStream.Position += 1 + 16 + 16 + 2;

                                var format = new Guid(reader.ReadBytes(16));

                                if (format == GUIDAudioFormatWav)
                                    reader.BaseStream.Position += reader.ReadCompressedInt();
                                
                                yield return new WZPropertyData<DataAudio>(name, this, new DataAudio(
                                    reader.ReadBytes(length)
                                ));
                                break;
                        }

                        reader.BaseStream.Position = position + size;
                        break;
                    }
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            _startDeferred = (int)reader.BaseStream.Position;
        }
    }
}
