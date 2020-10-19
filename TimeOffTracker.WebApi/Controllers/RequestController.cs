using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using ApiModels.Models;
using BusinessLogic.Exceptions;
using BusinessLogic.Services;
using BusinessLogic.Services.Interfaces;
using Domain.EF_Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace TimeOffTracker.WebApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class RequestController : ControllerBase
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
        public async Task<IReadOnlyCollection<TimeOffRequestApiModel>> Get(int userId = 0, string startDate = null, string endDate = null, int stateId = 0, int typeId = 0)
        {
            return await _service.GetAllAsync(userId, startDate, endDate, stateId, typeId);         
        }

        [HttpGet("/user/requests")]
        public async Task<IReadOnlyCollection<TimeOffRequestApiModel>> Get(string startDate = null, string endDate = null, int stateId = 0, int typeId = 0)
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
        public async Task <HttpStatusCode> Post ([FromForm] TimeOffRequestApiModel model)
        {
            if (!ModelState.IsValid)
                return HttpStatusCode.BadRequest;
            else
            {
                try
                {
                    await _service.AddAsync(model, int.Parse(this.User.Identity.Name));
                }
                catch(Exception ex)
                {
                   if(ex is ConflictException)
                        return HttpStatusCode.Conflict;
                   else if (ex is RequiredArgumentNullException || ex is NoReviewerException)
                        return HttpStatusCode.BadRequest;
                }   
            }
            return HttpStatusCode.OK;
        }

        [HttpPut("/requests/{requestId}")]
        public async Task Put(int requestId, [FromForm] TimeOffRequestApiModel newModel)   //FromBody not coming
        {
            await _service.UpdateAsync(requestId, newModel);
        }

        [Authorize(Roles = "Manager, Accountant, Admin")]
        [HttpDelete("requests/{requestId}")]
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
