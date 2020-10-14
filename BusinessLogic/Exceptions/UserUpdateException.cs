using System;
using System.Collections.Generic;
using System.Text;

namespace BusinessLogic.Exceptions
{
    public class UserUpdateException : Exception
    {
        public UserUpdateException(string message) : base(message)
        { }
    }
}

