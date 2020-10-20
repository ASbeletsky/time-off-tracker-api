using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TimeOffTracker.WebApi.ViewModels;
using TimeOffTracker.WebApi.Services;
using Microsoft.Extensions.Logging;
using MediatR;
using BusinessLogic.Notifications;
using System.Threading.Tasks;

namespace TimeOffTracker.WebApi.Controllers
{
    [ApiController]
    [AllowAnonymous]
    [Route("auth/token")]
    public class LoginController : BaseController
    {
        private readonly UserTokenService _userService;
        private ILogger<LoginController> _logger;

        public LoginController(UserTokenService userService, ILogger<LoginController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        [HttpPost]
        public IActionResult Login([FromBody]LoginModel model)
        {
            LoggedInUserModel userWithJWT = _userService.Authenticate(model.Username, model.Password);

            if (userWithJWT == null)
                return BadRequest(new { message = "Username or password is incorrect" });

            _logger.LogInformation("Login succes. User: {User}", model.Username);
            
            return Ok(userWithJWT);
        }
    }
}
