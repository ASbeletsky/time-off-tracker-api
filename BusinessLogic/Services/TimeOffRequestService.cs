using ApiModels.Models;
using AutoMapper;
using BusinessLogic.Services.Interfaces;
using DataAccess.Repository;
using DataAccess.Repository.Interfaces;
using Domain.EF_Models;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.Services
{
    public class TimeOffRequestService : ITimeOffRequestService<TimeOffRequestApiModel>
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
            var userRequests = await _repository.FilterAsync(x => x.UserId == userId);

            foreach (var item in userRequests)
                if (IntersectionDates(obj.StartDate, obj.EndDate, item.StartDate, item.EndDate))
                    return;

            if(obj.TypeId == (int)TimeOffType.PaidLeave || obj.TypeId == (int)TimeOffType.AdministrativeUnpaidLeave || obj.TypeId == (int)TimeOffType.SickLeaveWithDocuments || obj.TypeId == (int)TimeOffType.SickLeaveWithoutDocuments)
            {
                if (String.IsNullOrEmpty(obj.Comment))
                    return;
            }


            if (obj.TypeId == (int)TimeOffType.PaidLeave || obj.TypeId == (int)TimeOffType.AdministrativeUnpaidLeave || obj.TypeId == (int)TimeOffType.StudyLeave)
            {
                if (obj.Reviews.FirstOrDefault().Reviewer.Role == "Accountant")
                {
                    if (obj.Reviews.Skip(1).All(x => x.Reviewer.Role == "Manager"))
                    {
                        if (!String.IsNullOrEmpty(obj.ProjectRole))
                        {
                            await _repository.CreateAsync(_mapper.Map<TimeOffRequest>(obj));
                        }
                    }
                }
            }
            else if (obj.TypeId == (int)TimeOffType.ForceMajeureAdministrativeLeave || obj.TypeId == (int)TimeOffType.SocialLeave)
            {
                if (obj.Reviews.FirstOrDefault(x => x.Reviewer.Role == "Accountant") != null)
                {
                    await _repository.CreateAsync(_mapper.Map<TimeOffRequest>(obj));
                }
            }
            else if (obj.TypeId == (int)TimeOffType.SickLeaveWithDocuments || obj.TypeId == (int)TimeOffType.SickLeaveWithoutDocuments)
            {
                if (obj.Reviews.FirstOrDefault(x => x.Reviewer.Role == "Accountant") != null)
                {
                    if (obj.HasAccountingReviewPassed == true || obj.HasAccountingReviewPassed == false)
                    {
                        await _repository.CreateAsync(_mapper.Map<TimeOffRequest>(obj));
                    }
                }
            }

            var result = await _repository.FindAsync(obj.Id);

            if(result != null)
            {
                result.State = VacationRequestState.New;
                await _repository.UpdateAsync(result);
            }          
        }
        public async Task<IReadOnlyCollection<TimeOffRequestApiModel>> GetAllAsync(int userId, DateTime start, DateTime end, int stateId, int typeId)
        {
            return (_mapper.Map<IReadOnlyCollection<TimeOffRequestApiModel>>(await _repository.FilterAsync(x => x.UserId == userId && x.StartDate == start
                && x.EndDate == end && x.State == (VacationRequestState)Enum.GetValues(typeof(VacationRequestState)).GetValue(stateId)
                && x.Type == (TimeOffType)Enum.GetValues(typeof(TimeOffType)).GetValue(typeId))).ToList());
        }
        public async Task<TimeOffRequestApiModel> GetByIdAsync(int userId, int requestId)
        {
            return _mapper.Map<TimeOffRequestApiModel>(await _repository.FindAsync(x => x.UserId == userId && x.Id== requestId));
        }
        public async Task UpdateAsync(int userId, TimeOffRequestApiModel newModel)
        {
            var requestFromDb = await _repository.FindAsync(userId);

            if (requestFromDb.State == VacationRequestState.InProgress ||
                requestFromDb.State == VacationRequestState.Approved ||
                requestFromDb.State == VacationRequestState.Rejected)
                return;

            requestFromDb.StartDate = newModel.StartDate;
            requestFromDb.EndDate = newModel.EndDate;
            requestFromDb.Comment = newModel.Comment;

            await _repository.UpdateAsync(requestFromDb);
        }

        public async Task DeleteAsync(int userId, int requestId)
        {
            if (await _repository.FindAsync(x => x.UserId == userId && x.Id==requestId) != null)
                await _repository.DeleteAsync(requestId);
        }

        public async Task RejectedAsync(int userId, int requestId)
        {
            var requestFromDb = await _repository.FindAsync(x => x.UserId == userId && x.Id== requestId);
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
    }
}
