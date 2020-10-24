using BusinessLogic.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Web.CodeGeneration.EntityFrameworkCore;
using System.Security.Cryptography.X509Certificates;

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
                case ConflictException conflictException:
                    contextResult.StatusCode = conflictException.StatusCode;
                    break;
                case StateException stateException:
                    contextResult.StatusCode = stateException.StatusCode;
                    break;
                case NoReviewerException reviewerException:
                    contextResult.StatusCode = reviewerException.StatusCode;
                    break;
                case RequiredArgumentNullException requiredArgumentNullException:
                    contextResult.StatusCode = requiredArgumentNullException.StatusCode;
                    break;
                default:
                    contextResult.StatusCode = 400;
                    break;
            }

            context.ExceptionHandled = false;            
        }
    }
}
