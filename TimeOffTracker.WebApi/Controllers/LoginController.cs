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
        private readonly IMediator _mediator;
        private ILogger<LoginController> _logger;

        public LoginController(UserTokenService userService, IMediator mediator, ILogger<LoginController> logger)
        {
            _userService = userService;
            _mediator = mediator;
            _logger = logger;
        }

        [HttpPost]
        public IActionResult Login([FromForm]LoginModel model)
        {
            LoggedInUserModel userWithJWT = _userService.Authenticate(model.Username, model.Password);

            if (userWithJWT == null)
                return BadRequest(new { message = "Username or password is incorrect" });

            _logger.LogInformation("Login succes. User: {User}", model.Username);
            
            return Ok(userWithJWT);
        }

        [HttpPost("message")]
        public async Task<IActionResult> Send([FromForm] string message)
        {
            await _mediator.Publish(new TestNotification() { Message = message });

            return Ok();
        }
    }
}
