using ApiModels.Models;
using DataAccess.Repository;
using DataAccess.Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.Services
{
    public class TimeOffRequestService
    {
        //need mapper
        TimeOffRequestRepository _repository;
        public TimeOffRequestService(TimeOffRequestRepository repository)
        {
            _repository = repository;
        }

        public async Task AddAsync(TimeOffRequestApiModel obj)
        {
            //await _repository.CreateAsync(obj); 
        }
        public async Task<IReadOnlyCollection<TimeOffRequestApiModel>> GetAllAsync(string userId)
        {
            //return await _repository.FilterAsync(x=>x.UserId == userId);
            return null;
        }
        public async Task<TimeOffRequestApiModel> GetByIdAsync(string userId, int id)
        {
            //return await _repository.FilterAsync(x => x.UserId == userId && x.Id == id);
            return null;
        }
        public async Task UpdateAsync(int id, TimeOffRequestApiModel newModel)
        {
            var requestFromDb = await _repository.FindAsync(id);

            //status ... ?

            requestFromDb.StartDate = newModel.StartDate;
            requestFromDb.EndDate = newModel.EndDate;
            requestFromDb.Comment = newModel.Comment;

            await _repository.UpdateAsync(requestFromDb);
        }

        public async Task DeleteAsync(string userId, int id)
        {
            if(await _repository.FilterAsync(x => x.UserId == userId && x.Id == id) != null)
                await _repository.DeleteAsync(id);
        }

        
    }
}
