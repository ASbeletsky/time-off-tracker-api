using DataAccess.Static.Context;
using Domain.EF_Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Context
{
    public class IdentityInitializer
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager;

        public IdentityInitializer(UserManager<User> userManager, RoleManager<IdentityRole<int>> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }
        public async Task SeedAsync()
        {
            await _roleManager.CreateAsync(new IdentityRole<int>(RoleName.manager));
            await _roleManager.CreateAsync(new IdentityRole<int>(RoleName.accountant));
            await _roleManager.CreateAsync(new IdentityRole<int>(RoleName.employee));

            var login = "mainadmin@mail.ru";
            var pass = "mainadmin89M#";

            if ((await _userManager.FindByEmailAsync(login)) == null)
            {
                var user = new User() { UserName = login, Email = login };
                var saveuser = await _userManager.CreateAsync(user, pass);

                if (saveuser.Succeeded)
                {
                    if ((await _roleManager.FindByNameAsync(RoleName.admin)) == null)
                    {
                        var saverole = await _roleManager.CreateAsync(new IdentityRole<int>(RoleName.admin));

                        if (saverole.Succeeded)
                        {
                            await _userManager.AddToRoleAsync(user, RoleName.admin);
                        }
                    }
                }

            }
        }
    }
}
