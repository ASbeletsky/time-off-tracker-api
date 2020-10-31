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
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.Services
{
    public class TimeOffRequestReviewService : ITimeOffRequestReviewService
    {
        IRepository<TimeOffRequestReview, int> _reviewRepository;
        IRepository<TimeOffRequest, int> _requestRepository;
        IRepository<User, int> _userRepository;
        IMapper _mapper;
        IMediator _mediator;

        public TimeOffRequestReviewService(IRepository<TimeOffRequestReview, int> reviewRepository, IMapper mapper, IRepository<TimeOffRequest, int> requestRepository, IRepository<User, int> userRepository, IMediator mediator)
        {
            _requestRepository = requestRepository;
            _reviewRepository = reviewRepository;
            _mapper = mapper;           
            _userRepository = userRepository;
            _mediator = mediator;
        }

        public async Task CreateAsync(TimeOffRequestReviewApiModel obj)
        {
            await _reviewRepository.CreateAsync(_mapper.Map<TimeOffRequestReview>(obj));
        }

        public async Task DeleteAsync(int id)
        {
            if (await _reviewRepository.FindAsync(x => x.Id == id) != null)
                await _reviewRepository.DeleteAsync(id);
        }

        public async Task<IReadOnlyCollection<TimeOffRequestReviewApiModel>> GetAllAsync(int? reviewerId = null, int? requestId = null, int? stateId = null, DateTime? startDate = null, DateTime? endDate = null, string name = null, int? typeId = null)
        {
            User userName = null;

            if(name!= null)
                userName = (await _userRepository.FilterAsync(x => ((x.FirstName + " " + x.LastName).ToLower()).Contains(name.ToLower()))).FirstOrDefault();

            if(name != null && userName == null)
                throw new ConflictException("Name not found");

            Expression<Func<TimeOffRequestReview, bool>> condition = review =>
                    (reviewerId == null || review.ReviewerId == reviewerId)
                    && (requestId == null || review.RequestId== requestId)
                    && (stateId == null || (int)review.Request.State == stateId)
                    && (startDate == null || review.Request.StartDate.Date == startDate)
                    && (endDate == null || review.Request.EndDate.Date == endDate)
                    && (name == null || review.Request.UserId == userName.Id)
                    && (typeId == null || (int)review.Request.Type == typeId);

            return _mapper.Map<IReadOnlyCollection<TimeOffRequestReviewApiModel>>(await _reviewRepository.FilterAsync(condition));
        }

        public async Task<TimeOffRequestReviewApiModel> GetByIdAsync(int reviewId)
        {
            return _mapper.Map<TimeOffRequestReviewApiModel>(await _reviewRepository.FindAsync(x => x.Id == reviewId));
        }

        public async Task UpdateAsync(int reviewId, TimeOffRequestReviewApiModel newModel, int userId)
        {
            var reviewfromDb = await _reviewRepository.FindAsync(x=>x.Id == reviewId);

            if (reviewfromDb.Request.State == VacationRequestState.Rejected)
                throw new ConflictException("The request has already been rejected!");
         
            var reviewer = await _userRepository.FindAsync(userId);

            if ((await _requestRepository.FilterAsync(r => r.Id == reviewfromDb.RequestId && r.Reviews
                .Select(x => x.ReviewerId).Contains(userId)))
                .Any() == false) 
                throw new ConflictException("The request is not actual!");

            if (IsReviewPassed(reviewfromDb, userId))
                    throw new ConflictException("The request has already been <approved/rejected>!");

                reviewfromDb.IsApproved = newModel.IsApproved;
                reviewfromDb.Comment = newModel.Comment;
                reviewfromDb.UpdatedAt = DateTime.Now.Date;

            await _reviewRepository.UpdateAsync(reviewfromDb);

            var notification = new ReviewUpdateHandler { Request = await _requestRepository.FindAsync(reviewfromDb.RequestId) };
            await _mediator.Publish(notification);
        }

        private bool IsReviewPassed(TimeOffRequestReview review, int reviewerId)
        {
            return review.Request.Reviews.Where(x => x.ReviewerId == reviewerId && x.IsApproved != null).FirstOrDefault() != null ? true : false;
        }
    }
}
