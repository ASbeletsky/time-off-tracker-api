﻿using ApiModels.Models;
using AutoMapper;
using BusinessLogic.Exceptions;
using BusinessLogic.Services.Interfaces;
using DataAccess.Repository.Interfaces;
using Domain.EF_Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeOffTracker.WebApi.Exceptions;

namespace BusinessLogic.Services
{
    public class UserService : IUserService
    {
        private readonly IRepository<User, int> _repository;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager;
        private readonly IMapper _mapper;

        public UserService(IRepository<User, int> repository, UserManager<User> userManager, RoleManager<IdentityRole<int>> roleManager, IMapper mapper)
        {
            _repository = repository;
            _userManager = userManager;
            _roleManager = roleManager;
            _mapper = mapper;
        }

        public async Task<UserApiModel> GetUser(int id)
        {
            UserApiModel user = _mapper.Map<UserApiModel>(await _repository.FindAsync(id));
            
            if (user == null)
                throw new UserNotFoundException($"Can't find user with Id: {id}");

            return user;
        }

        public async Task<IEnumerable<UserApiModel>> GetUsers() =>
            _mapper.Map<IEnumerable<UserApiModel>>(await _repository.GetAllAsync());

        public async Task<IEnumerable<UserApiModel>> GetUsersByRole(string roleName)
        {
            IEnumerable<User> users = await _repository.FilterAsync(user => user.Role == roleName);
            IEnumerable<UserApiModel> models = _mapper.Map<IEnumerable<UserApiModel>>(users);

            if(!models.Any())
                throw new UserNotFoundException($"Can't find users in role: {roleName}");

            return models;
        }

        public async Task UpdateUser(UserApiModel userModel)
        {
            User user = await _userManager.FindByIdAsync(userModel.Id.ToString());
            
            if (user == null)
                throw new UserNotFoundException($"Can't find user with Id: {userModel.Id}");
            if (await _roleManager.FindByNameAsync(userModel.Role) == null)
                throw new RoleNotFoundException($"Role does not exist: {userModel.Role}");
            try
            {
                var userRole = await _userManager.GetRolesAsync(user);

                if (userRole.FirstOrDefault() != userModel.Role)
                {
                    await _userManager.AddToRoleAsync(user, userModel.Role);
                    await _userManager.RemoveFromRolesAsync(user, userRole);
                }

                _mapper.Map(userModel, user);
            }
            catch (Exception ex)
            {
                throw new UserUpdateException(ex.Message);
            }

            await _repository.UpdateAsync(user);
        }

        public async Task DeleteUser(int id)
        {
            User user = await _userManager.FindByIdAsync(id.ToString());
            if(user == null)
                throw new UserNotFoundException($"Can't find user with Id: {id}");

            IdentityResult result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                StringBuilder sb = new StringBuilder();
                foreach (IdentityError err in result.Errors)
                {
                    sb.Append(err.Description).Append(";");
                }
                throw new UserNotDeleteException(sb.ToString());
            }
        }
    }
}