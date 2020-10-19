using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace BusinessLogic.Notifications
{
    public class TestNotification : INotification
    {
        public string Message { get; set; }
    }
}
