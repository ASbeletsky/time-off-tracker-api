﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TimeOffTracker.WebApi.ViewModels;
using TimeOffTracker.WebApi.Services;
using Microsoft.Extensions.Logging;

namespace TimeOffTracker.WebApi.Controllers
{
    [ApiController]
    [Route("auth/[controller]")]
    public class LoginController : BaseController
    {
        private readonly UserService _userService;
        private ILogger<LoginController> _logger;

        public LoginController(UserService userService, ILogger<LoginController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult Login([FromForm]AuthenticateModel model)
        {
            string token = _userService.Authenticate(model.Username, model.Password);

            if (token == null)
                return BadRequest(new { message = "Username or password is incorrect" });

            _logger.LogInformation("Login succes. User: {User}", model.Username);

            return Ok(token);
        }
    }
}