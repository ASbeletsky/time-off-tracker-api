using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.EF_Models
{
    public class TimeOffRequestReview
    {
        public Guid Id { get; set; }
        public User Reviewer { get; set; }
        public int ReviewerId { get; set; }
        public bool IsApproved { get; set; }
        public TimeOffRequest Application { get; set; }
        public int ApplicationId { get; set; }
    }
}
