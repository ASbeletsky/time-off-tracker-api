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

            string exceptionMessage = context.Exception.Message;
            context.Result = new ContentResult()
            {
                Content = exceptionMessage,
                StatusCode = 400
            };
            context.ExceptionHandled = true;
        }
    }
}
