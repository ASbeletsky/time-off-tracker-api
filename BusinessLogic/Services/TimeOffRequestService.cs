using ApiModels.Models;
using AutoMapper;
using DataAccess.Repository;
using DataAccess.Repository.Interfaces;
using Domain.EF_Models;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.Services
{
    public class TimeOffRequestService
    {
        TimeOffRequestRepository _repository;
        IMapper _mapper;

        public TimeOffRequestService(TimeOffRequestRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task AddAsync(TimeOffRequestApiModel obj)
        {
            await _repository.CreateAsync(_mapper.Map<TimeOffRequest>(obj)); 
        }
        public async Task<IReadOnlyCollection<TimeOffRequestApiModel>> GetAllAsync(int userId)
        {
            return _mapper.Map< IReadOnlyCollection<TimeOffRequestApiModel>> (await _repository.FilterAsync(x=>x.UserId == userId));
        }
        public async Task<TimeOffRequestApiModel> GetByIdAsync(int userId, int id)
        {        
            return _mapper.Map<TimeOffRequestApiModel>(await _repository.FindAsync(x => x.UserId == userId && x.Id == id));
        }
        public async Task UpdateAsync(int id, TimeOffRequestApiModel newModel)
        {
            var requestFromDb = await _repository.FindAsync(id);

            if (requestFromDb.State == VacationRequestState.InProgress ||
                requestFromDb.State == VacationRequestState.Approved ||
                requestFromDb.State == VacationRequestState.Rejected)
                return;

            requestFromDb.StartDate = newModel.StartDate;
            requestFromDb.EndDate = newModel.EndDate;
            requestFromDb.Comment = newModel.Comment;

            await _repository.UpdateAsync(requestFromDb);
        }

        public async Task DeleteAsync(int userId, int id)
        {
            if(await _repository.FilterAsync(x => x.UserId == userId && x.Id == id) != null)
                await _repository.DeleteAsync(id);
        }

        
    }
}
