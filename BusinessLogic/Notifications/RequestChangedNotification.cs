using Domain.EF_Models;
using MediatR;

namespace BusinessLogic.Notifications
{
    class RequestChangedNotification : INotification
    {
        public TimeOffRequest Request { get; set; }
    }
}
