using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiModels.Models;
using AutoMapper;
using BusinessLogic.Services;
using Domain.EF_Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TimeOffTracker.WebApi.ViewModels;

namespace TimeOffTracker.WebApi.Controllers
{
    [ApiController]
    [Route("auth/[controller]")]
    [Authorize(Roles = "Admin")]
    public class RoleController : BaseController
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserService _userService;
        private readonly IMapper _mapper;
        private ILogger<RoleController> _logger;

        public RoleController(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, UserService userService, IMapper mapper, ILogger<RoleController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _userService = userService;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpGet]
        public IEnumerable<string> GetAllRoles()
        {
            IEnumerable<string> allRoles = _roleManager.Roles.AsNoTracking()
                   .Select(role => role.Name).Where(roleName => roleName != "ADMIN")
                   .ToList();

            return allRoles;
        }

        [HttpPost]
        public async Task<ActionResult<UserApiModel>> SetUserRole([FromForm] RoleChangeModel model)
        {
            User user = await _userManager.FindByIdAsync(model.UserId);

            if (user == null)
                throw new RoleChangeException($"Cannot find user with Id: {model.UserId}");
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

            UserApiModel userModel = await _userService.GetUser(user);
            return Ok(userModel);
        }
    }
}
