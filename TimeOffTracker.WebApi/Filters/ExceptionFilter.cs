using BusinessLogic.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace TimeOffTracker.WebApi.Filters
{
    public class ExceptionFilter : ExceptionFilterAttribute
    {
        private ILogger<ExceptionFilter> _Logger;

        public ExceptionFilter(ILogger<ExceptionFilter> logger)
        {
            _Logger = logger;
        }

        public override void OnException(ExceptionContext context)
        {
            string actionName = context.ActionDescriptor.DisplayName;
            _Logger.LogError(context.Exception, "Error in method: {Method}", actionName);
           
            var contextResult = new ContentResult();
            contextResult.Content = context.Exception.Message;
          
            switch (context.Exception)
            {
                case CustomTimeOffTrackerException timeOffTrackerException:
                    contextResult.StatusCode = timeOffTrackerException.StatusCode;
                    break;
                default:
                    contextResult.StatusCode = 500;
                    break;
            }

            context.Result = contextResult;
            context.ExceptionHandled = contextResult.StatusCode < 500;            
        }
    }
}
