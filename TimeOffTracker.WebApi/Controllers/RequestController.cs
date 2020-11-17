using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiModels.Models;
using BusinessLogic.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace TimeOffTracker.WebApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class RequestController : BaseController
    {
        private ITimeOffRequestService _service;
        private readonly ILogger<RequestController> _logger;

        public RequestController(ITimeOffRequestService service, ILogger<RequestController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [Authorize(Roles = "Manager, Accountant, Admin")]
        [HttpGet("/requests")]
        public async Task<IReadOnlyCollection<TimeOffRequestApiModel>> Get(int userId, DateTime? startDate = null, DateTime? endDate = null, int? stateId = null, int? typeId = null)
        {
            return await _service.GetAllAsync(userId, startDate, endDate, stateId, typeId);
        }

        [HttpGet("/user/requests")]
        public async Task<IReadOnlyCollection<TimeOffRequestApiModel>> Get(DateTime? startDate = null, DateTime? endDate = null, int? stateId = null, int? typeId = null)
        {
            return await _service.GetAllAsync(int.Parse(this.User.Identity.Name), startDate, endDate, stateId, typeId);
        }

        [Authorize(Roles = "Manager, Accountant, Admin")]
        [HttpGet("{requestId}")]      
        public async Task<TimeOffRequestApiModel> Get(int requestId)
        {
            return await _service.GetByIdAsync(requestId);
        }

        [HttpGet("/user/requests/{requestId}")]
        public async Task<TimeOffRequestApiModel> Get(int requestId, int userId)
        {
            return await _service.GetByIdAsync(int.Parse(this.User.Identity.Name), requestId);
        }

        
        [HttpPost("/requests")]
        public async Task<IActionResult> Post ([FromBody] TimeOffRequestApiModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.Values.SelectMany(v => v.Errors));
                       
            model.UserId = int.Parse(this.User.Identity.Name);
            TimeOffRequestApiModel newRequest =  await _service.AddAsync(model);
            _logger.LogInformation($"Request created successfully(id: {newRequest.Id}, author: {model.UserId})");

            return Ok(newRequest);
        }

        [HttpPut("/requests/{requestId}")]
        public async Task Put(int requestId, [FromBody] TimeOffRequestApiModel newModel)   
        {
            newModel.UserId = int.Parse(this.User.Identity.Name);
            _logger.LogInformation($"Request updated successfully(id: {requestId})");
            await _service.UpdateAsync(requestId, newModel);
        }

        [Authorize(Roles = "Manager, Accountant, Admin")]
        [HttpDelete("/requests/{requestId}")]
        public async Task Delete(int requestId)
        {
            await _service.DeleteAsync(requestId);
            _logger.LogInformation($"Request deleted successfully(id: {requestId})");
        }

        [HttpDelete("/user/requests/{requestId}")]
        public async Task RejectByOwner(int requestId)
        {
            int userId = int.Parse(this.User.Identity.Name);
            await _service.RejectedByOwnerAsync(userId, requestId);
            _logger.LogInformation($"Request rejected by author (id: {requestId}, authorId: {userId})");
        }
    }
}
