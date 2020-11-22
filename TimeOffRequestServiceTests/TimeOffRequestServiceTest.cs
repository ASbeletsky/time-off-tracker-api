using ApiModels.Models;
using AutoMapper;
using BusinessLogic.Exceptions;
using BusinessLogic.Services;
using BusinessLogic.Services.Interfaces;
using DataAccess.Repository.Interfaces;
using DataAccess.Static.Context;
using Domain.EF_Models;
using Domain.Enums;
using MediatR;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TimeOffRequestServiceTests.TestData;
using TimeOffTracker.WebApi.MapperProfile;
using Xunit;

namespace TimeOffRequestServiceTests
{
    public class TimeOffRequestServiceTest
    {
        ITimeOffRequestService _service;
        Mock<IRepository<TimeOffRequest, int>> requestRepository;
        IMapper mapper;
        Mock<IUserService> userService;
        Mock<IMediator> mediator;

        public TimeOffRequestServiceTest()
        {
            requestRepository = new Mock<IRepository<TimeOffRequest, int>>();
            mapper = new Mapper(new MapperConfiguration(cfg => cfg.AddProfile(new MapperProfile())));
            userService = new Mock<IUserService>();
            mediator = new Mock<IMediator>();
        }

        [Theory]
        [ClassData(typeof(TimeOffRequestClassData))]
        public async Task AddAsyncTest(TimeOffRequestApiModel model)
        {
            // Arrange
            var requestList = new List<TimeOffRequest>() {
                new TimeOffRequest {

                    StartDate = DateTime.Now.Date.AddDays(8),
                    Comment = model.Comment,
                    EndDate = DateTime.Now.Date.AddDays(15),
                    UserId = 7,
                    State = VacationRequestState.InProgress,
                    Type = TimeOffType.SocialLeave
                },
                new TimeOffRequest {

                    StartDate = DateTime.Now.Date.AddDays(2),
                    Comment = model.Comment,
                    EndDate = DateTime.Now.Date.AddDays(3),
                    UserId = 7,
                    State = Domain.Enums.VacationRequestState.Rejected,
                    Type = TimeOffType.AdministrativeUnpaidLeave
                }
            };

            requestRepository.Setup(x => x.FilterAsync(u => u.UserId == model.UserId)).ReturnsAsync(requestList);

            requestRepository.Setup(x => x.FilterAsync(u => u.UserId == model.UserId
                    && u.State != Domain.Enums.VacationRequestState.Rejected
                    && (model.ParentRequestId == null || u.Id != model.ParentRequestId)
                    && ((model.StartDate >= u.StartDate && model.StartDate <= u.EndDate)
                        || (model.EndDate <= u.EndDate && model.EndDate >= u.StartDate)
                        || (model.StartDate < u.StartDate && model.EndDate > u.EndDate))))
                .ReturnsAsync(requestList.FindAll(u => u.UserId == model.UserId
                    && u.State != Domain.Enums.VacationRequestState.Rejected
                    && (model.ParentRequestId == null || u.Id != model.ParentRequestId)
                    && ((model.StartDate >= u.StartDate && model.StartDate <= u.EndDate)
                        || (model.EndDate <= u.EndDate && model.EndDate >= u.StartDate)
                        || (model.StartDate < u.StartDate && model.EndDate > u.EndDate))));

            userService.Setup(x => x.GetUser(1)).ReturnsAsync(new UserApiModel() { Role = RoleName.accountant });
            userService.Setup(x => x.GetUser(3)).ReturnsAsync(new UserApiModel() { Role = RoleName.manager });
            userService.Setup(x => x.GetUser(5)).ReturnsAsync(new UserApiModel() { Role = RoleName.manager });
            userService.Setup(x => x.GetUser(6)).ReturnsAsync(new UserApiModel() { Role = RoleName.employee });
            userService.Setup(x => x.GetUser(6)).ReturnsAsync(new UserApiModel() { Role = RoleName.employee });

            _service = new TimeOffRequestService(requestRepository.Object, mapper, userService.Object, mediator.Object);

            TimeOffRequestApiModel result = null;

            // Act
            try
            {
                result = await _service.AddAsync(model);
            }
            catch (ConflictException ex)
            {
                Assert.Null(result);
            }
            catch (NoReviewerException ex)
            {
                Assert.Null(result);
            }
            catch (RequiredArgumentNullException ex)
            {
                Assert.Null(result);
            }

            // Assert
            if (result != null)
            {
                Assert.Equal(model.DurationId, result.DurationId);
                Assert.Equal(model.Comment, result.Comment);
                Assert.Equal(model.StartDate, result.StartDate);
                Assert.Equal(model.EndDate, result.EndDate);
                Assert.Equal(model.UserId, result.UserId);
                Assert.Equal(model.TypeId, result.TypeId);
                Assert.Equal(model.StateId, result.StateId);
                Assert.Equal(model.IsDateIntersectionAllowed, result.IsDateIntersectionAllowed);
                Assert.Equal(model.ReviewsIds.Count, result.Reviews.Count);
            }
        }
    }
}
