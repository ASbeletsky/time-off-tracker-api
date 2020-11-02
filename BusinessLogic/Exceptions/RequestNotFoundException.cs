using System;

namespace BusinessLogic.Exceptions
{
    public class RequestNotFoundException : Exception
    {
        public RequestNotFoundException(string message) : base(message)
        { }

        public int StatusCode = 400;
    }
}
