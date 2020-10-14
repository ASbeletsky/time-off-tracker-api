using Domain.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Domain.EF_Models
{
    public class TimeOffRequest
    {
        public TimeOffRequest()
        {
            Reviews = new List<TimeOffRequestReview>();
        }
        public int Id { get; set; }
        public TimeOffType Type { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public ICollection<TimeOffRequestReview> Reviews { get; set; }
        public bool HasAccountingReviewPassed { get; set; }
        public string Comment { get; set; }
        public string ProjectRole { get; set; }
        public VacationRequestState State { get; set; }
        public TimeOffDuration Duration { get; set; }
        public User User { get; set; }
        public int UserId { get; set; }
    }
}
