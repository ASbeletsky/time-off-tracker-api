using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ApiModels.Models;
using BusinessLogic.Services;
using Domain.EF_Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;


namespace TimeOffTracker.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RequestController : ControllerBase
    {
        private TimeOffRequestService _service;
      
        public RequestController(TimeOffRequestService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IReadOnlyCollection<TimeOffRequestApiModel>> Get()
        {
            var id = this.User.Identity.Name;
            return await _service.GetAllAsync(this.User.Identity.Name);
        }

        [HttpGet("{id}")]
        public async Task<TimeOffRequestApiModel> Get(int id)
        {
            return await _service.GetByIdAsync(this.User.Identity.Name, id);
        }

        [HttpPost]
        public async Task Post([FromBody] TimeOffRequestApiModel model)
        {
            await _service.AddAsync(model);
        }

        [HttpPut("{id}")]
        public async Task Put(int id, [FromBody] TimeOffRequestApiModel newModel)
        {
            await _service.UpdateAsync(id, newModel);
        }

        [HttpDelete("{id}")]
        public async Task Delete(int id)
        {
            await _service.DeleteAsync(this.User.Identity.Name, id);
        }
    }
}
