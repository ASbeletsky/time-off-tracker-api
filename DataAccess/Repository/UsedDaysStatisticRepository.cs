using DataAccess.Context;
using DataAccess.Repository.Interfaces;
using Domain.EF_Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repository
{
    public class UsedDaysStatisticRepository : BaseRepository<UsedDaysStatistic, int>
    {
        public UsedDaysStatisticRepository(TimeOffTrackerContext context) : base(context) { }

        public async override Task<IReadOnlyCollection<UsedDaysStatistic>> FilterAsync(Expression<Func<UsedDaysStatistic, bool>> predicate)
        {
            return await Entities.Where(predicate)
                 .Include(r => r.Request)
                 .ToListAsync();
        }
    }
}
