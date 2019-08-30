using System;

namespace Duey.Exceptions
{
    public class NXFileException : Exception
    {
        public NXFileException(string message = null) : base(message)
        {
        }
    }
}