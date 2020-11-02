using ApiModels.Models;
using AutoMapper;
using BusinessLogic.Exceptions;
using BusinessLogic.Notifications;
using BusinessLogic.Services.Interfaces;
using DataAccess.Repository.Interfaces;
using Domain.EF_Models;
using EmailTemplateRender;
using EmailTemplateRender.Services.Interfaces;
using EmailTemplateRender.Views.Emails;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TimeOffTracker.WebApi.Exceptions;

namespace BusinessLogic.NotificationHandlers
{
    class RequestRejectEmailHandelr : INotificationHandler<RequestRejectedNotification>
    {
        IRepository<TimeOffRequestReview, int> _reviewRepository;
        UserManager<User> _userManager;
        IStringLocalizer<SharedEmailResources> _localizer;
        IRazorViewToStringRenderer _razorViewToStringRenderer;
        IMapper _mapper;
        IEmailService _mailer;

        public RequestRejectEmailHandelr(
            IRepository<TimeOffRequestReview, int>
            revRepository, UserManager<User> userManager,
            IStringLocalizer<SharedEmailResources> localizer,
            IRazorViewToStringRenderer razorViewToStringRenderer,
            IMapper mapper,
            IEmailService mailer)
        {
            _reviewRepository = revRepository;
            _razorViewToStringRenderer = razorViewToStringRenderer;
            _localizer = localizer;
            _userManager = userManager;
            _mapper = mapper;
            _mailer = mailer;
        }

        public async Task Handle(RequestRejectedNotification notification, CancellationToken cancellationToken)
        {
            TimeOffRequest request = notification.Request;
            RequestDataForEmailModel model = _mapper.Map<RequestDataForEmailModel>(request);

            User author = await _userManager.FindByIdAsync(request.UserId.ToString());
            model.AuthorFullName = $"{author.FirstName} {author.LastName}".Trim();

            IEnumerable<TimeOffRequestReview> reviews = await _reviewRepository.FilterAsync(rev => rev.RequestId == request.Id);

            foreach (var review in reviews)
                review.Reviewer = await _userManager.FindByIdAsync(review.ReviewerId.ToString());

            var approvedPeopleNames = reviews.Where(r => r.IsApproved == true).Select(r => $"{r.Reviewer.FirstName} {r.Reviewer.LastName}".Trim()).ToList();
            model.ApprovedFullNames = string.Join(", ", approvedPeopleNames);


            var rejectReview = reviews.Where(r => r.IsApproved == false).FirstOrDefault();
            bool isDeclineByTheOwner = rejectReview == null;
            if (isDeclineByTheOwner)
            {
                model.RejectedBy = model.AuthorFullName;
                model.RejectComment = _localizer.GetString("DeclinedByTheOwner");
            }
            else
            {
                var rejectPerson = await _userManager.FindByIdAsync(rejectReview.ReviewerId.ToString());
                if (rejectPerson == null)
                    throw new UserNotFoundException($"No user found who canceled the request. UserId: {rejectReview.ReviewerId}");

                model.RejectedBy = $"{rejectPerson.FirstName} {rejectPerson.LastName}".Trim();
                model.RejectComment = rejectReview.Comment;
            }

            var dataForViewModel = new RequestEmailViewModel(model);

            if(!isDeclineByTheOwner)
            {   //Author mail
                string authorAddress = author.Email;
                string authorTheme = string.Format(
                    _localizer.GetString("RejectedAuthorTheme"),
                        _localizer.GetString(model.RequestType),
                        model.StartDate,
                        model.EndDate
                        );

                string authorBody = await _razorViewToStringRenderer.RenderViewToStringAsync("/Views/Emails/RequestReject/RequestRejectForAuthor.cshtml", dataForViewModel);

                await _mailer.SendEmailAsync(authorAddress, authorTheme, authorBody);
            }

            {   //Approved people mail
                string rejectedTheme = string.Format(
                    _localizer.GetString("RejectedTheme"),
                        _localizer.GetString(model.RequestType),
                        model.AuthorFullName,
                        model.StartDate,
                        model.EndDate
                        );

                string rejectedBody = await _razorViewToStringRenderer.RenderViewToStringAsync("/Views/Emails/RequestReject/RequestReject.cshtml", dataForViewModel);
                foreach(TimeOffRequestReview review in reviews.Where(r => r.IsApproved == true))
                {
                    string approvedPerson = review.Reviewer.Email;
                    await _mailer.SendEmailAsync(approvedPerson, rejectedTheme, rejectedBody);
                }
            }
        }
    }
}
