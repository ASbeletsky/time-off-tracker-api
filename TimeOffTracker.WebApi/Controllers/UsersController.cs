using ApiModels.Models;
using BusinessLogic.Services.Interfaces;
using DataAccess.Static.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TimeOffTracker.WebApi.Controllers
{

    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class UsersController : BaseController
    {
        private readonly IUserService _userService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IUserService userService, ILogger<UsersController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IEnumerable<UserApiModel>> GetAll([FromQuery] string name = null, [FromQuery] string role = null)
        {
            var users = await _userService.GetUsers(name, role);

            return users;
        }

        [HttpGet("{userid:int}")]
        public async Task<IActionResult> GetById(int userId)
        {
            var user = await _userService.GetUser(userId);
            return Ok(user);
        }

        [HttpPut]
        [Authorize(Roles = RoleName.admin)]
        public async Task<IActionResult> UpdateUser([FromBody] UserApiModel user)
        {
            await _userService.UpdateUser(user);

            _logger.LogInformation("User (id: {User}) updated successfully", user.Id);

            return Ok();
        }

        [HttpDelete("{userid:int}")]
        [Authorize(Roles = RoleName.admin)]
        public async Task<IActionResult> DeleteUser(int userId)
        {
            await _userService.DeleteUser(userId);

            _logger.LogInformation("User deleted successfully, id: {userId}", userId);

            return NoContent();
        }
    }
}
