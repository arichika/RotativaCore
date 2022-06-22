using System;

namespace RotativaCore
{
    public class WkhtmlDriverException : Exception
    {
        public WkhtmlDriverException(string message) : base(message: message)
        { }
        public WkhtmlDriverException(string message, Exception innerException) : base(message: message, innerException: innerException)
        { }
    }
}
