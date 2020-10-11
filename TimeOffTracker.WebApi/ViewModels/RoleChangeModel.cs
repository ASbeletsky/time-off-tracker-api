using System.ComponentModel.DataAnnotations;

namespace TimeOffTracker.WebApi.ViewModels
{
    public class RoleChangeModel
    {
        [Required]
        public string UserId { get; set; }
        
        [Required]
        public string Role { get; set; }
    }
}
