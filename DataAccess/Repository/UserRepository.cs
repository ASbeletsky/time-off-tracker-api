using DataAccess.Context;
using DataAccess.Repository.Interfaces;
using Domain.EF_Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace DataAccess.Repository
{
    public class UserRepository : BaseRepository<User, int>
    {
        public UserRepository(TimeOffTrackerContext context) : base(context)
        {

        }

        public override async Task<IReadOnlyCollection<User>> GetAllAsync()
        {
            var users = GetAllUsers();

            return await users.AsNoTracking().ToListAsync();
        }

        public override async Task<User> FindAsync(int id)
        {
            var resultUser = GetAllUsers();

            return await resultUser.Where(u => u.Id == id).AsNoTracking().FirstOrDefaultAsync();
        }

        public override async Task<IReadOnlyCollection<User>> FilterAsync(Expression<Func<User, bool>> predicate)
        {
            var users = GetAllUsers();

            return await users.Where(predicate).AsNoTracking().ToListAsync();
        }

        private IQueryable<User> GetAllUsers()
        {
            return from user in _context.Users
                   join userRole in _context.UserRoles
                       on user.Id equals userRole.UserId
                   join role in _context.Roles
                       on userRole.RoleId equals role.Id
                   select new User()
                   {
                       Id = user.Id,
                       FirstName = user.FirstName,
                       LastName = user.LastName,
                       UserName = user.UserName,
                       Role = role.Name
                   };
        }
    }
}
