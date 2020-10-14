using System;

namespace TimeOffTracker.WebApi.Exceptions
{
    public class UserNotFoundException : Exception
    {
        public UserNotFoundException(string message) : base(message)
        { }
    }
}
