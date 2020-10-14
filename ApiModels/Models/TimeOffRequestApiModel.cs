using System;
using System.Collections.Generic;
using System.Text;

namespace ApiModels.Models
{
    public class TimeOffRequestApiModel
    {
        public TimeOffRequestApiModel()
        {
            Reviews = new List<TimeOffRequestReviewApiModel>();
        }
        public int Id { get; set; }
        public int TypeId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public ICollection<TimeOffRequestReviewApiModel> Reviews { get; set; }
        public bool HasAccountingReviewPassed { get; set; }
        public string Comment { get; set; }
        public string ProjectRole { get; set; }
        public int StateId { get; set; }
        public int DurationId { get; set; }
        public UserApiModel User { get; set; }
        public int UserId { get; set; }
    }
}
