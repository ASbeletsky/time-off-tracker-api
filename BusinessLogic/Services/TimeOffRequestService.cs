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

namespace BusinessLogic.Services
{
    public class TimeOffRequestService : ITimeOffRequestService
    {
        IRepository<TimeOffRequest, int> _repository;
        IMapper _mapper;
        ITimeOffRequestReviewService _timeOffRequestReviewService;
        IUserService _userService;
        IMediator _mediator;

        public TimeOffRequestService(IRepository<TimeOffRequest, int> repository, IMapper mapper, ITimeOffRequestReviewService timeOffRequestReviewService, IUserService userService, IMediator mediator)
        {
            _repository = repository;
            _mapper = mapper;
            _timeOffRequestReviewService = timeOffRequestReviewService;
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
                    && (start == null || request.StartDate.Date == start)
                    && (end == null || request.EndDate.Date == end)
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
            var requestFromDb = await _repository.FindAsync(requestId);

            switch (requestFromDb.State)
            {
                case VacationRequestState.New:

                    FillReviewers(newModel);
                    newModel.StateId = (int)VacationRequestState.New;
                    if (await CheckNewRequest(newModel))
                        _mapper.Map(newModel, requestFromDb);
                    break;

                case VacationRequestState.InProgress:
                    //Reviewers

                    break;

                case VacationRequestState.Approved:

                    if (await IntersectionDates(newModel))
                        throw new ConflictException("Incorrect dates");
                    if (String.IsNullOrEmpty(newModel.Comment))
                        throw new RequiredArgumentNullException("Comment field is empty");

                    //if Reviewers
                    requestFromDb.EndDate = newModel.EndDate;
                    requestFromDb.StartDate = newModel.StartDate;
                    requestFromDb.Comment = newModel.Comment;
                    requestFromDb.State = VacationRequestState.Rejected;
                    //reviewers update
                    await Duplicate(_mapper.Map<TimeOffRequestApiModel>(requestFromDb), requestFromDb.Id);

                    break;

                case VacationRequestState.Rejected:

                        throw new StateException("State does not allow the request");
            }

            await _repository.UpdateAsync(requestFromDb);           
        }

        public async Task Duplicate(TimeOffRequestApiModel duplicateModel, int parentId)
        {
            var newModel = new TimeOffRequest();
            duplicateModel.ParentRequestId = parentId;
            duplicateModel.StateId = (int)VacationRequestState.New;
            //duplicateModel.ReviewsIds = null;
            //duplicateModel.Reviews = null;
            await _repository.CreateAsync(_mapper.Map(duplicateModel, newModel));

            var notification = new RequestUpdatedNotification { Request = newModel };
            await _mediator.Publish(notification);
        }

        public async Task DeleteAsync(int requestId)
        {
            if (await _repository.FindAsync(x => x.Id == requestId) != null)
                await _repository.DeleteAsync(requestId);
        }

        public async Task RejectedAsync(int userId, int requestId)
        {
            var requestFromDb = await _repository.FindAsync(x => x.UserId == userId && x.Id == requestId);

            if (requestFromDb != null)
            {
                requestFromDb.State = VacationRequestState.Rejected;
                await _repository.UpdateAsync(requestFromDb);

                var notification = new RequestRejectedNotification { Request = requestFromDb };
                await _mediator.Publish(notification);
            }
        }

        private async Task<bool> ValidateAccountingReviewer(TimeOffRequestReview review)
        {              
            var accountantReview = _mapper.Map<User>(await _userService.GetUser(review.ReviewerId));
           
            return (accountantReview != null && accountantReview.Role==RoleName.accountant);
        }

        private bool ValidateManagerReviewers(ICollection<TimeOffRequestReviewApiModel> reviews, int requestTypeId)
        {
            var managerReviews = reviews.Select(x => _userService.GetUser(x.ReviewerId));

            return managerReviews.All(x=>x.Result.Role==RoleName.manager);
        }

        private async Task<bool> CheckNewRequest(TimeOffRequestApiModel obj)
        {
            if(await IntersectionDates(obj))
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
           foreach(var item in obj.ReviewsIds)
           {
                var rewiew = new TimeOffRequestReviewApiModel()
                {                    
                    ReviewerId = item,
                    RequestId = obj.Id
                };

               obj.Reviews.Add(rewiew);
           }
        }

        //private async Task<ICollection<TimeOffRequestReview>> UpdateReviewers(TimeOffRequestApiModel newModel)
        //{
            
        //}

        private async Task<bool> IntersectionDates(TimeOffRequestApiModel obj)
        {
            if (obj.EndDate < obj.StartDate)
                return true;

            return (await _repository.FilterAsync((u => u.UserId == obj.UserId 
            && (obj.StartDate >= u.StartDate && obj.StartDate <= u.EndDate) 
                || (obj.EndDate <= u.EndDate && obj.StartDate >= u.StartDate)))).Any();
        }
    }
}
