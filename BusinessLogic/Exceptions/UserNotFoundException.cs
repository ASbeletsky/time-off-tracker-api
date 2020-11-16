namespace BusinessLogic.Exceptions
{
    public class UserNotFoundException : CustomTimeOffTrackerException
    {
        public UserNotFoundException(string message) : base(message, 400)
        { }
    }
}
