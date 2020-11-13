using ApiModels.Models;
using AutoMapper;
using BusinessLogic.Exceptions;
using BusinessLogic.Notifications;
using BusinessLogic.Services.Interfaces;
using DataAccess.Repository.Interfaces;
using DataAccess.Static.Context;
using Domain.EF_Models;
using Domain.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Transactions;
using TimeOffTracker.WebApi.Exceptions;

namespace BusinessLogic.Services
{
    public class TimeOffRequestService : ITimeOffRequestService
    {
        IRepository<TimeOffRequest, int> _repository;
        IRepository<TimeOffRequestReview, int> _reviewRepository;
        IMapper _mapper;
        IUserService _userService;
        IMediator _mediator;

        public TimeOffRequestService(IRepository<TimeOffRequest, int> repository, IRepository<TimeOffRequestReview, int> reviewRepository, IMapper mapper, IUserService userService, IMediator mediator)
        {
            _repository = repository;
            _reviewRepository = reviewRepository;
            _mapper = mapper;
            _userService = userService;
            _mediator = mediator;
        }

        public async Task AddAsync(TimeOffRequestApiModel obj)
        {
            FillReviewers(obj);

            if (await CheckNewRequest(obj))
            {
                obj.StateId = (int)VacationRequestState.New;
                TimeOffRequest request = _mapper.Map<TimeOffRequest>(obj);
                await _repository.CreateAsync(request);

                var notification = new RequestUpdatedNotification { Request = request };
                await _mediator.Publish(notification);
            }
        }

        public async Task<IReadOnlyCollection<TimeOffRequestApiModel>> GetAllAsync(int userId, DateTime? start = null, DateTime? end = null, int? stateId = null, int? typeId = null)
        {

            Expression<Func<TimeOffRequest, bool>> condition = request =>
                (request.UserId == userId)
                && (start == null || request.StartDate.Date >= start)
                && (end == null || request.EndDate.Date <= end)
                && (stateId == null || (int)request.State == stateId)
                && (typeId == null || (int)request.Type == typeId);

            return _mapper.Map<IReadOnlyCollection<TimeOffRequestApiModel>>(await _repository.FilterAsync(condition));

        }
        public async Task<TimeOffRequestApiModel> GetByIdAsync(int requestId)
        {
            return _mapper.Map<TimeOffRequestApiModel>(await _repository.FindAsync(x => x.Id == requestId));
        }
        public async Task<TimeOffRequestApiModel> GetByIdAsync(int uderId, int requestId)
        {
            return _mapper.Map<TimeOffRequestApiModel>(await _repository.FindAsync(x => x.UserId == uderId && x.Id == requestId));
        }

        public async Task UpdateAsync(int requestId, TimeOffRequestApiModel newModel)
        {
            var requestFromDb = await _repository.FindAsync(r => r.Id == requestId); //include reviews
            if (requestFromDb == null)
                throw new RequestNotFoundException($"Can't find request with id {requestId}");
            if (newModel.UserId != requestFromDb.UserId)      //only author can change his own request
                throw new ConflictException("Current user is not the author of the request");

            bool needToNotify = false; 

            using (var transactionScope = new TransactionScope(TransactionScopeOption.Required, TransactionScopeAsyncFlowOption.Enabled))
            {
                switch (requestFromDb.State)
                {
                    case VacationRequestState.New:
                        FillReviewers(newModel);

                        newModel.ParentRequestId = requestFromDb.Id;
                        if (await CheckNewRequest(newModel))
                        {
                            newModel.StateId = (int)VacationRequestState.New;
                            _mapper.Map(newModel, requestFromDb);
                        }
                        needToNotify = true;
                        break;

                    case VacationRequestState.InProgress:
                        needToNotify = await ChangeAsync(requestFromDb, newModel);
                        break;

                    case VacationRequestState.Approved:
                        await Duplicate(requestFromDb, newModel);
                        break;

                    case VacationRequestState.Rejected: break;
                        throw new StateException("State does not allow the request");
                }

                await _repository.UpdateAsync(requestFromDb);
                transactionScope.Complete();
            }

            if (needToNotify) //incorrect information gets into the letter inside the TransactionScope
            {
                var notification = new RequestUpdatedNotification { Request = requestFromDb };
                await _mediator.Publish(notification);
            }
        }

        public async Task DeleteAsync(int requestId)
        {
            if (await _repository.FindAsync(x => x.Id == requestId) != null)
                await _repository.DeleteAsync(requestId);
        }

        public async Task RejectedByOwnerAsync(int userId, int requestId)
        {
            var requestFromDb = await _repository.FindAsync(x => x.Id == requestId);

            if (requestFromDb == null)
                throw new RequestNotFoundException($"Can't find request with id {requestId}");
            if (requestFromDb.UserId != userId)
                throw new ConflictException("Current user is not the author of the request");
            if (requestFromDb.EndDate <= DateTime.Now.Date)
                throw new ConflictException("End date of the request must be later than the current date");

            requestFromDb.State = VacationRequestState.Rejected;
            requestFromDb.ModifiedByUserId = requestFromDb.UserId;
            await _repository.UpdateAsync(requestFromDb);

            var notification = new RequestRejectedNotification { Request = requestFromDb };
            await _mediator.Publish(notification);
        }

        private async Task<bool> ChangeAsync(TimeOffRequest sourceRequest, TimeOffRequestApiModel changedModel)
        {
            var approvedReviews = sourceRequest.Reviews.TakeWhile(r => r.IsApproved == true);
            
            if (!Enumerable.SequenceEqual(approvedReviews.Select(r => r.ReviewerId), changedModel.ReviewsIds.Take(approvedReviews.Count())))
                throw new ConflictException("Approved reviews can't be changed");   //approved reviews mustn't change

            var replacedReviews = sourceRequest.Reviews.Skip(approvedReviews.Count()).ToList();
            var newReviewers = changedModel.ReviewsIds.Skip(approvedReviews.Count()).ToList();

            if (!newReviewers.Any())
                throw new ConflictException("Last review can't be already approved");

            bool isActiveReviewReplace = replacedReviews.First().ReviewerId != newReviewers.First();
            if (!isActiveReviewReplace)
            {
                replacedReviews.RemoveAt(0);
                newReviewers.RemoveAt(0);
            }

            foreach (TimeOffRequestReview review in replacedReviews)    //delete replaced reviews
                sourceRequest.Reviews.Remove(review);

            foreach (int reviewerId in newReviewers)                    //add new reviews
            {
                if (await CanBeReviewer(reviewerId))
                    sourceRequest.Reviews.Add(new TimeOffRequestReview() { ReviewerId = reviewerId, RequestId = sourceRequest.Id });
                else
                    throw new ConflictException($"User with id {reviewerId} can't be a reviewer");
            }

            return isActiveReviewReplace;
        }

        private async Task<bool> CanBeReviewer(int userId)
        {
            var curUser = await _userService.GetUser(userId);
            if (curUser == null)
                throw new UserNotFoundException("Can't find user with Id");

            return (curUser.Role == RoleName.manager || curUser.Role == RoleName.accountant);
        }

        private async Task Duplicate(TimeOffRequest parentRequest, TimeOffRequestApiModel duplicateModel)
        {
            if (parentRequest.EndDate <= DateTime.Now.Date)                         //documentation condition
                throw new ConflictException("End date of the request must be later than the current date");

            var safeModel = _mapper.Map<TimeOffRequestApiModel>(parentRequest);     //Can change only comment, dates and reviewer

            safeModel.EndDate = duplicateModel.EndDate;
            safeModel.StartDate = duplicateModel.StartDate;
            safeModel.Comment = duplicateModel.Comment;
            safeModel.ReviewsIds = duplicateModel.ReviewsIds;
            safeModel.ParentRequestId = duplicateModel.Id;                      //prevent date intersection

            await AddAsync(safeModel);                                          //All standart checks and create

            parentRequest.State = VacationRequestState.Rejected;                //If all right - change parent request stage
            parentRequest.ModifiedByUserId = parentRequest.UserId;
        }

        private async Task<bool> ValidateAccountingReviewer(TimeOffRequestReview review)
        {
            var accountantReview = _mapper.Map<User>(await _userService.GetUser(review.ReviewerId));

            return (accountantReview != null && accountantReview.Role == RoleName.accountant);
        }

        private bool ValidateManagerReviewers(ICollection<TimeOffRequestReviewApiModel> reviews, int requestTypeId)
        {
            var managerReviews = reviews.Select(x => _userService.GetUser(x.ReviewerId));

            return managerReviews.All(x => x.Result.Role == RoleName.manager);
        }

        private async Task<bool> CheckNewRequest(TimeOffRequestApiModel obj)
        {
            if (await IntersectionDates(obj))
                throw new ConflictException("Incorrect dates");

            if (obj.TypeId == (int)TimeOffType.PaidLeave || obj.TypeId == (int)TimeOffType.AdministrativeUnpaidLeave || obj.TypeId == (int)TimeOffType.SickLeaveWithDocuments || obj.TypeId == (int)TimeOffType.SickLeaveWithoutDocuments)
                if (String.IsNullOrEmpty(obj.Comment))
                    throw new RequiredArgumentNullException("Comment field is empty");

            if (!await ValidateAccountingReviewer(_mapper.Map<TimeOffRequestReview>(obj.Reviews.FirstOrDefault())))
                throw new NoReviewerException("Not defined accounting");

            if (!ValidateManagerReviewers(obj.Reviews.Skip(1).ToList(), obj.TypeId))
                throw new NoReviewerException("Not all managers defined");

            return true;
        }

        private void FillReviewers(TimeOffRequestApiModel obj)
        {
            foreach (var item in obj.ReviewsIds)
            {
                var rewiew = new TimeOffRequestReviewApiModel()
                {
                    ReviewerId = item,
                    RequestId = obj.Id
                };

                obj.Reviews.Add(rewiew);
            }
        }

        private async Task<bool> IntersectionDates(TimeOffRequestApiModel obj)
        {
            if (obj.EndDate < obj.StartDate)
                throw new ConflictException("End date can't be earlier than start date");

            return obj.IsDateIntersectionAllowed || 
                (await _repository.FilterAsync(u => u.UserId == obj.UserId
                    && u.State != VacationRequestState.Rejected
                    && (obj.ParentRequestId == null || u.Id != obj.ParentRequestId)
                    && ((obj.StartDate >= u.StartDate && obj.StartDate <= u.EndDate) || (obj.EndDate <= u.EndDate && obj.StartDate >= u.StartDate)))
                ).Any();
        }
    }
}
