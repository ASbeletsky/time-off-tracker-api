using Domain.EF_Models;
using MediatR;

namespace BusinessLogic.Notifications
{
    class RequestRejectedNotification : INotification
    {
        public TimeOffRequest Request { get; set; }
    }
}
