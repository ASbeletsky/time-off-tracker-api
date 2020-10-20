using System;
using System.Text;
using System.Threading.Tasks;
using ApiModels.Models;
using AutoMapper;
using DataAccess.Static.Context;
using Domain.EF_Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Logging;
using TimeOffTracker.WebApi.Exceptions;
using TimeOffTracker.WebApi.ViewModels;

namespace TimeOffTracker.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize(Roles = RoleName.admin)]
    public class AccountsController : BaseController
    {
        private readonly UserManager<User> _userManager;
        private readonly IMapper _mapper;
        private ILogger<AccountsController> _logger;

        public AccountsController(UserManager<User> userManager, IMapper mapper, ILogger<AccountsController> logger)
        {
            _mapper = mapper;
            _userManager = userManager;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                User user = _mapper.Map<User>(model);

                try
                {
                    var result = await _userManager.CreateAsync(user, model.Password);

                    if (result.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(user, model.Role);
                        
                        _logger.LogInformation("Account created successfully:\n{User}, {UserId}", user.UserName, user.Id);
                        return Ok(_mapper.Map<UserApiModel>(user));
                    }

                    StringBuilder sb = new StringBuilder();
                    foreach (IdentityError err in result.Errors)
                    {
                        sb.Append(err.Description).Append(";");
                    }
                    throw new Exception(sb.ToString());

                }
                catch (Exception ex)
                {
                    User exUser = await _userManager.FindByNameAsync(user.UserName);
                    if ( exUser != null)
                        if (!_userManager.GetRolesAsync(exUser).Result.Any())
                            await _userManager.DeleteAsync(user);

                    throw new UserCreateException(ex.Message);
                }
            }
            else
                throw new UserCreateException("Invalid user data");
        }
    }
}
