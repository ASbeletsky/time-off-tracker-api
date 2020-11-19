using BusinessLogic.Notifications;
using DataAccess.Repository.Interfaces;
using Domain.EF_Models;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BusinessLogic.NotificationHandlers
{
    public class OnStatisticUpdateHandler : INotificationHandler<StatisticUpdateHandler>
    {
        IRepository<UsedDaysStatistic, int> _repository;

        public OnStatisticUpdateHandler(IRepository<UsedDaysStatistic, int> repository)
        {
            _repository = repository;
        }

        public async Task Handle(StatisticUpdateHandler notification, CancellationToken cancellationToken)
        {
            foreach (var item in await _repository.FilterAsync(x => x.RequestId == notification.Request.Id))
            {
                var entry = await _repository.FindAsync(item.Id);
                entry.NumberDaysUsed = 0;
                await _repository.UpdateAsync(entry);
            }
        }
    }
}
