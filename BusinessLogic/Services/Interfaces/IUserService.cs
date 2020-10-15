using ApiModels.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BusinessLogic.Services.Interfaces
{
    public interface IUserService
    {
        Task<UserApiModel> GetUser(int id);
        Task<IEnumerable<UserApiModel>> GetUsers();
        Task<IEnumerable<UserApiModel>> GetUsersByConditions(string name, string role);
        Task UpdateUser(UserApiModel userModel);
        Task DeleteUser(int id);
    }
}
