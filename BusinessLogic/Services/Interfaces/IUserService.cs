using ApiModels.Models;
using Domain.EF_Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using TimeOffTracker.WebApi.ViewModels;

namespace BusinessLogic.Services.Interfaces
{
    public interface IUserService
    {
        Task<UserApiModel> GetUser(int id);
        Task<IEnumerable<UserApiModel>> GetUsers(string name = null, string role = null);
        Task<User> CreateUser(RegisterViewModel registerModel);
        Task UpdateUser(UserApiModel userModel);
        Task DeleteUser(int id);
    }
}
