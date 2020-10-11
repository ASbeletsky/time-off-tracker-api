using ApiModels.Models;
using AutoMapper;
using DataAccess.Context;
using Domain.EF_Models;

using System.Linq;

namespace TimeOffTracker.WebApi.MapperProfile
{
    public class MapperProfile : Profile
    {
        private TimeOffTrackerContext _context;

        public MapperProfile(TimeOffTrackerContext context)
        {
            _context = context;
            CreateMap<User, UserApiModel>()
                .ForMember(model => model.Role, opt => opt.MapFrom(usr =>
                    _context.Users
                        .Join(_context.UserRoles, user => user.Id, userRole => userRole.UserId,
                            (user, userRole) => new { userId = user.Id, userRole.RoleId })
                        .Join(_context.Roles, userRole => userRole.RoleId, role => role.Id,
                            (userRole, role) => new { userRole.userId, role.Name })
                        .Where(ur => ur.userId == usr.Id)
                        .Select(ur => ur.Name)
                        .FirstOrDefault()
                ));
        }
    }
}
