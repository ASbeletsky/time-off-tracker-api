using System.ComponentModel.DataAnnotations;

namespace TimeOffTracker.WebApi.ViewModels
{
    public class RoleChangeModel
    {
        [Required]
        public int UserId { get; set; }
        
        [Required]
        public string Role { get; set; }
    }
}
