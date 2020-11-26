namespace BusinessLogic.Exceptions
{
    public class UserUpdateException : CustomTimeOffTrackerException
    {
        public UserUpdateException(string message) : base(message, 400)
        { }
    }
}

