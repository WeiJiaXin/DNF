using System;

namespace Lowy.Bind
{
    public class InjectException : Exception
    {

        public InjectException(string message) : base(message)
        {
        }

        public InjectException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}