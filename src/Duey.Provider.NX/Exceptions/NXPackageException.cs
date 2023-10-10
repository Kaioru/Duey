using Duey.Abstractions.Exceptions;

namespace Duey.Provider.NX.Exceptions;

public class NXPackageException : DataException
{
    public NXPackageException(string? message = null) : base(message)
    {
    }
}