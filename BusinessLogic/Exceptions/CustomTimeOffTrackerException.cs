using System;

namespace BusinessLogic.Exceptions
{
    public class CustomTimeOffTrackerException : Exception
    {
        public int StatusCode { get; set; }
        public CustomTimeOffTrackerException(string message, int statusCode) : base(message)
        {
            StatusCode = statusCode;
        }
    }
}
