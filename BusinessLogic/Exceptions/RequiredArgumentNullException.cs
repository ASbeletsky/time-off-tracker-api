using System;
using System.Collections.Generic;
using System.Text;

namespace BusinessLogic.Exceptions
{
    public class RequiredArgumentNullException:Exception
    {
        public RequiredArgumentNullException(string message) : base(message)
        { }

        public int StatusCode = 400;
    }
}
