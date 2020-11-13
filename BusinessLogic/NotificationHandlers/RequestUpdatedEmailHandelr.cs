using ApiModels.Models;
using AutoMapper;
using BusinessLogic.Notifications;
using BusinessLogic.Services.Interfaces;
using DataAccess.Repository.Interfaces;
using Domain.EF_Models;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using EmailTemplateRender;
using EmailTemplateRender.Services.Interfaces;
using EmailTemplateRender.Views.Emails;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using BusinessLogic.Settings;

namespace BusinessLogic.NotificationHandlers
{
    class RequestUpdatedEmailHandelr : INotificationHandler<RequestUpdatedNotification>
    {
        IRepository<TimeOffRequestReview, int> _reviewRepository;
        UserManager<User> _userManager;
        IStringLocalizer<SharedEmailResources> _localizer;
        IRazorViewToStringRenderer _razorViewToStringRenderer;
        IMapper _mapper;
        IEmailService _mailer;
        UIConfig _uiConfig;

        public RequestUpdatedEmailHandelr(
            IRepository<TimeOffRequestReview, int>
            revRepository, UserManager<User> userManager,
            IStringLocalizer<SharedEmailResources> localizer,
            IRazorViewToStringRenderer razorViewToStringRenderer,
            IMapper mapper, 
            IEmailService mailer,
            IOptions<UIConfig> uiConfig)
        {
            _reviewRepository = revRepository;
            _razorViewToStringRenderer = razorViewToStringRenderer;
            _localizer = localizer;
            _userManager = userManager;
            _mapper = mapper;
            _mailer = mailer;
            _uiConfig = uiConfig.Value;
        }

        public async Task Handle(RequestUpdatedNotification notification, CancellationToken cancellationToken)
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

            var curReview = reviews.Where(r => r.IsApproved == null).FirstOrDefault();
            string address = curReview.Reviewer.Email;
            string theme = string.Format(
                _localizer.GetString("UpdatedTheme"),
                    _localizer.GetString(model.RequestType),
                    model.AuthorFullName,
                    model.StartDate,
                    model.EndDate
                    );

            string reference = _uiConfig.Url + $"other_requests/actions?review={curReview.Id}&action=";
            var dataForViewModel = new RequestEmailViewModel(model, reference + "approve", reference + "reject");

            string body = await _razorViewToStringRenderer.RenderViewToStringAsync("/Views/Emails/RequestUpdate/RequestUpdate.cshtml", dataForViewModel);

            await _mailer.SendEmailAsync(address, theme, body);
        }
    }
}
