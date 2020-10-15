using System;

namespace BusinessLogic.Exceptions
{
    public class RoleNotFoundException : Exception
    {
        public RoleNotFoundException(string message) : base(message)
        { }
    }
}
