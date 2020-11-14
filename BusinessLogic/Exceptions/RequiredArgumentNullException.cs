namespace BusinessLogic.Exceptions
{
    public class RequiredArgumentNullException : CustomTimeOffTrackerException
    {
        public RequiredArgumentNullException(string message) : base(message, 400)
        { }
    }
}
