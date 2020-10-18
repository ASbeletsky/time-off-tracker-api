using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ApiModels.Models;
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
        private ITimeOffRequestService<TimeOffRequestApiModel> _service;
        private readonly ILogger<RequestController> _logger;

        public RequestController(ITimeOffRequestService<TimeOffRequestApiModel> service, ILogger<RequestController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [Authorize(Roles = "Manager, Accountant, Admin")]
        [HttpGet]
        [Route("{userId:int}/{startDate}/{endDate}/{stateId:int}/{typeId:int}")]
        public async Task<IReadOnlyCollection<TimeOffRequestApiModel>> Get(int userId, string startDate = null, string endDate = null,  int stateId = 0, int typeId = 0)
        {
           
            return await _service.GetAllAsync(userId, DateTime.Parse(startDate), DateTime.Parse(endDate), stateId, typeId);
        }

        [HttpGet]
        [Route("{startDate}/{endDate}/{stateId:int}/{typeId:int}")]
        public async Task<IReadOnlyCollection<TimeOffRequestApiModel>> Get(string startDate = null, string endDate = null, int stateId = 0, int typeId = 0)
        {
            return await _service.GetAllAsync(int.Parse(this.User.Identity.Name), DateTime.Parse(startDate), DateTime.Parse(endDate), stateId, typeId);
        }

        [Authorize(Roles = "Manager, Accountant, Admin")]
        [HttpGet]
        [Route("{userId:int}/{requestId:int}")]
        public async Task<TimeOffRequestApiModel> Get(int userId, int requestId)
        {
            return await _service.GetByIdAsync(userId, requestId);
        }

        [HttpGet]
        [Route("{requestId:int}")]
        public async Task<TimeOffRequestApiModel> Get(int requestId)
        {
            return await _service.GetByIdAsync(int.Parse(this.User.Identity.Name), requestId);
        }



        [HttpPost]
        public async Task Post([FromForm] TimeOffRequestApiModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.TypeId > 0 && (model.TypeId < 8 
                    && model.StartDate.Date >= DateTime.Now.Date 
                    && model.EndDate.Date > model.StartDate.Date
                    && model.Reviews.Count > 0)) 
                {
                    await _service.AddAsync(model, int.Parse(this.User.Identity.Name));
                }
            }
        }

        [HttpPut]
        public async Task Put(int userId, [FromForm] TimeOffRequestApiModel newModel)
        {
            await _service.UpdateAsync(userId, newModel);
        }

        [HttpDelete]
        [Authorize(Roles = "Admin")]
        [Route("{userId:int}/{requestId:int}")]
        public async Task Delete(int userId, int requestId)
        {
            await _service.DeleteAsync(userId, requestId);
        }

        [HttpDelete]
        [Route("{requestId:int}")]
        public async Task Delete(int requestId)
        {
            await _service.RejectedAsync(int.Parse(this.User.Identity.Name), requestId);
        }
    }
}
