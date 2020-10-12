using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiModels.Models;
using AutoMapper;
using Domain.EF_Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace TimeOffTracker.WebApi.Controllers
{
    [ApiController]
    [Route("auth/[controller]")]
    [Authorize(Roles = "Admin")]
    public class RoleController : BaseController
    {
        private readonly RoleManager<IdentityRole> _roleManager;

        public RoleController(RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
        }

        [HttpGet]
        public IEnumerable<string> GetAllRoles()
        {
            IEnumerable<string> allRoles = _roleManager.Roles.AsNoTracking()
                   .Select(role => role.Name).Where(roleName => roleName != "ADMIN")
                   .ToList();

            return allRoles;
        }
    }
}
