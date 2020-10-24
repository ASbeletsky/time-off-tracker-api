using ApiModels.Models;
using AutoMapper;
using Domain.EF_Models;
using Domain.Enums;
using System.Linq;
using TimeOffTracker.WebApi.ViewModels;

namespace TimeOffTracker.WebApi.MapperProfile
{
    public class MapperProfile : Profile
    {

        public MapperProfile()
        {
            CreateMap<User, UserApiModel>();
            CreateMap<UserApiModel, User>()
                .ForMember(user => user.UserName, opt => opt.Ignore());
                

            CreateMap<RegisterViewModel, User>()
                .ForMember(user => user.UserName, opt => opt.MapFrom(model => string.Concat(model.Email.TakeWhile(ch => ch != '@'))))
                .ForMember(user => user.Id, opt => opt.Ignore());

            CreateMap<TimeOffRequestApiModel, TimeOffRequest>()
                .ForMember(request => request.Type, opt => opt.MapFrom(model => (TimeOffType)model.TypeId))
                .ForMember(request => request.State, opt => opt.MapFrom(model => (VacationRequestState)model.StateId))
                .ForMember(request => request.Duration, opt => opt.MapFrom(model => (TimeOffDuration)model.DurationId))
                .ForMember(request => request.Reviews, opt => opt.MapFrom(model => model.Reviews))
                .ForMember(request => request.Id, opt => opt.Ignore());

            CreateMap<TimeOffRequest, TimeOffRequestApiModel>()
                .ForMember(request => request.TypeId, opt => opt.MapFrom(model => (int)model.Type))
                .ForMember(request => request.StateId, opt => opt.MapFrom(model => (int)model.State))
                .ForMember(request => request.DurationId, opt => opt.MapFrom(model => (int)model.Duration));
                

            CreateMap<TimeOffRequestReview, TimeOffRequestReviewApiModel>();
            CreateMap<TimeOffRequestReviewApiModel, TimeOffRequestReview>();
        }
    }
}
