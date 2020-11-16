namespace BusinessLogic.Exceptions
{
    public class StateException : CustomTimeOffTrackerException
    {
        public StateException(string message) : base(message, 409)
        { }
    }
}
