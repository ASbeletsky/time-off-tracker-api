namespace BusinessLogic.Exceptions
{
    public class NoReviewerException : CustomTimeOffTrackerException
    {
        public NoReviewerException(string message) : base(message, 400)
        { }
    }
}
