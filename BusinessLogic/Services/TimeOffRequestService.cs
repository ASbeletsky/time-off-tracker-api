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
using Microsoft.EntityFrameworkCore.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Transactions;

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

        public async Task<TimeOffRequestApiModel> AddAsync(TimeOffRequestApiModel obj)
        {
            if (await IsRequestCorrect(obj))
            {
                FillReviewers(obj);
                obj.StateId = (int)VacationRequestState.New;
                TimeOffRequest request = _mapper.Map<TimeOffRequest>(obj);
                await _repository.CreateAsync(request);

                var notification = new RequestUpdatedNotification { Request = request };
                await _mediator.Publish(notification);

                return _mapper.Map<TimeOffRequestApiModel>(request);
            }
            else
                return null;
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
                throw new RequestNotFoundException($"Request not found: RequestId={requestId}");
            if (newModel.UserId != requestFromDb.UserId)      //only author can change his own request
                throw new ConflictException($"Current user is not the author of the request (userId: {newModel.UserId}, requestId: {requestId})");


            bool needToNotify = false;

            using (var transactionScope = new TransactionScope(TransactionScopeOption.Required, TransactionScopeAsyncFlowOption.Enabled))
            {
                switch (requestFromDb.State)
                {
                    case VacationRequestState.New:
                        newModel.ParentRequestId = requestFromDb.Id;
                        if (await IsRequestCorrect(newModel))
                        {
                            newModel.ParentRequestId = null;
                            newModel.StateId = (int)VacationRequestState.New;
                            FillReviewers(newModel);
                            _mapper.Map(newModel, requestFromDb);
                            needToNotify = true;
                        }
                        break;

                    case VacationRequestState.InProgress:
                        needToNotify = await ChangeAsync(requestFromDb, newModel);
                        break;

                    case VacationRequestState.Approved:
                        await Duplicate(requestFromDb, newModel);
                        break;

                    case VacationRequestState.Rejected:
                        throw new StateException("It is forbidden to change the rejected request");

                    default:
                        throw new StateException("Request status is not allowed or does not exist");
                }

                await _repository.UpdateAsync(requestFromDb);
                transactionScope.Complete();
            }

            if (needToNotify)
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
                throw new RequestNotFoundException($"Request not found: RequestId={requestId}");
            if (requestFromDb.UserId != userId)
                throw new ConflictException($"Current user is not the author of the request (userId: {userId}, requestId: {requestId})");

            switch (requestFromDb.State) { 
                case VacationRequestState.New: 
                    await _repository.DeleteAsync(requestId); 
                    break;

                case VacationRequestState.InProgress:
                case VacationRequestState.Approved:
                    if (requestFromDb.EndDate <= DateTime.Now.Date)
                        throw new ConflictException("End date of the request must be later than the current date");

                    requestFromDb.State = VacationRequestState.Rejected;
                    requestFromDb.ModifiedByUserId = requestFromDb.UserId;
                    await _repository.UpdateAsync(requestFromDb);

                    await _mediator.Publish(new RequestRejectedNotification { Request = requestFromDb });
                    await _mediator.Publish(new StatisticUpdateHandler { Request = requestFromDb });
                    break;
            } 
           
        }

        private async Task<bool> ChangeAsync(TimeOffRequest sourceRequest, TimeOffRequestApiModel changedModel)
        {
            await IsReviewsCorrect(changedModel);

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
                sourceRequest.Reviews.Add(new TimeOffRequestReview() { ReviewerId = reviewerId, RequestId = sourceRequest.Id });

            return isActiveReviewReplace;
        }

        private async Task Duplicate(TimeOffRequest parentRequest, TimeOffRequestApiModel duplicateModel)
        {
            if (parentRequest.EndDate <= DateTime.Now.Date)                     //documentation condition
                throw new ConflictException("End date of the request must be later than the current date");

            var safeModel = _mapper.Map<TimeOffRequestApiModel>(parentRequest);
            //Can change only comment, dates and reviewer
            safeModel.EndDate = duplicateModel.EndDate;
            safeModel.StartDate = duplicateModel.StartDate;
            safeModel.Comment = duplicateModel.Comment;
            safeModel.ReviewsIds = duplicateModel.ReviewsIds;
            safeModel.Reviews.Clear();
            safeModel.ParentRequestId = duplicateModel.Id;

            await AddAsync(safeModel);                                          //All standard checks and create

            parentRequest.State = VacationRequestState.Rejected;
            parentRequest.ModifiedByUserId = parentRequest.UserId;
        }

        private async Task<bool> ValidateAccounting(IEnumerable<int> reviewerId)
        {
            var accountantReview = await _userService.GetUser(reviewerId.FirstOrDefault());

            return (accountantReview.Role == RoleName.accountant);
        }

        private bool ValidateManagers(ICollection<int> reviewerIds)
        {
            reviewerIds = reviewerIds.Skip(1).ToList();
            var managerReviews = reviewerIds.Select(rId => _userService.GetUser(rId).Result).ToList();

            return managerReviews.Any() && managerReviews.All(r => r.Role == RoleName.manager);
        }

        private bool IsNoDuplicate(ICollection<int> ids) => (ids != null && ids.Count() == ids.Distinct().Count());

        private async Task<bool> IsRequestCorrect(TimeOffRequestApiModel request)
        {
            if (request.TypeId == (int)TimeOffType.PaidLeave || request.TypeId == (int)TimeOffType.AdministrativeUnpaidLeave || request.TypeId == (int)TimeOffType.SickLeaveWithDocuments || request.TypeId == (int)TimeOffType.SickLeaveWithoutDocuments)
                if (String.IsNullOrEmpty(request.Comment))
                    throw new RequiredArgumentNullException("Comment field is empty");

            await IsReviewsCorrect(request);

            if (await IntersectionDates(request))
                throw new ConflictException("Dates intersection");

            return true;
        }

        private async Task IsReviewsCorrect(TimeOffRequestApiModel request)
        {
            if (!await ValidateAccounting(request.ReviewsIds))
                throw new NoReviewerException("Not defined accounting");

            if (request.TypeId == (int)TimeOffType.AdministrativeUnpaidLeave || request.TypeId == (int)TimeOffType.StudyLeave || request.TypeId == (int)TimeOffType.PaidLeave)
            {
                if (!ValidateManagers(request.ReviewsIds))
                    throw new NoReviewerException("Not all managers defined");
            }
            else if(request.ReviewsIds.Count() > 1)
                throw new NoReviewerException("Not all managers defined");

            if (!IsNoDuplicate(request.ReviewsIds))
                throw new ConflictException("Any manager cannot be specified more than once");
        }

        private void FillReviewers(TimeOffRequestApiModel request)
        {
            foreach (var item in request.ReviewsIds)
            {
                var rewiew = new TimeOffRequestReviewApiModel()
                {
                    ReviewerId = item,
                    RequestId = request.Id
                };

                request.Reviews.Add(rewiew);
            }
        }

        private async Task<bool> IntersectionDates(TimeOffRequestApiModel obj)
        {
            if (obj.EndDate < obj.StartDate)
                throw new ConflictException("End date can't be earlier than start date");

            return !obj.IsDateIntersectionAllowed &&
                (await _repository.FilterAsync(u => u.UserId == obj.UserId
                    && u.State != VacationRequestState.Rejected
                    && (obj.ParentRequestId == null || u.Id != obj.ParentRequestId)
                    && ((obj.StartDate >= u.StartDate && obj.StartDate <= u.EndDate) || (obj.EndDate <= u.EndDate && obj.StartDate >= u.StartDate)))
                ).Any();
        }
    }
}
