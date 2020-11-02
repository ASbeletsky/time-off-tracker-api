using ApiModels.Models;
using AutoMapper;
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
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BusinessLogic.NotificationHandlers
{
    class RequestApprovedEmailHander : INotificationHandler<RequestApprovedNotification>
    {
        IRepository<TimeOffRequestReview, int> _reviewRepository;
        UserManager<User> _userManager;
        IStringLocalizer<SharedEmailResources> _localizer;
        IRazorViewToStringRenderer _razorViewToStringRenderer;
        IMapper _mapper;
        IEmailService _mailer;

        public RequestApprovedEmailHander(
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

        public async Task Handle(RequestApprovedNotification notification, CancellationToken cancellationToken)
        {
            TimeOffRequest request = notification.Request;
            RequestDataForEmailModel model = _mapper.Map<RequestDataForEmailModel>(request);

            User author = await _userManager.FindByIdAsync(request.UserId.ToString());
            model.AuthorFullName = $"{author.FirstName} {author.LastName}".Trim();

            IEnumerable<TimeOffRequestReview> reviews = await _reviewRepository.FilterAsync(rev => rev.RequestId == request.Id);

            foreach (var review in reviews)
                review.Reviewer = await _userManager.FindByIdAsync(review.ReviewerId.ToString());

            var approvedPeopleNames = reviews.Select(r => $"{r.Reviewer.FirstName} {r.Reviewer.LastName}".Trim()).ToList();
            model.ApprovedFullNames = string.Join(", ", approvedPeopleNames);

            var dataForViewModel = new RequestEmailViewModel(model);
            
            {   //Author mail
                string authorAddress = author.Email;
                string authorTheme = string.Format(
                    _localizer.GetString("ApprovedAuthorTheme"),
                        _localizer.GetString(model.RequestType),
                        model.StartDate,
                        model.EndDate
                        );

                string authorBody = await _razorViewToStringRenderer.RenderViewToStringAsync("/Views/Emails/RequestApprove/RequestApproveForAuthor.cshtml", dataForViewModel);

                await _mailer.SendEmailAsync(authorAddress, authorTheme, authorBody);
            }

            {   //Accountant mail
                string accountantAddress = reviews.FirstOrDefault().Reviewer.Email;
                string accountantTheme = string.Format(
                    _localizer.GetString("ApprovedTheme"),
                        _localizer.GetString(model.RequestType),
                        model.AuthorFullName,
                        model.StartDate,
                        model.EndDate
                        );

                string accountantBody = await _razorViewToStringRenderer.RenderViewToStringAsync("/Views/Emails/RequestApprove/RequestApprove.cshtml", dataForViewModel);

                await _mailer.SendEmailAsync(accountantAddress, accountantTheme, accountantBody);
            }
        }
    }
}
