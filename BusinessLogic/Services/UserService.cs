using ApiModels.Models;
using AutoMapper;
using AutoMapper.Internal;
using DataAccess.Repository;
using DataAccess.Repository.Interfaces;
using Domain.EF_Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BusinessLogic.Services
{
    public class UserService
    {
        UserManager<User> _userManager;
        IMapper _mapper;

        public UserService(UserManager<User> userManager, IMapper mapper)
        {
            _userManager = userManager;
            _mapper = mapper;
        }

        public UserApiModel GetUser(User user)
        {
            UserApiModel userModel = _mapper.Map<UserApiModel>(user);
            SetUserRole(user, userModel);

            return userModel;
        }

        public IEnumerable<UserApiModel> GetUsers()
        {
            var users = _userManager.Users.ToList();
            IEnumerable<UserApiModel> userModels = _mapper.Map<IEnumerable<UserApiModel>>(users);
            
            var usersEnumerator = users.GetEnumerator();
            var modelsEnumerator = userModels.GetEnumerator();
            while (usersEnumerator.MoveNext() && modelsEnumerator.MoveNext())
            {
                SetUserRole(usersEnumerator.Current, modelsEnumerator.Current);
            }

            return userModels;
        }

        private void SetUserRole(User user, UserApiModel userModel)
        {
            userModel.Role = _userManager.GetRolesAsync(user).Result.FirstOrDefault();
        }
    }
}
