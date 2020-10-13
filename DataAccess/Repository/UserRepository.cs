using DataAccess.Context;
using DataAccess.Repository.Interfaces;
using Domain.EF_Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repository
{
    public class UserRepository : BaseRepository<User, string>
    {
        public UserRepository(TimeOffTrackerContext context) : base(context)
        {

        }

        public override async Task<IReadOnlyCollection<User>> GetAllAsync()
        {
            var users =
                from user in _context.Users
                join userRole in _context.UserRoles
                    on user.Id equals userRole.UserId
                join role in _context.Roles
                    on userRole.RoleId equals role.Id
                select new User
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Role = role.Name
                };

            return await users.AsNoTracking().ToListAsync();
        }

        public override async Task<User> FindAsync(string id)
        {
            var resultUser =
                from user in _context.Users
                join userRole in _context.UserRoles
                    on user.Id equals userRole.UserId
                join role in _context.Roles
                    on userRole.RoleId equals role.Id
                where user.Id == id
                select new User
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Role = role.Name
                };

            return await resultUser.AsNoTracking().FirstAsync();
        }
    }
}
