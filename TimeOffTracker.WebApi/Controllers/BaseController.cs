using Microsoft.AspNetCore.Mvc;
using TimeOffTracker.WebApi.Filters;

namespace TimeOffTracker.WebApi.Controllers
{
    [ServiceFilter(typeof(ExceptionFilter))]
    public class BaseController : ControllerBase
    {

    }
}