using ApiModels.Models;
using AutoMapper;
using BusinessLogic.Exceptions;
using BusinessLogic.Notifications;
using BusinessLogic.Services.Interfaces;
using DataAccess.Repository.Interfaces;
using DataAccess.Static.Context;
using Domain.EF_Models;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using TimeOffTracker.WebApi.Exceptions;
using TimeOffTracker.WebApi.ViewModels;

namespace BusinessLogic.Services
{
    public class UserService : IUserService
    {
        private readonly IRepository<User, int> _repository;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager;
        IMediator _mediator;
        private readonly IMapper _mapper;

        public UserService(IRepository<User, int> repository, UserManager<User> userManager, RoleManager<IdentityRole<int>> roleManager, IMediator mediator, IMapper mapper)
        {
            _repository = repository;
            _userManager = userManager;
            _roleManager = roleManager;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<UserApiModel> GetUser(int id)
        {
            UserApiModel user = _mapper.Map<UserApiModel>(await _repository.FindAsync(id));

            if (user == null)
                throw new UserNotFoundException($"User not found: UserId={id}");

            return user;
        }

        public async Task<IEnumerable<UserApiModel>> GetUsers(string name = null, string role = null)
        {
            Expression<Func<User, bool>> condition = user =>
                (name == null || (user.FirstName + " " + user.LastName).ToLower().Contains(name.ToLower()))
                && (role == null || user.Role == role);

            IEnumerable<User> users = await _repository.FilterAsync(condition);
            IEnumerable<UserApiModel> models = _mapper.Map<IEnumerable<UserApiModel>>(users);

            return models;
        }

        public async Task<User> CreateUser(RegisterViewModel registerModel)
        {
            using (var transactionScope = new TransactionScope(TransactionScopeOption.Required, TransactionScopeAsyncFlowOption.Enabled))
            {
                User user = _mapper.Map<User>(registerModel);

                var result = await _userManager.CreateAsync(user, registerModel.Password);

                if (!result.Succeeded)
                {
                    StringBuilder sb = new StringBuilder("User not created: ");
                    foreach (IdentityError err in result.Errors)
                        sb.Append(err.Description).Append(";");

                    throw new UserCreateException(sb.ToString());
                }

                await _userManager.AddToRoleAsync(user, registerModel.Role);
                transactionScope.Complete();

                return user;
            }
        }

        public async Task UpdateUser(UserApiModel userModel)
        {
            User user = await _userManager.FindByIdAsync(userModel.Id.ToString());

            if (user == null)
                throw new UserNotFoundException($"User not found: UserId={userModel.Id}");
            if (await _roleManager.FindByNameAsync(userModel.Role) == null)
                throw new RoleNotFoundException($"Role does not exist: {userModel.Role}");
            if (userModel.Email == null)
                throw new ConflictException($"Email can't be empty");

            using (var transactionScope = new TransactionScope(TransactionScopeOption.Required, TransactionScopeAsyncFlowOption.Enabled))
            {
                var userRole = await _userManager.GetRolesAsync(user);

                if (userRole.FirstOrDefault() != userModel.Role)
                {
                    await _userManager.AddToRoleAsync(user, userModel.Role);
                    await _userManager.RemoveFromRolesAsync(user, userRole);
                    if (userRole.First() == RoleName.manager && userModel.Role == RoleName.employee)
                    {
                        var notification = new RoleChangeToEmployeeNotification { User = user };
                        await _mediator.Publish(notification);
                    }
                }

                _mapper.Map(userModel, user);
                await _repository.UpdateAsync(user);

                transactionScope.Complete();
            }
        }

        public async Task DeleteUser(int id)
        {
            User user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
                throw new UserNotFoundException($"User not found: UserId={id}");

            IdentityResult result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                StringBuilder sb = new StringBuilder("User not deleted: ");
                foreach (IdentityError err in result.Errors)
                {
                    sb.Append(err.Description).Append(";");
                }
                throw new UserNotDeleteException(sb.ToString());
            }
        }
    }
}
