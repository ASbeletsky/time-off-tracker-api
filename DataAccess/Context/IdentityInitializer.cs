using DataAccess.Static.Context;
using Domain.EF_Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
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
            await CreateDefaultAccount("accounting@mail.ru", "accounting89M#", RoleName.accountant);
            await CreateDefaultAccount("mainadmin@mail.ru", "mainadmin89M#", RoleName.admin);
           
            await _roleManager.CreateAsync(new IdentityRole<int>(RoleName.manager));
            await _roleManager.CreateAsync(new IdentityRole<int>(RoleName.employee));
        }

        private async Task CreateDefaultAccount(string login, string password, string role) 
        {

            if ((await _userManager.FindByEmailAsync(login)) == null)
            {
                var user = new User() { UserName = login, Email = login };
                var saveuser = await _userManager.CreateAsync(user, password);

                if (saveuser.Succeeded)
                {
                    if ((await _roleManager.FindByNameAsync(role)) == null)
                    {
                        var saverole = await _roleManager.CreateAsync(new IdentityRole<int>(role));

                        if (saverole.Succeeded)
                        {
                            await _userManager.AddToRoleAsync(user, role);
                        }
                    }
                }

            }
        }
    }
}
