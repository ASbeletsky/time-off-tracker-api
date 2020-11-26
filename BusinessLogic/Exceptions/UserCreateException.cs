using BusinessLogic.Exceptions;

namespace TimeOffTracker.WebApi.Exceptions
{
    public class UserCreateException : CustomTimeOffTrackerException
    {
        public UserCreateException(string message) : base(message, 400)
        { }
    }
}
