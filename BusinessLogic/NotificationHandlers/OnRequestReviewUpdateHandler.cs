using BusinessLogic.Notifications;
using BusinessLogic.Services.Interfaces;
using DataAccess.Repository;
using DataAccess.Repository.Interfaces;
using Domain.EF_Models;
using Domain.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BusinessLogic.NotificationHandlers
{
    class OnRequestReviewUpdateHandler : INotificationHandler<ReviewUpdateHandler>
    {
        IRepository<TimeOffRequest, int> _requestRepository;
      
        public OnRequestReviewUpdateHandler(IRepository<TimeOffRequest, int> requestRepository)
        {
            _requestRepository = requestRepository;
        }

        public async Task Handle(ReviewUpdateHandler notification, CancellationToken cancellationToken)
        {
            var requestfromDb = await _requestRepository.FindAsync(x => x.Id == notification.Request.Id);

            if (requestfromDb.Reviews.All(x => x.IsApproved != null))
            {
                requestfromDb.State = VacationRequestState.Approved;
                await _requestRepository.UpdateAsync(requestfromDb);                
            }             
        }
    }
}
