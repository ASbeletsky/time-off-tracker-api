namespace BusinessLogic.Exceptions
{
    public class RoleNotFoundException : CustomTimeOffTrackerException
    {
        public RoleNotFoundException(string message) : base(message, 400)
        { }
    }
}
