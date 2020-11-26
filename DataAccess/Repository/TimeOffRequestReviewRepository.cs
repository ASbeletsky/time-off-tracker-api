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
    public class TimeOffRequestReviewRepository : BaseRepository<TimeOffRequestReview, int>
    {
        public TimeOffRequestReviewRepository(TimeOffTrackerContext context) : base(context) { }

        public override async Task<IReadOnlyCollection<TimeOffRequestReview>> FilterAsync(Expression<Func<TimeOffRequestReview, bool>> predicate)
        {
            return await Entities.Where(predicate)
                 .Include(r => r.Request)
                 .ThenInclude(u=>u.Reviews)
                 .ToListAsync();
        }

        public override async Task<TimeOffRequestReview> FindAsync(Expression<Func<TimeOffRequestReview, bool>> predicate)
        {
            return await Entities.Where(predicate)
                 .Include(r => r.Request)
                 .ThenInclude(u => u.Reviews)
                 .FirstOrDefaultAsync();
        }                
    }
}
