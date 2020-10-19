using ApiModels.Models;
using AutoMapper;
using BusinessLogic.Exceptions;
using BusinessLogic.Services.Interfaces;
using DataAccess.Repository;
using DataAccess.Repository.Interfaces;
using DataAccess.Static.Context;
using Domain.EF_Models;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.Services
{
    public class TimeOffRequestService : ITimeOffRequestService
    {
        IRepository<TimeOffRequest, int> _repository;
        IMapper _mapper;

        public TimeOffRequestService(IRepository<TimeOffRequest, int> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task AddAsync(TimeOffRequestApiModel obj, int userId)
        {
            obj.StateId = (int)VacationRequestState.New;

            var userRequests = await _repository.FilterAsync(x => x.UserId == userId);

            foreach (var item in userRequests)
                if (IntersectionDates(obj.StartDate, obj.EndDate, item.StartDate, item.EndDate))
                    throw new ConflictException("Have a vacation on these dates");

            if (obj.TypeId == (int)TimeOffType.PaidLeave || obj.TypeId == (int)TimeOffType.AdministrativeUnpaidLeave || obj.TypeId == (int)TimeOffType.SickLeaveWithDocuments || obj.TypeId == (int)TimeOffType.SickLeaveWithoutDocuments)
                if (String.IsNullOrEmpty(obj.Comment))
                    throw new RequiredArgumentNullException("Comment field is empty");

            if (!ValidateAccountingReviewer(_mapper.Map<UserApiModel>(obj.Reviews.FirstOrDefault())))
                throw new NoReviewerException("Not defined accounting");

            if (!ValidateManagerReviewers(obj.Reviews.Skip(1).ToList(), obj.TypeId))
                throw new NoReviewerException("Not all managers defined");

            await _repository.CreateAsync(_mapper.Map<TimeOffRequest>(obj));
        }

        public async Task<IReadOnlyCollection<TimeOffRequestApiModel>> GetAllAsync(int userId, string start, string end, int stateId, int typeId)
        {
            try
            {
                Expression<Func<TimeOffRequest, bool>> condition = request =>
                    (userId == 0 || request.UserId == userId)
                    && (start == null || request.StartDate.Date == DateTime.Parse(start).Date)
                    && (end == null || request.EndDate.Date == DateTime.Parse(end).Date)
                    && (stateId == 0 || (int)request.State == stateId) //-1?
                    && (typeId == 0 || (int)request.Type == typeId); //-1?

                return _mapper.Map<IReadOnlyCollection<TimeOffRequestApiModel>>(await _repository.FilterAsync(condition));
            }
            catch (Exception ex)
            {

            }

            return null;
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

            if (requestFromDb != null)
            {
                if (requestFromDb.State == VacationRequestState.InProgress ||
                    requestFromDb.State == VacationRequestState.Approved ||
                    requestFromDb.State == VacationRequestState.Rejected)
                    return;

                requestFromDb.StartDate = newModel.StartDate;
                requestFromDb.EndDate = newModel.EndDate;
                requestFromDb.Comment = newModel.Comment;

                await _repository.UpdateAsync(requestFromDb);
            }
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

        private bool ValidateAccountingReviewer(UserApiModel reviewer)
        {
            if(reviewer != null)
             return reviewer.Role == RoleName.accountant ? true : false;
          
                return false;
        }

        private bool ValidateManagerReviewers(ICollection<TimeOffRequestReviewApiModel> reviewers, int requestTypeId)
        {
            if(reviewers.Count>0)
                return reviewers.All(x=>x.Reviewer.Role == RoleName.manager) ? true : false;

            return false;
        }

    }
}
