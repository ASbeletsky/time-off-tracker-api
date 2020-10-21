using ApiModels.Models;
using AutoMapper;
using BusinessLogic.Services.Interfaces;
using DataAccess.Repository.Interfaces;
using Domain.EF_Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.Services
{
    public class TimeOffRequestReviewService : ITimeOffRequestReviewService
    {
        IRepository<TimeOffRequestReview, int> _repository;
        IMapper _mapper;

        public TimeOffRequestReviewService(IRepository<TimeOffRequestReview, int> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
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

        public async Task<TimeOffRequestReviewApiModel> GetByIdAsync(int reviewId)
        {
            return _mapper.Map<TimeOffRequestReviewApiModel>(await _repository.FindAsync(x => x.Id == reviewId));
        }
    }
}
