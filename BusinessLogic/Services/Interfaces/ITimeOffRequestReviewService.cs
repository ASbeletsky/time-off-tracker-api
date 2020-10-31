using ApiModels.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.Services.Interfaces
{
    public interface ITimeOffRequestReviewService
    {
        Task<IReadOnlyCollection<TimeOffRequestReviewApiModel>> GetAllAsync(int? reviewerId = null, int? requestId = null, int? stateId = null, DateTime? startDate = null, DateTime? endDate = null, string name = null, int? typeId = null);
        Task<TimeOffRequestReviewApiModel> GetByIdAsync(int reviewId);
        Task CreateAsync(TimeOffRequestReviewApiModel obj);
        Task DeleteAsync(int id);
        Task UpdateAsync(int reviewId, TimeOffRequestReviewApiModel newModel, int userId);
    }
}
