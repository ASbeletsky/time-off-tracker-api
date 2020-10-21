using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using ApiModels.Models;
using BusinessLogic.Exceptions;
using BusinessLogic.Services;
using BusinessLogic.Services.Interfaces;
using Domain.EF_Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using TimeOffTracker.WebApi.Filters;

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
                await _service.AddAsync(model);

            return Accepted();
        }

        [HttpPut("/requests/{requestId}")]
        public async Task Put(int requestId, [FromBody] TimeOffRequestApiModel newModel)   
        {
            newModel.UserId = int.Parse(this.User.Identity.Name);
            await _service.UpdateAsync(requestId, newModel);
        }

        [Authorize(Roles = "Manager, Accountant, Admin")]
        [HttpDelete("/requests/{requestId}")]
        public async Task Delete(int requestId)
        {
            await _service.DeleteAsync(requestId);
        }

        [HttpDelete("/user/requests/{requestId}")]
        public async Task Delete(int requestId, int userId)
        {
            await _service.RejectedAsync(int.Parse(this.User.Identity.Name), requestId);
        }
    }
}
