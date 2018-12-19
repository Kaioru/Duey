using System;

namespace Duey.NX.Exceptions
{
    public class NXFileException : Exception
    {
        public NXFileException(string message = null) : base(message)
        {
        }
    }
}