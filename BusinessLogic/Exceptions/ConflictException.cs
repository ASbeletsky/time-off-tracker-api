namespace BusinessLogic.Exceptions
{
    public class ConflictException : CustomTimeOffTrackerException
    {
        public ConflictException(string message) : base(message, 409)
        { }
    }
}
