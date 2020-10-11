using ApiModels.Models;
using Domain.EF_Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using TimeOffTracker.WebApi.ViewModels;

namespace TimeOffTracker.WebApi.Controllers
{

    [ApiController]
    [Route("auth/[controller]")]
    public class UsersController : BaseController
    {
        private readonly UserManager<User> _userManager;
        private ILogger<UsersController> _logger;

        public UsersController(UserManager<User> userManager, ILogger<UsersController> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        [HttpGet]
        [Authorize]
        public UserApiModel GetUser(int userId)
        {
            throw new NotImplementedException();
        }

        [HttpPut]
        [Authorize(Roles = "Admin")]
        public UserApiModel ChangeUserRole([FromForm] RoleChangeModel model)
        {
            throw new NotImplementedException();
        }

        [HttpDelete]
        [Authorize(Roles = "Admin")]
        public IActionResult DeleteUser([FromForm] int userId)
        {
            throw new NotImplementedException();
        }
    }
}
