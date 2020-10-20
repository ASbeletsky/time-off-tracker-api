using ApiModels.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.Services.Interfaces
{
    public interface ITimeOffRequestReviewService
    {
        Task<TimeOffRequestReviewApiModel> GetByIdAsync(int reviewId);
    }
}
