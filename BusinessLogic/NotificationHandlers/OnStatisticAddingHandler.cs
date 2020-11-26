using BusinessLogic.Notifications;
using DataAccess.Repository.Interfaces;
using Domain.EF_Models;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BusinessLogic.NotificationHandlers
{
    public class OnStatisticAddingHandler : INotificationHandler<StatisticAddingHandler>
    {
        IRepository<UsedDaysStatistic, int> _repository;

        public OnStatisticAddingHandler(IRepository<UsedDaysStatistic, int> repository)
        {
            _repository = repository;
        }
        public async Task Handle(StatisticAddingHandler notification, CancellationToken cancellationToken)
        {
            var currYear = DateTime.Now.Year;

            var dateRange = Enumerable.Range(0, 1 + notification.Request.EndDate.Subtract(notification.Request.StartDate).Days)
             .Select(day => notification.Request.StartDate.AddDays(day))
             .ToList();

            var entry = CreateObject(notification.Request, currYear.ToString(), dateRange.Where(d => d.Year == currYear).Count());
            await _repository.CreateAsync(entry);

            if (notification.Request.StartDate.Year < currYear)
            {
                var addEntry = CreateObject(notification.Request, notification.Request.StartDate.Year.ToString(), dateRange.Where(d => d.Year == currYear - 1).Count());
                await _repository.CreateAsync(addEntry);
            }
            else if (notification.Request.EndDate.Year > currYear)
            {
                var addEntry = CreateObject(notification.Request, notification.Request.EndDate.Year.ToString(), dateRange.Where(d => d.Year == currYear + 1).Count());
                await _repository.CreateAsync(addEntry);
            }
        }

        private UsedDaysStatistic CreateObject(TimeOffRequest request, string year, int days)
        {
            return new UsedDaysStatistic
            {
                RequestId = request.Id,
                Type = request.Type,
                UserId = request.UserId,
                Year = year,
                NumberDaysUsed = days
            };
        }
    }
}
