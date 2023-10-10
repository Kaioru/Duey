namespace Duey.Abstractions.Exceptions;

public class DataException : Exception
{
    protected DataException(string? message = null) : base(message)
    {
    }
}