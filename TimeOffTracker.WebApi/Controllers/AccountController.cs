using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ApiModels.Models;
using AutoMapper;
using DataAccess.Static.Context;
using Domain.EF_Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
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

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IEnumerable<UserApiModel> GetAllUsers()
        {
            return _userManager.Users.ToList()
                .Select(user => _mapper.Map<UserApiModel>(user)).ToList();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<HttpStatusCode> Post([FromForm] RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                User user = new User 
                { 
                    Email = model.Email, UserName = model.Email, 
                    FirstName = model.FirstName, LastName = model.LastName 
                };

                try
                {
                    var result = await _userManager.CreateAsync(user, model.Password);

                    if (result.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(user, model.Role);
                        return HttpStatusCode.OK;
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
                    throw new UserCreateException(ex.Message);
                }
            }
            else
                throw new UserCreateException("Invalid user data");
        }
    }
}
