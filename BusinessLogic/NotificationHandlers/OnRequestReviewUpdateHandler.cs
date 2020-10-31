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
        IMediator _mediator;
        public OnRequestReviewUpdateHandler(IRepository<TimeOffRequest, int> requestRepository, IMediator mediator)
        {
            _requestRepository = requestRepository;
            _mediator = mediator;
        }

        public async Task Handle(ReviewUpdateHandler notification, CancellationToken cancellationToken)
        {
            var requestfromDb = await _requestRepository.FindAsync(x => x.Id == notification.Request.Id);

            if(requestfromDb.Reviews.Any(x => x.IsApproved == null))
                requestfromDb.State = VacationRequestState.InProgress;
            else if(requestfromDb.Reviews.All(x => x.IsApproved == true))
                requestfromDb.State = VacationRequestState.Approved;
            else if(requestfromDb.Reviews.Any(x => x.IsApproved == false))
                requestfromDb.State = VacationRequestState.Rejected;

            await _requestRepository.UpdateAsync(requestfromDb);

            //var notification_approved = new RequestApprovedNotification { Request = await _requestRepository.FindAsync(notification.Request.Id) };
            //await _mediator.Publish(notification_approved);
        }
    }
}
