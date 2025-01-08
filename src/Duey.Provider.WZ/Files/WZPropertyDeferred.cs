using System.IO.MemoryMappedFiles;
using Duey.Abstractions;
using Duey.Provider.WZ.Codecs;
using Duey.Provider.WZ.Crypto;

namespace Duey.Provider.WZ.Files;

public abstract class WZPropertyDeferred<T> : WZPropertyFile, IDataProperty<T>
{
    public WZPropertyDeferred(
        MemoryMappedFile view, 
        XORCipher cipher, 
        int start, 
        int offset,
        string name, 
        IDataNode? parent = null
    ) : base(view, cipher, start, offset, name, parent)
    {
    }
    
    public T Resolve()
    {
        do _ = Children.ToList();
        while (!_startDeferred.HasValue);
        
        using var stream = _view.CreateViewStream(_offset, 0, MemoryMappedFileAccess.Read);
        using var reader = new WZReader(stream, _cipher, _startDeferred.Value);

        return Resolve(reader);
    }

    protected abstract T Resolve(WZReader reader);
}
