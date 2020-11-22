using ApiModels.Models;
using AutoMapper;
using BusinessLogic.Services;
using BusinessLogic.Services.Interfaces;
using DataAccess.Repository.Interfaces;
using DataAccess.Static.Context;
using Domain.EF_Models;
using MediatR;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using TimeOffRequestServiceTests.TestData;
using TimeOffTracker.WebApi.MapperProfile;
using Xunit;

namespace TimeOffRequestServiceTests
{
    public class UpdateAsyncTests
    {
        IMapper _mapper;
        Mock<IMediator> _mockMediator;
        Mock<IUserService> _mockUserService;
        Mock<IRepository<TimeOffRequest, int>> _mockRepo;
        List<UserApiModel> _users = new List<UserApiModel>
        {
            new UserApiModel{ Id = 1, Role = RoleName.accountant },
            new UserApiModel{ Id = 2, Role = RoleName.manager },
            new UserApiModel{ Id = 3, Role = RoleName.manager },
            new UserApiModel{ Id = 4, Role = RoleName.employee },
            new UserApiModel{ Id = 5, Role = RoleName.employee }
        };

        public UpdateAsyncTests()
        {
            _mockMediator = new Mock<IMediator>();

            _mockUserService = new Mock<IUserService>();
            _mockUserService.Setup(us => us.GetUser(It.IsAny<int>())).ReturnsAsync((int id) => _users.FirstOrDefault(u => u.Id == id));

            _mockRepo = new Mock<IRepository<TimeOffRequest, int>>();
            _mockRepo.Setup(r => r.FilterAsync(It.IsAny<Expression<Func<TimeOffRequest, bool>>>())).ReturnsAsync(new TimeOffRequest[0]);

            _mapper = new Mapper(new MapperConfiguration(cfg => cfg.AddProfile(new MapperProfile())));
        }

        [Theory]
        [ClassData(typeof(UpdateAsyncTestData))]
        public async void CorrectWorkTest(TimeOffRequest srcReq, TimeOffRequestApiModel changedModel, TimeOffRequest expReq)
        {
            _mockRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<TimeOffRequest, bool>>>())).ReturnsAsync(srcReq);

            var service = new TimeOffRequestService(_mockRepo.Object, _mapper, _mockUserService.Object, _mockMediator.Object);

            await service.UpdateAsync(srcReq.Id, changedModel);

            Assert.True(srcReq.Duration == expReq.Duration);
            Assert.True(srcReq.State == expReq.State);
            Assert.Equal(srcReq.StartDate, expReq.StartDate);
            Assert.Equal(srcReq.EndDate, expReq.EndDate);
            Assert.Equal(srcReq.Comment, expReq.Comment);
            Assert.True(srcReq.Type == expReq.Type);
            Assert.Equal(srcReq.Reviews.Count(), expReq.Reviews.Count());
            Assert.True(srcReq.Reviews.All(r => r.RequestId == srcReq.Id));
            Assert.True(Enumerable.SequenceEqual(srcReq.Reviews.Select(r => r.ReviewerId), expReq.Reviews.Select(r => r.ReviewerId)));
        }

    }
}
