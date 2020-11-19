using ApiModels.Models;
using Domain.EF_Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.Services.Interfaces
{
    public interface IStatisticService
    {
        Task AddAsync(UsedDaysStatistic obj);
        Task UpdateAsync(int statisticId, UsedDaysStatistic obj);
        Task<IEnumerable<UsedDaysStatisticApiModel>> GetStatisticByUserAsync(int userId);
    }
}
