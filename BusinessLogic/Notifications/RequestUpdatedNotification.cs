using Domain.EF_Models;
using MediatR;

namespace BusinessLogic.Notifications
{
    class RequestUpdatedNotification : INotification
    {
        public TimeOffRequest Request { get; set; }
    }
}
