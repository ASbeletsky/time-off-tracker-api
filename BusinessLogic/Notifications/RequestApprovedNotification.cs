using Domain.EF_Models;
using MediatR;

namespace BusinessLogic.Notifications
{
    class RequestApprovedNotification : INotification
    {
        public TimeOffRequest Request { get; set; }
    }
}
