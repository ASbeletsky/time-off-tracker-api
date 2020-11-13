using System;

namespace TimeOffTracker.WebApi.Exceptions
{
    public class UserCreateException : Exception
    {
        public UserCreateException(string message) : base(message)
        { }
    }
}
