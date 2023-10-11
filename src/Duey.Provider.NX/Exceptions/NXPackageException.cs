using Duey.Abstractions.Exceptions;

namespace Duey.Provider.NX.Exceptions;

public class NXPackageException : DataException
{
    internal NXPackageException(string? message = null) : base(message)
    {
    }
}