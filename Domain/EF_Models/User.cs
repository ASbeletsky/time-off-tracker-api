using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace Domain.EF_Models
{
    public class User : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public ICollection<TimeOffRequest> Requests { get; set; }

        public User()
        {
            Requests = new List<TimeOffRequest>();
        }
    }
}
