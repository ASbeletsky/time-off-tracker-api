using System;
using System.Text;
using System.Threading.Tasks;
using ApiModels.Models;
using AutoMapper;
using BusinessLogic.Services.Interfaces;
using DataAccess.Static.Context;
using Domain.EF_Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        private readonly  IUserService _userService;
        private readonly IMapper _mapper;
        private ILogger<AccountsController> _logger;

        public AccountsController(IUserService userService, IMapper mapper, ILogger<AccountsController> logger)
        {
            _mapper = mapper;
            _userService = userService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                User newUser = await _userService.CreateUser(model);
                _logger.LogInformation("Account created successfully( id: {UserId}, username: {User} )", newUser.UserName, newUser.Id);

                return Ok(_mapper.Map<UserApiModel>(newUser));
            }
            else
                throw new UserCreateException("User not created: Invalid user data");
        }
    }
}
