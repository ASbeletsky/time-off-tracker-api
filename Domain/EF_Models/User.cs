using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.EF_Models
{
    public class User//:IdentityUser
    {
        public User()
        {
            Applications = new List<TimeOffRequest>();
            Signs = new List<TimeOffRequestReview>();
        }
        public string Position { get; set; }
        public ICollection<TimeOffRequest> Applications { get; set; }
        public ICollection<TimeOffRequestReview> Signs { get; set; }
    }
}
