using ApiModels.Models;
using AutoMapper;
using BusinessLogic.Notifications;
using BusinessLogic.Services.Interfaces;
using DataAccess.Repository.Interfaces;
using DataAccess.Static.Context;
using Domain.EF_Models;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BusinessLogic.NotificationHandlers
{
    class RequestUpdatedEmailHandelr : INotificationHandler<RequestUpdatedNotification>
    {
        private const string resourceFile = @"BusinessLogic.Resources.Email";
        ResourceManager _resourceManager;

        IRepository<TimeOffRequestReview, int> _reviewRepository;
        UserManager<User> _userManager;
        IMapper _mapper;
        IEmailService _mailer;

        public RequestUpdatedEmailHandelr(IRepository<TimeOffRequestReview, int> revRepository, UserManager<User> userManager, IMapper mapper, IEmailService mailer)
        {
            _resourceManager = new ResourceManager(resourceFile, Assembly.GetExecutingAssembly());
            _reviewRepository = revRepository;
            _userManager = userManager;
            _mapper = mapper;
            _mailer = mailer;
        }

        public async Task Handle(RequestUpdatedNotification notification, CancellationToken cancellationToken)
        {          
            TimeOffRequest request = notification.Request;
            User author = await _userManager.FindByIdAsync(request.UserId.ToString());

            RequestDataForEmailModel model = _mapper.Map<RequestDataForEmailModel>(request);
            model.AuthorFullName = author.FirstName + " " + author.LastName;
            
            //correct work
            string theme = string.Format(_resourceManager.GetString("UpdatedTheme"), model.RequestType, model.AuthorFullName, model.StartDate, model.EndDate); 

            string address = string.Empty;
            if (request.HasAccountingReviewPassed)
            {
                // obtain collection of reviews and reviewers (if repository function was override) 
                // Another way: we can manually get reviewers for reviews collection using UserManager/UserRepository (in foreach loop)
                IEnumerable<TimeOffRequestReview> reviews = await _reviewRepository.FilterAsync(rev => rev.RequestId == request.Id);
                List<string> approvedPeopleNames = new List<string>(); 
                foreach (TimeOffRequestReview review in reviews)
                {
                    StringBuilder sb = new StringBuilder(50).Append(_resourceManager.GetString("Accountant"));
                    if (review.IsApproved) //collect names for approved list
                    {
                        sb.Append(", ").Append(review.Reviewer.FirstName + " " + review.Reviewer.LastName);
                    }
                    else // next manager in reviews
                    {
                        address = review.Reviewer.Email;
                        break;
                    }
                    model.ApprovedFullNames = sb.ToString();
                }
            }
            else //Accountant not approved
            {
                var users = await _userManager.GetUsersInRoleAsync(RoleName.accountant);
                address = users.FirstOrDefault().Email;
            }

            string head = await GetMailHead();
            string body = await GetMailBody();

            body = string.Format(body,
                model.AuthorFullName,       //{0} : Author
                model.RequestType,          //{1} : RequestType  
                model.StartDate,            //{2} : StartDate  
                model.EndDate,              //{3} : EndDate 
                model.Duration,             //{4} : Duration 
                model.Comment,              //{5} : Comment
                model.ApprovedFullNames     //{6} : ApprovedBy 
                );                          //{...}: references for button

            await _mailer.SendEmailAsync(address, theme, head + body);
        }

        private async Task<string> GetMailHead()
        {
            string bodyHeadPath = _resourceManager.GetString("UpdatedMailHead");
            string head = string.Empty;
            using (StreamReader SourceReader = File.OpenText(bodyHeadPath))
            {
                head = await SourceReader.ReadToEndAsync();
            }

            return head;
        }

        private async Task<string> GetMailBody()
        {
            string bodyPath = _resourceManager.GetString("UpdatedMailBody");
            string body = string.Empty;
            using (StreamReader SourceReader = File.OpenText(bodyPath))
            {
                body = await SourceReader.ReadToEndAsync();
            }

            return body;
        }
    }
}
