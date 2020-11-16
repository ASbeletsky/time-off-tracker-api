using Domain.EF_Models;
using MediatR;

namespace BusinessLogic.Notifications
{
    public class RoleChangeToEmployeeNotification : INotification
    {
        public User User { get; set; }
    }
}
