using DataAccess.Context;
using DataAccess.Repository.Interfaces;
using Domain.EF_Models;

namespace DataAccess.Repository
{
    public class TimeOffRequestReviewRepository : BaseRepository<TimeOffRequestReview, int>
    {
        public TimeOffRequestReviewRepository(TimeOffTrackerContext context) : base(context) { }

    }
}
