using ApiModels.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.Services.Interfaces
{
    public interface ITimeOffRequestService
    {
        Task AddAsync(TimeOffRequestApiModel obj, int userId);
        Task<IReadOnlyCollection<TimeOffRequestApiModel>> GetAllAsync(int userId, string start, string end, int stateId, int typeId);
        Task<TimeOffRequestApiModel> GetByIdAsync(int requestId);
        Task<TimeOffRequestApiModel> GetByIdAsync(int userId, int requestId);
        Task UpdateAsync(int userId, TimeOffRequestApiModel newModel);
        Task DeleteAsync(int requestId);
        Task RejectedAsync(int userId, int requestId);
      
    }
}
