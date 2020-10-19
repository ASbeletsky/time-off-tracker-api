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
            var reviews = Entities.Include(review => review.Reviewer);

            return await reviews.Where(predicate).AsNoTracking().ToListAsync();
        }
    }
}
