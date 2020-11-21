using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ApiModels.Models;
using BusinessLogic.Services.Interfaces;
using DataAccess.Static.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace TimeOffTracker.WebApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewsController : BaseController
    {

        private ITimeOffRequestReviewService _service;
        private readonly ILogger<RequestController> _logger;

        public ReviewsController(ITimeOffRequestReviewService service, ILogger<RequestController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [Authorize(Roles = RoleName.manager + ", " + RoleName.accountant)]
        [HttpGet("/user/reviews")]
        public async Task<IEnumerable<TimeOffRequestReviewApiModel>> Get(int? requestId = null, int? stateId = null, DateTime ? startDate = null, DateTime? endDate = null, string name = null, int? typeId = null)
        {
            var reviewerId = int.Parse(this.User.Identity.Name);
            
            return await _service.GetAllAsync(reviewerId, requestId, stateId, startDate, endDate, name, typeId); ;
        }

        [Authorize(Roles = RoleName.admin)]
        [HttpGet("/reviews")]
        public async Task<IEnumerable<TimeOffRequestReviewApiModel>> Get(int? reviewerId = null, int? requestId = null, int? stateId = null, DateTime? startDate = null, DateTime? endDate = null, string name = null, int? typeId = null)
        {
            return await _service.GetAllAsync(reviewerId, requestId, stateId, startDate, endDate, name, typeId); ;
        }

        [Authorize(Roles = RoleName.manager + ", " + RoleName.accountant)]
        [HttpPut("/user/reviews/{id}")]
        public async Task Put(int id, [FromBody] TimeOffRequestReviewApiModel model)
        {
            var reviewerId = int.Parse(this.User.Identity.Name);

            await _service.UpdateAsync(id, model, reviewerId);
            _logger.LogInformation($"Review updated successfully (id: {id}, state: {model.IsApproved})");
        }


        [HttpDelete("/reviews/{id}")]
        public async Task Delete(int id)
        {
            await _service.DeleteAsync(id);
            _logger.LogInformation($"Review deleted successfully (id: {id})");
        }
    }
}
