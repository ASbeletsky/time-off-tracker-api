using ApiModels.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BusinessLogic.Services.Interfaces
{
    public interface IUserService
    {
        Task<UserApiModel> GetUser(int id);
        Task<IEnumerable<UserApiModel>> GetUsers();
        Task<IEnumerable<UserApiModel>> GetUsersByRole(string roleName);
        Task UpdateUser(UserApiModel userModel);
        Task DeleteUser(int id);
    }
}
