using ApiModels.Models;
using BusinessLogic.Services;
using Domain.EF_Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeOffTracker.WebApi.Exceptions;
using TimeOffTracker.WebApi.ViewModels;

namespace TimeOffTracker.WebApi.Controllers
{

    [ApiController]
    [Route("auth/[controller]")]
    public class UsersController : BaseController
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserService _userService;
        private readonly ILogger<UsersController> _logger;

        public UsersController( UserManager<User> userManager, RoleManager<IdentityRole> roleManager, UserService userService, ILogger<UsersController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _userService = userService;
            _logger = logger;
        }

        [HttpGet]
        [Authorize]
        public async Task<IEnumerable<UserApiModel>> GetAll()
        {
            return await _userService.GetUsers();
        }

        [HttpGet("[action]")]
        [Authorize]
        public async Task<IActionResult> GetById([FromQuery(Name = "id")] int userId)
        {
            UserApiModel userModel = await _userService.GetUser(userId);
            if (userModel != null)
                return Ok(userModel);
            else
                throw new UserNotFoundException($"Can't find user with Id: {userId}");
        }

        [HttpGet("[action]")]
        [Authorize]
        public async Task<IActionResult> GetByRole([FromQuery(Name = "role")] string roleName)
        {
            var users = await _userService.GetUsersByRole(roleName);

            if (users.Count() != 0)
                return Ok(users);
            else
                throw new UserNotFoundException($"Can't find users in role: {roleName}");
        }

        [HttpPut]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<UserApiModel>> SetUserRole([FromForm] RoleChangeModel model)
        {
            User user = await _userManager.FindByIdAsync(model.UserId.ToString());

            if (user == null)
                throw new UserNotFoundException($"Can't find user with Id: {model.UserId}");
            if (_roleManager.FindByNameAsync(model.Role).Result == null)
                throw new RoleChangeException($"Role does not exist: {model.Role}");
            try
            {
                var userRole = await _userManager.GetRolesAsync(user);

                if (userRole.FirstOrDefault() != model.Role)
                {
                    await _userManager.AddToRoleAsync(user, model.Role);
                    await _userManager.RemoveFromRolesAsync(user, userRole);
                }
            }
            catch (Exception ex)
            {
                throw new RoleChangeException(ex.Message);
            }

            _logger.LogInformation("Successful role change for user {User} with id {userId} to {toRole}",
                user.UserName, model.UserId, model.Role);

            UserApiModel userModel = await _userService.GetUser(user.Id);
            return Ok(userModel);
        }

        [HttpDelete]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser([FromForm(Name = "id")] string userId)
        {
            User user = await _userManager.FindByIdAsync(userId);
            IdentityResult result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                _logger.LogInformation("User {User} deleted successfully, id: {userId}", user.UserName, user.Id);
                return NoContent();
            }
            else
            {
                StringBuilder sb = new StringBuilder();
                foreach (IdentityError err in result.Errors)
                {
                    sb.Append(err.Description).Append(";");
                }
                throw new UserNotDeleteException(sb.ToString());
            }
        }
    }
}
