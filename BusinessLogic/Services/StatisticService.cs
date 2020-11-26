using ApiModels.Models;
using BusinessLogic.Services.Interfaces;
using DataAccess.Repository.Interfaces;
using Domain.EF_Models;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.Services
{
    public class StatisticService : IStatisticService
    {
        IRepository<UsedDaysStatistic, int> _repository;
        IMediator _mediator;

        public StatisticService(IRepository<UsedDaysStatistic, int> repository, IMediator mediator)
        {
            _repository = repository;
            _mediator = mediator;
        }

        public async Task AddAsync(UsedDaysStatistic obj)
        {
            await _repository.CreateAsync(obj);
        }

        public async Task<IEnumerable<UsedDaysStatisticApiModel>> GetStatisticByUserAsync(int userId)
        {
            var currYear = DateTime.Now.Year;
            return (await _repository.FilterAsync(x => x.UserId == userId && x.Year == currYear.ToString()))
                .Select(obj => new { type = (int)obj.Type, days = obj.NumberDaysUsed })
                .GroupBy(x => x.type).Select(result => new UsedDaysStatisticApiModel { TypeId = result.Key, Days = result.Sum(x => x.days) });
        }

        public async Task UpdateAsync(int statisticId, UsedDaysStatistic obj)
        {
            var res = await _repository.FindAsync(statisticId);
            res.NumberDaysUsed = obj.NumberDaysUsed;
            await _repository.UpdateAsync(res);
        }
    }
}
