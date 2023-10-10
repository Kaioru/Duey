using Duey.Abstractions.Exceptions;

namespace Duey.Provider.WZ.Exceptions;

public class WZPackageException : DataException
{
    internal WZPackageException(string? message = null) : base(message)
    {
    }
}
