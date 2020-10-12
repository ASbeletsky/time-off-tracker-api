using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ApiModels.Models;
using AutoMapper;
using Domain.EF_Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TimeOffTracker.WebApi.Exceptions;
using TimeOffTracker.WebApi.ViewModels;

namespace TimeOffTracker.WebApi.Controllers
{
    [Route("auth/[controller]")]
    [ApiController]
    public class AccountController : BaseController
    {
        private readonly UserManager<User> _userManager;
        private readonly IMapper _mapper;
        private ILogger<AccountController> _logger;

        public AccountController(UserManager<User> userManager, IMapper mapper, ILogger<AccountController> logger)
        {
            _mapper = mapper;
            _userManager = userManager;
            _logger = logger;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateAccount([FromForm] RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                User user = new User 
                { 
                    Email = model.Email, 
                    UserName = string.Concat(model.Email.TakeWhile(ch => ch != '@')), 
                    FirstName = model.FirstName, LastName = model.LastName 
                };

                try
                {
                    var result = await _userManager.CreateAsync(user, model.Password);

                    if (result.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(user, model.Role);

                        _logger.LogInformation("Account created successfully:\n{User}", user);
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
                    if (_userManager.FindByNameAsync(user.UserName).Result != null)
                        _userManager.DeleteAsync(user).Wait();
                    throw new UserCreateException(ex.Message);
                }
            }
            else
                throw new UserCreateException("Invalid user data");
        }
    }
}
