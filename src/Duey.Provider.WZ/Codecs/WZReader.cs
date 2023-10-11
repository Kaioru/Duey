using System.Text;
using Duey.Provider.WZ.Crypto;
using Duey.Provider.WZ.Exceptions;

namespace Duey.Provider.WZ.Codecs;

public class WZReader : BinaryReader
{
    private readonly XORCipher _cipher;
    private readonly int _start;
    private readonly int _offset;
    
    public WZReader(Stream input, XORCipher cipher, int start, int offset = 0) : base(input)
    {
        input.Position = start;
        _cipher = cipher;
        _start = start;
        _offset = offset;
    }

    public string ReadStringBlock()
    {
        switch (ReadByte())
        {
            case 0:
            case 0x73:
                return ReadString();
            case 1:
            case 0x1B:
                return ReadStringOffset();
            default:
                throw new WZPackageException("Unknown string type when reading string block");
        }
    }
    
    public string ReadStringOffset()
    {
        var offset = ReadInt32() + _offset;
        var position = BaseStream.Position;
        
        BaseStream.Position = offset;
        
        var result = ReadString();
        
        BaseStream.Position = position;

        return result;
    }

    public new string ReadString()
    {
        var length = ReadSByte();
        if (length == 0) return string.Empty;
        return length > 0
            ? ReadStringUnicode(length)
            : ReadStringASCII(length);
    }
    
    private string ReadStringASCII(sbyte length)
    {
        var size = length == -128
            ? ReadInt32()
            : -length;
        var bytes = ReadBytes(size).ApplyXOR(false);
        
        _cipher.Transform(bytes);
        return Encoding.ASCII.GetString(bytes);
    }
    
    private string ReadStringUnicode(sbyte length)
    {
        var size = (length == 127
            ? ReadInt32()
            : length) * 2;
        var bytes = ReadBytes(size).ApplyXOR(true);
        
        _cipher.Transform(bytes);
        return Encoding.Unicode.GetString(bytes);
    }

    public int ReadOffset(int start, uint key)
    {
        var position = BaseStream.Position;
        var encrypted = ReadUInt32();
        var offset = (uint)position;
        
        offset = (uint)(offset - start) ^ 0xFFFFFFFF;
        offset *= key;

        offset -= 0x581C3F6D;
        offset = ROL(offset, (byte)(offset & 0x1F));

        offset ^= encrypted;
        offset += (uint)(start * 2);
        return (int)offset;
    }
    
    private static uint ROL(uint value, byte times) => value << times | value >> (32 - times);
}
