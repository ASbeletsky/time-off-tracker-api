using Domain.EF_Models;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace BusinessLogic.Notifications
{
    class ReviewUpdateHandler : INotification
    {
        public TimeOffRequest Request { get; set; }
    }
}
