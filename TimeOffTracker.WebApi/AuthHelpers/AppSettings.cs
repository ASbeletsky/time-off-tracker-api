namespace TimeOffTracker.WebApi.AuthHelpers
{
    public class AppSettings
    {
        public string Secret { get; set; }

        public double TokenExpiresTimeHours { get; set; }
    }
}
