using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ApiModels.Models;
using AutoMapper;
using BusinessLogic.Services;
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
    [Authorize(Roles = "Admin")]
    public class AccountController : BaseController
    {
        private readonly UserManager<User> _userManager;
        private readonly IMapper _mapper;
        private readonly UserService _userService;
        private ILogger<AccountController> _logger;

        public AccountController(UserManager<User> userManager, IMapper mapper, UserService userService, ILogger<AccountController> logger)
        {
            _mapper = mapper;
            _userManager = userManager;
            _userService = userService;
            _logger = logger;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IEnumerable<UserApiModel>> GetAllUsers()
        {
            return await _userService.GetUsers();
        }

        [HttpPost]
        public async Task<HttpStatusCode> Post([FromForm] RegisterViewModel model)
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
