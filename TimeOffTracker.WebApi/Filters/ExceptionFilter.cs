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
          

            switch (context.Exception.GetType().Name)
            {
                case "ConflictException":
                    contextResult.StatusCode = 409;
                    break;
                case "NoReviewerException":
                    contextResult.StatusCode = 400;
                    break;
                case "RequiredArgumentNullException":
                    contextResult.StatusCode = 400;
                    break;
                default:
                    contextResult.StatusCode = 400;
                    break;
            }


            //context.Result = new ContentResult()
            //{
            //    Content = exceptionMessage,
            //    StatusCode = 400
            //};
            context.ExceptionHandled = false;
            
        }
    }
}
