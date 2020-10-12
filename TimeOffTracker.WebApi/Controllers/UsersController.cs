using ApiModels.Models;
using AutoMapper;
using BusinessLogic.Services;
using Domain.EF_Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
        private readonly IMapper _mapper;
        private readonly ILogger<UsersController> _logger;

        public UsersController( UserManager<User> userManager, RoleManager<IdentityRole> roleManager, UserService userService,
                                IMapper mapper, ILogger<UsersController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _userService = userService;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpGet]
        [Authorize]
        public IEnumerable<UserApiModel> GetAll()
        {
            return _userManager.Users.ToList()
                .Select(user => _mapper.Map<UserApiModel>(user)).ToList();
        }

        [HttpGet("[action]")]
        [Authorize]
        public async Task<IActionResult> GetById([FromQuery(Name = "id")]string userId)
        {
            User user = await _userManager.FindByIdAsync(userId);
            if (user != null)
                return Ok(_userService.GetUser(user));
            else
                throw new UserNotFoundException($"Can't find user with Id: {userId}");
        }

        [HttpGet("[action]")]
        [Authorize]
        public async Task<IActionResult> GetByRole([FromQuery(Name = "role")] string roleName)
        {
            var users = await _userManager.GetUsersInRoleAsync(roleName);

            if (users != null)
                return Ok(users.Select(user => _mapper.Map<UserApiModel>(user)).ToList());
            else
                throw new UserNotFoundException($"Can't find users in role: {roleName}");
        }

        [HttpPut]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<UserApiModel>> SetUserRole([FromForm] RoleChangeModel model)
        {
            User user = await _userManager.FindByIdAsync(model.UserId);

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

            UserApiModel userModel = await _userService.GetUser(user);
            return Ok(await _userService.GetUser(user));
        }

        [HttpDelete]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser([FromForm(Name = "id")] string userId)
        {
            User user = await _userManager.FindByIdAsync(userId);
            IdentityResult result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                _logger.LogInformation("User {User} deleted successfully",
                user.UserName);
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
