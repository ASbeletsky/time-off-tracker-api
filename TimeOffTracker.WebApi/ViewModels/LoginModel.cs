using System.ComponentModel.DataAnnotations;

namespace TimeOffTracker.WebApi.ViewModels
{
    public class LoginModel
    {
        [Required(ErrorMessage = "Login can't be empty")]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
