using System;
using System.Collections.Generic;
using System.Text;

namespace BusinessLogic.Exceptions
{
    public class NoReviewerException:Exception
    {
        public NoReviewerException(string message) : base(message)
        { }
        
        public int StatusCode = 400;
    }
}
