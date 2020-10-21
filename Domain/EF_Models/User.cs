using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.EF_Models
{
    public class User : IdentityUser<int>
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public ICollection<TimeOffRequest> Requests { get; set; }

        public User()
        {
            Requests = new List<TimeOffRequest>();
        }

        [NotMapped]
        public string Role { get; set; }
    }
}
