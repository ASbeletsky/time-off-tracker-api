using ApiModels.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.Services.Interfaces
{
    public interface ITimeOffRequestService<T> where T: class
    {
        Task AddAsync(T obj, int userId);
        Task<IReadOnlyCollection<T>> GetAllAsync(int userId, DateTime start, DateTime end, int stateId, int typeId);
        Task<T> GetByIdAsync(int userId, int requestId);
        Task UpdateAsync(int userId, T newModel);
        Task DeleteAsync(int userId, int requestId);
        Task RejectedAsync(int userId, int requestId);
    }
}
