namespace BusinessLogic.Exceptions
{
    public class RequestNotFoundException : CustomTimeOffTrackerException
    {
        public RequestNotFoundException(string message) : base(message, 404)
        { }
    }
}
