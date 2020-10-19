using BusinessLogic.Notifications;
using BusinessLogic.Services.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Threading;
using System.Threading.Tasks;

namespace BusinessLogic.NotificationHandlers
{
    class TestNotificationHandler : INotificationHandler<TestNotification>
    {
        IEmailService _mailer;
        private const string resourceFile = @"BusinessLogic.Resources.Email";
        ResourceManager _resourceManager;

        public TestNotificationHandler(IEmailService mailer) 
        {
            _resourceManager = new ResourceManager(resourceFile, Assembly.GetExecutingAssembly());
            _mailer = mailer;
        }

        public async Task Handle(TestNotification notification, CancellationToken cancellationToken)
        {
            string bodyPath = _resourceManager.GetString("UpdatedBody");
            string body = string.Empty;
            using (StreamReader SourceReader = File.OpenText(bodyPath))
            {
                body = SourceReader.ReadToEnd();
            }

            string theme = string.Format(_resourceManager.GetString("UpdatedTheme"), "Hell", "No", "newer", "again");
            await _mailer.SendEmailAsync("pedenkoia@gmail.com", theme, body);
        }
    }
}
