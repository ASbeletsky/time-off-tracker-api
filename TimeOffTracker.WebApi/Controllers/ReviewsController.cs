using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiModels.Models;
using BusinessLogic.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TimeOffTracker.WebApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewsController : ControllerBase
    {

        private ITimeOffRequestReviewService _service;
        private readonly ILogger<RequestController> _logger;

        public ReviewsController(ITimeOffRequestReviewService service, ILogger<RequestController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [Authorize(Roles = "Manager, Accountant")]
        [HttpGet("/user/reviews")]
        public async Task<IReadOnlyCollection<TimeOffRequestReviewApiModel>> Get(int? requestId = null, int? stateId = null, DateTime ? startDate = null, DateTime? endDate = null, string name = null, int? typeId = null)
        {
            var reviewerId = int.Parse(this.User.Identity.Name);
            
            return await _service.GetAllAsync(reviewerId, requestId, stateId, startDate, endDate, name, typeId); ;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("/reviews")]
        public async Task<IReadOnlyCollection<TimeOffRequestReviewApiModel>> Get(int? reviewerId = null, int? requestId = null, int? stateId = null, DateTime? startDate = null, DateTime? endDate = null, string name = null, int? typeId = null)
        {
            return await _service.GetAllAsync(reviewerId, requestId, stateId, startDate, endDate, name, typeId); ;
        }

        [Authorize(Roles = "Manager, Accountant")]
        [HttpPut("/user/reviews/{id}")]
        public async Task Put(int id, [FromBody] TimeOffRequestReviewApiModel model)
        {
            var reviewerId = int.Parse(this.User.Identity.Name);

            await _service.UpdateAsync(id, model, reviewerId);
        }


        [HttpDelete("/reviews/{id}")]
        public async Task Delete(int id)
        {
            await _service.DeleteAsync(id);
        }
    }
}
