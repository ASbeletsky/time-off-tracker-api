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
    public class TimeOffRequestRepository : BaseRepository<TimeOffRequest, int>
    {
        public TimeOffRequestRepository(TimeOffTrackerContext context) : base(context) { }

        public async override Task<TimeOffRequest> FindAsync(Expression<Func<TimeOffRequest, bool>> predicate)
        {
            return await Entities.Where(predicate)
                 .Include(r => r.Reviews)
                 .FirstOrDefaultAsync();
        }

        public async override Task<IReadOnlyCollection<TimeOffRequest>> FilterAsync(Expression<Func<TimeOffRequest, bool>> predicate)
        {
            return await Entities.Where(predicate)
                 .Include(r => r.Reviews)
                 .ToListAsync();
        }
    }
}
