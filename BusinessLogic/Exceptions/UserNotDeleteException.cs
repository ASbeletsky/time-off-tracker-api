namespace BusinessLogic.Exceptions
{
    public class UserNotDeleteException : CustomTimeOffTrackerException
    {
        public UserNotDeleteException(string message) : base(message, 400)
        { }
    }
}
