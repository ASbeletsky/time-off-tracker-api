using ApiModels.Models;
using AutoMapper;
using AutoMapper.Internal;
using DataAccess.Repository;
using DataAccess.Repository.Interfaces;
using Domain.EF_Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.Services
{
    public class UserService
    {
        IRepository<User, string> _repository;
        IMapper _mapper;

        public UserService(IRepository<User, string> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<UserApiModel> GetUser(User user) => _mapper.Map<UserApiModel>(await _repository.FindAsync(user.Id));

        public async Task<IEnumerable<UserApiModel>> GetUsers() => _mapper.Map<IEnumerable<UserApiModel>>(await _repository.GetAllAsync());
    }
}
