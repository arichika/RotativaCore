using System;
using System.Collections.Generic;
using System.Text;

namespace RotativaCore
{
    public class WkhtmlDriverStandardErrorException : WkhtmlDriverException
    {
        public WkhtmlDriverStandardErrorException(string message) : base(message: message)
        { }
        public WkhtmlDriverStandardErrorException(string message, Exception innerException) : base(message: message, innerException: innerException)
        { }
    }
}
