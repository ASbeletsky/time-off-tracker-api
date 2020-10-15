using ApiModels.Models;
using AutoMapper;
using Domain.EF_Models;
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
        }
    }
}
