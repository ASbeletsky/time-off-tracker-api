using System;
using System.Collections.Generic;
using System.Text;

namespace BusinessLogic.Exceptions
{
    public class StateException:Exception
    {
        public StateException(string message) : base(message)
        { }

        public int StatusCode = 409;
    }
}
