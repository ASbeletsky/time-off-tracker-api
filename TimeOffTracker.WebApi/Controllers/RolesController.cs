using System.Collections.Generic;
using System.Linq;
using DataAccess.Static.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace TimeOffTracker.WebApi.Controllers
{
    [ApiController]
    [Route("auth/[controller]")]
    [Authorize(Roles = RoleName.admin)]
    public class RolesController : BaseController
    {
        private readonly RoleManager<IdentityRole<int>> _roleManager;

        public RolesController(RoleManager<IdentityRole<int>> roleManager)
        {
            _roleManager = roleManager;
        }

        [HttpGet]
        public IEnumerable<string> GetAllRoles()
        {
            IEnumerable<string> allRoles = _roleManager.Roles.AsNoTracking()
                   .Select(role => role.Name).Where(roleName => roleName != RoleName.admin)
                   .ToList();

            return allRoles;
        }
    }
}
