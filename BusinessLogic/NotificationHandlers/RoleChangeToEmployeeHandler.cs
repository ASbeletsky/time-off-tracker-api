using BusinessLogic.Notifications;
using DataAccess.Repository.Interfaces;
using Domain.EF_Models;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BusinessLogic.NotificationHandlers
{
    class RoleChangeToEmployeeHandler : INotificationHandler<RoleChangeToEmployeeNotification>
    {
        IRepository<TimeOffRequest, int> _requestRepository;
        IRepository<TimeOffRequestReview, int> _reviewRepository;
        IMediator _mediator;

        public RoleChangeToEmployeeHandler(IRepository<TimeOffRequest, int> requestRepository, IRepository<TimeOffRequestReview, int> reviewRepository, IMediator mediator)
        {
            _requestRepository = requestRepository;
            _reviewRepository = reviewRepository;
            _mediator = mediator;
        }

        public async Task Handle(RoleChangeToEmployeeNotification notification, CancellationToken cancellationToken)
        {
            User user = notification.User;

            var activeReviews = (await _reviewRepository.FilterAsync(r => r.ReviewerId == user.Id && r.IsApproved == null)).ToList();

            foreach (TimeOffRequestReview review in activeReviews)
            {
                if (review.Request.Reviews.OrderBy(rev => rev.Id).First(r => r.IsApproved == null) == review)
                {
                    review.IsApproved = true;
                    review.UpdatedAt = DateTime.Now.Date;
                    await _reviewRepository.UpdateAsync(review);

                    var reviewUpdateNotification = new ReviewUpdateHandler { Request = await _requestRepository.FindAsync(review.RequestId) };
                    await _mediator.Publish(reviewUpdateNotification);
                }
                else 
                {
                    review.Request.Reviews.Remove(review);
                    await _requestRepository.UpdateAsync(review.Request);
                }
            }

        }
    }
}
