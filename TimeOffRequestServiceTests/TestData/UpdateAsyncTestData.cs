using ApiModels.Models;
using Domain.EF_Models;
using Domain.Enums;
using System;
using System.Collections.Generic;
using Xunit;

namespace TimeOffRequestServiceTests.TestData
{
    class UpdateAsyncTestData : TheoryData<TimeOffRequest, TimeOffRequestApiModel, TimeOffRequest>
    {
        public UpdateAsyncTestData()
        {
            Add(    //new
                new TimeOffRequest { Id = 1, UserId = 5, State = VacationRequestState.New, Comment = "State new" },
                new TimeOffRequestApiModel
                {
                    Id = 1,
                    UserId = 5,
                    DurationId = 2,
                    Comment = "Hello new",
                    TypeId = 2,
                    StartDate = DateTime.Now.Date.AddDays(2),
                    EndDate = DateTime.Now.Date.AddDays(4),
                    ReviewsIds = new int[] { 1, 2 }
                },
                new TimeOffRequest
                {
                    Duration = TimeOffDuration.FullDay,
                    Comment = "Hello new",
                    State = VacationRequestState.New,
                    Type = TimeOffType.AdministrativeUnpaidLeave,
                    StartDate = DateTime.Now.Date.AddDays(2),
                    EndDate = DateTime.Now.Date.AddDays(4),
                    Reviews = new List<TimeOffRequestReview>
                    {
                        new TimeOffRequestReview { RequestId = 1, ReviewerId = 1 },
                        new TimeOffRequestReview { RequestId = 1, ReviewerId = 2 }
                    }
                }
            );

            Add(    //inProgress
                new TimeOffRequest
                {
                    Id = 2,
                    UserId = 6,
                    State = VacationRequestState.InProgress,
                    Comment = "InProgress",
                    Duration = TimeOffDuration.FullDay,
                    Type = TimeOffType.AdministrativeUnpaidLeave,
                    StartDate = DateTime.Now.Date.AddDays(2),
                    EndDate = DateTime.Now.Date.AddDays(4),
                    Reviews = new List<TimeOffRequestReview>
                    {
                        new TimeOffRequestReview { RequestId = 2, ReviewerId = 1 },
                        new TimeOffRequestReview { RequestId = 2, ReviewerId = 2 }
                    }
                },
                new TimeOffRequestApiModel
                {
                    Id = 2,
                    UserId = 6,
                    DurationId = 1,
                    Comment = "Hello new",
                    TypeId = 7,
                    StartDate = DateTime.Now.Date.AddDays(3),
                    EndDate = DateTime.Now.Date.AddDays(5),
                    ReviewsIds = new int[] { 1, 3, 2 }
                },
                new TimeOffRequest
                {
                    Duration = TimeOffDuration.FullDay,
                    Comment = "InProgress",
                    State = VacationRequestState.InProgress,
                    Type = TimeOffType.AdministrativeUnpaidLeave,
                    StartDate = DateTime.Now.Date.AddDays(2),
                    EndDate = DateTime.Now.Date.AddDays(4),
                    Reviews = new List<TimeOffRequestReview>
                    {
                        new TimeOffRequestReview { RequestId = 2, ReviewerId = 1 },
                        new TimeOffRequestReview { RequestId = 2, ReviewerId = 3 },
                        new TimeOffRequestReview { RequestId = 2, ReviewerId = 2 }
                    }
                }
            );

            Add(    //Approved
                new TimeOffRequest
                {
                    Id = 3,
                    UserId = 3,
                    State = VacationRequestState.Approved,
                    Comment = "Approved",
                    Duration = TimeOffDuration.FullDay,
                    Type = TimeOffType.AdministrativeUnpaidLeave,
                    StartDate = DateTime.Now.Date.AddDays(2),
                    EndDate = DateTime.Now.Date.AddDays(4),
                },
                new TimeOffRequestApiModel
                {
                    Id = 3,
                    UserId = 3,
                    DurationId = 1,
                    Comment = "Hello new",
                    TypeId = 7,
                    StartDate = DateTime.Now.Date.AddDays(3),
                    EndDate = DateTime.Now.Date.AddDays(5),
                    ReviewsIds = new int[] { 1, 3, 2 }
                },
                new TimeOffRequest
                {
                    Id = 3,
                    UserId = 3,
                    State = VacationRequestState.Rejected,
                    Comment = "Approved",
                    Duration = TimeOffDuration.FullDay,
                    Type = TimeOffType.AdministrativeUnpaidLeave,
                    StartDate = DateTime.Now.Date.AddDays(2),
                    EndDate = DateTime.Now.Date.AddDays(4),
                }
            );
        }
    }
}
