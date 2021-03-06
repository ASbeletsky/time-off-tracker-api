﻿using DataAccess.Static.Context;
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
            await CreateDefaultAccount("Accounting", "AccTOT2020@gmail.com", "Accounting@1", "Accounting", RoleName.accountant);
            await CreateDefaultAccount("Admin", "OdminTOT2020@gmail.com", "Admin@1", "Administrator", RoleName.admin);
           
            await _roleManager.CreateAsync(new IdentityRole<int>(RoleName.manager));
            await _roleManager.CreateAsync(new IdentityRole<int>(RoleName.employee));
        }

        private async Task CreateDefaultAccount(string login, string email, string password, string name, string role) 
        {

            if ((await _userManager.FindByNameAsync(login)) == null)
            {
                var user = new User() { UserName = login, Email = email, FirstName = name, LastName = "" };
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
