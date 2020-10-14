using ApiModels.Models;
using BusinessLogic.Services.Interfaces;
using DataAccess.Static.Context;
using Domain.EF_Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TimeOffTracker.WebApi.Exceptions;

namespace TimeOffTracker.WebApi.Controllers
{

    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class UsersController : BaseController
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager;
        private readonly IUserService _userService;
        private readonly ILogger<UsersController> _logger;

        public UsersController( UserManager<User> userManager, RoleManager<IdentityRole<int>> roleManager, IUserService userService, ILogger<UsersController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _userService = userService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IEnumerable<UserApiModel>> GetAll()
        {
            return await _userService.GetUsers();
        }

        [HttpGet("{userid:int}")]
        public async Task<IActionResult> GetById(int userId)
        {
            var user = await _userService.GetUser(userId);
            return Ok(user);
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GetByRole([FromQuery(Name = "role")] string roleName)
        {
            var users = await _userService.GetUsersByRole(roleName);
            return Ok(users);
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
