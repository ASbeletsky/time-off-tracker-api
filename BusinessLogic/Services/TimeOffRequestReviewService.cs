using ApiModels.Models;
using AutoMapper;
using BusinessLogic.Exceptions;
using BusinessLogic.Services.Interfaces;
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
    public class TimeOffRequestReviewService : ITimeOffRequestReviewService
    {
        IRepository<TimeOffRequestReview, int> _repository;
        IMapper _mapper;
        IUserService _userService;

        public TimeOffRequestReviewService(IRepository<TimeOffRequestReview, int> repository, IMapper mapper, IUserService userService)
        {
            _repository = repository;
            _mapper = mapper;
            _userService = userService;
        }

        public async Task CreateAsync(TimeOffRequestReviewApiModel obj)
        {
            await _repository.CreateAsync(_mapper.Map<TimeOffRequestReview>(obj));
        }

        public async Task DeleteAsync(int id)
        {
            if (await _repository.FindAsync(x => x.Id == id) != null)
                await _repository.DeleteAsync(id);
        }

        public async Task<IReadOnlyCollection<TimeOffRequestReviewApiModel>> GetAllAsync(int userId, int? stateId = null, DateTime? startDate = null, DateTime? endDate = null, string name = null, int? typeId = null)
        {           
            Expression<Func<TimeOffRequestReview, bool>> condition = review =>
                    (review.ReviewerId == userId)
                    && (stateId == null || (int)review.Request.State == stateId)
                    && (startDate == null || review.Request.StartDate.Date == startDate)
                    && (endDate == null || review.Request.EndDate.Date == endDate)
                    && (name == null || (review.Request.User.LastName + " " + review.Request.User.FirstName).ToLower().Contains(name.ToLower()))
                    && (typeId == null || (int)review.Request.Type == typeId);

            return _mapper.Map<IReadOnlyCollection<TimeOffRequestReviewApiModel>>(await _repository.FilterAsync(condition));
        }

        public async Task<TimeOffRequestReviewApiModel> GetByIdAsync(int reviewId)
        {
            return _mapper.Map<TimeOffRequestReviewApiModel>(await _repository.FindAsync(x => x.Id == reviewId));
        }

        public async Task UpdateAsync(int reviewId, TimeOffRequestReviewApiModel newModel, int userId)
        {
            var reviewfromDb = await _repository.FindAsync(x=>x.Id == reviewId);

            if (reviewfromDb.Request.State == VacationRequestState.Rejected)
                throw new ConflictException("The request has already been rejected!");

            var reviewer = await _userService.GetUser(userId);

                if (reviewfromDb.Request.Reviews.FirstOrDefault(x => x.ReviewerId == userId) == null)
                    throw new ConflictException("The request is not actual!");

                if (isReapproval(reviewfromDb, userId))
                    throw new ConflictException("The request has already been approved!");

                reviewfromDb.IsApproved = newModel.IsApproved;
                reviewfromDb.Comment = newModel.Comment;

                if (reviewer.Role == RoleName.accountant)
                    reviewfromDb.Request.HasAccountingReviewPassed = true;

                if (reviewfromDb.Request.Reviews.All(x => x.IsApproved != null))
                    reviewfromDb.Request.State = VacationRequestState.Approved;
            
            await _repository.UpdateAsync(reviewfromDb);
        }

        public async Task UpdateAsync(TimeOffRequestReviewApiModel newModel, int userId)
        {
            var requestfromDb = await _repository.FindAsync(x => x.RequestId == newModel.RequestId);

            if (requestfromDb.Request.State == VacationRequestState.New)
                requestfromDb.Request.State = VacationRequestState.InProgress;

            await _repository.UpdateAsync(requestfromDb);
        }

        private bool isReapproval(TimeOffRequestReview review, int reviewerId)
        {
            return review.Request.Reviews.Where(x => x.ReviewerId == reviewerId && x.IsApproved != null).FirstOrDefault() != null ? true : false;
        }
    }
}
