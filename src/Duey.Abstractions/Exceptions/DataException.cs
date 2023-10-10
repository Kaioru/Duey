namespace Duey.Abstractions.Exceptions;

public class DataException : Exception
{
    public DataException(string? message = null) : base(message)
    {
    }
}