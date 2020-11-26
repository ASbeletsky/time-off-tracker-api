using ApiModels.Models;
using System;
using System.Collections;
using System.Collections.Generic;

namespace TimeOffRequestServiceTests.TestData
{
    public class TimeOffRequestClassData : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            yield return new object[] { new TimeOffRequestApiModel() {
                StartDate=DateTime.Now.Date,
                Comment= "TestRequest",
                EndDate=DateTime.Now.Date.AddDays(4),
                IsDateIntersectionAllowed=false,
                TypeId=7,
                ReviewsIds = new List<int>() { 1, 3, 5},
                UserId = 7
            }};

            yield return new object[] { new TimeOffRequestApiModel() {
                StartDate=DateTime.Now.Date,
                Comment= "TestRequest",
                EndDate=DateTime.Now.Date.AddDays(2),
                IsDateIntersectionAllowed=false,
                TypeId=7,
                ReviewsIds = new List<int>() { 1, 3, 5},
                UserId = 7
            }};

            yield return new object[] { new TimeOffRequestApiModel() {
                StartDate=DateTime.Now.Date.AddDays(10),
                Comment= "TestRequest",
                EndDate=DateTime.Now.Date.AddDays(15),
                IsDateIntersectionAllowed=false,
                TypeId=3,
                ReviewsIds = new List<int>() { 1, 3, 5},
                UserId = 7
            }};

            yield return new object[] { new TimeOffRequestApiModel() {
                StartDate=DateTime.Now.Date.AddDays(20),
                Comment= "TestRequest",
                EndDate=DateTime.Now.Date.AddDays(23),
                IsDateIntersectionAllowed=false,
                TypeId=4,
                ReviewsIds = new List<int>() { 1, 6, 5},
                UserId = 7
            }};

            yield return new object[] { new TimeOffRequestApiModel() {
                StartDate=DateTime.Now.Date.AddDays(25),
                Comment= "TestRequest",
                EndDate=DateTime.Now.Date.AddDays(27),
                IsDateIntersectionAllowed=false,
                TypeId=6,
                ReviewsIds = new List<int>() { 3, 6, 5},
                UserId = 7
            }};

            yield return new object[] { new TimeOffRequestApiModel() {
                StartDate=DateTime.Now.Date.AddDays(30),
                EndDate=DateTime.Now.Date.AddDays(35),
                IsDateIntersectionAllowed=false,
                TypeId=7,
                ReviewsIds = new List<int>() { 1, 3, 5},
                UserId = 7
            }};
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
