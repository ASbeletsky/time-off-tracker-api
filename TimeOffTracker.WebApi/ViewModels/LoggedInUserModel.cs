namespace TimeOffTracker.WebApi.ViewModels
{
    public class LoggedInUserModel
    {
        public int UserId { get; set; }

        public string Role { get; set; }

        public string Token { get; set; }
    }
}
