using System;
using System.Collections.Generic;
using System.Text;

namespace BusinessLogic.Exceptions
{
    public class ConflictException : Exception
    {
        public ConflictException(string message) : base(message)
        { }

        public int StatusCode = 409;
    }
}
