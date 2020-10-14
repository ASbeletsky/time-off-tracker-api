using System;

namespace TimeOffTracker.WebApi.Exceptions
{
    public class UserNotDeleteException : Exception
    {
        public UserNotDeleteException(string message) : base(message)
        { }
    }
}
