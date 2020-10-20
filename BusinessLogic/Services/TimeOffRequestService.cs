using ApiModels.Models;
using AutoMapper;
using BusinessLogic.Exceptions;
using BusinessLogic.Services.Interfaces;
using DataAccess.Repository;
using DataAccess.Repository.Interfaces;
using DataAccess.Static.Context;
using Domain.EF_Models;
using Domain.Enums;
using Microsoft.AspNetCore.Http.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.Services
{
    public class TimeOffRequestService : ITimeOffRequestService
    {
        IRepository<TimeOffRequest, int> _repository;
        IMapper _mapper;
        ITimeOffRequestReviewService _timeOffRequestReviewService;

        public TimeOffRequestService(IRepository<TimeOffRequest, int> repository, IMapper mapper, ITimeOffRequestReviewService timeOffRequestReviewService)
        {
            _repository = repository;
            _mapper = mapper;
            _timeOffRequestReviewService = timeOffRequestReviewService;
        }

        public async Task AddAsync(TimeOffRequestApiModel obj)
        {
            await FillReviewers(obj);

            if (await CheckNewRequest(obj))
            {
                obj.StateId = (int)VacationRequestState.New;
                await _repository.CreateAsync(_mapper.Map<TimeOffRequest>(obj));
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

            if (requestFromDb.State == VacationRequestState.New)
            {
                if (await CheckNewRequest(newModel))
                {
                    requestFromDb.Comment = newModel.Comment;
                    requestFromDb.Duration = (TimeOffDuration)newModel.DurationId;
                    requestFromDb.EndDate = newModel.EndDate;
                    requestFromDb.StartDate = newModel.StartDate;
                    requestFromDb.HasAccountingReviewPassed = false;
                    requestFromDb.Reviews = _mapper.Map<ICollection<TimeOffRequestReview>>(newModel.Reviews);
                    requestFromDb.State = (VacationRequestState)newModel.StateId;
                    requestFromDb.Type = (TimeOffType)newModel.TypeId;
                }
            }
           else if(requestFromDb.State == VacationRequestState.InProgress)
           {
               
           }
           else if (requestFromDb.State == VacationRequestState.Approved)
           {
                if(requestFromDb.EndDate.Date > DateTime.Now.Date)
                    throw new ConflictException("End date is later than current");

                var userRequests = await _repository.FilterAsync(x => x.UserId == requestFromDb.UserId);

                foreach (var item in userRequests)
                    if (IntersectionDates(newModel.StartDate, newModel.EndDate, item.StartDate, item.EndDate))
                        throw new ConflictException("Have a vacation on these dates");

                if (String.IsNullOrEmpty(newModel.Comment))
                    throw new RequiredArgumentNullException("Comment field is empty");

                //if Reviewers

                requestFromDb.EndDate = newModel.EndDate;
                requestFromDb.StartDate = newModel.StartDate;
                requestFromDb.Comment = newModel.Comment;
                requestFromDb.State = VacationRequestState.Rejected;
                //reviewers update
                await Duplicate(_mapper.Map<TimeOffRequestApiModel>(requestFromDb));
           }

            await _repository.UpdateAsync(requestFromDb);
        }

        public async Task Duplicate(TimeOffRequestApiModel duplicateModel)
        {
            await this.AddAsync(new TimeOffRequestApiModel
            {
                DurationId = duplicateModel.DurationId,
                Comment = duplicateModel.Comment,
                StartDate = duplicateModel.StartDate,
                EndDate = duplicateModel.EndDate,
                HasAccountingReviewPassed = duplicateModel.HasAccountingReviewPassed,
                Reviews = duplicateModel.Reviews, //обнулить
                StateId = duplicateModel.StateId,
                TypeId = duplicateModel.TypeId,
                UserId = duplicateModel.UserId,
                ParentRequestId = duplicateModel.Id
            });
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
            }
        }

        private bool IntersectionDates(DateTime beginDate1, DateTime endDate1, DateTime beginDate2, DateTime endDate2)
        {
            if (beginDate2 > endDate1 || endDate2 < beginDate1)
                return false;
            else
                return true;
        }

        private bool ValidateAccountingReviewer(TimeOffRequestReview review)
        {
            if(review.Reviewer != null)
             return review.Reviewer.Role == RoleName.accountant ? true : false;
          
                return false;
        }

        private bool ValidateManagerReviewers(ICollection<TimeOffRequestReviewApiModel> reviewers, int requestTypeId)
        {
            if(reviewers.Count>0)
                return reviewers.All(x=>x.Reviewer.Role == RoleName.manager) ? true : false;

            return false;
        }

        private async Task<bool> CheckNewRequest(TimeOffRequestApiModel obj)
        {            
            var userRequests = await _repository.FilterAsync(x => x.UserId == obj.Id);

            if(userRequests.Count>0)
                if(userRequests.Where((u => u.UserId == obj.Id && (obj.EndDate < u.StartDate || obj.StartDate > u.EndDate))).Any() == false)
                    throw new ConflictException("Have a vacation on these dates");

            //if (!_repository.FilterAsync((u => u.UserId == obj.Id && (obj.EndDate < u.StartDate || obj.StartDate > u.EndDate))).Result.Any())
            //    throw new ConflictException("Have a vacation on these dates");

            if (obj.TypeId == (int)TimeOffType.PaidLeave || obj.TypeId == (int)TimeOffType.AdministrativeUnpaidLeave || obj.TypeId == (int)TimeOffType.SickLeaveWithDocuments || obj.TypeId == (int)TimeOffType.SickLeaveWithoutDocuments)
                if (String.IsNullOrEmpty(obj.Comment))
                    throw new RequiredArgumentNullException("Comment field is empty");

            if (!ValidateAccountingReviewer(_mapper.Map<TimeOffRequestReview>(obj.Reviews.FirstOrDefault())))
                throw new NoReviewerException("Not defined accounting");

            if (!ValidateManagerReviewers(obj.Reviews.Skip(1).ToList(), obj.TypeId))
                throw new NoReviewerException("Not all managers defined");

            return true;
        }

        private async Task FillReviewers(TimeOffRequestApiModel obj)
        {
            foreach (var item in obj.ReviewsIds)
                obj.Reviews.Add(await _timeOffRequestReviewService.GetByIdAsync(item));
        }
    }
}
