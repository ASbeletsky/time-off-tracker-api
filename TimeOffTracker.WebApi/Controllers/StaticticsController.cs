using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiModels.Models;
using BusinessLogic.Services.Interfaces;
using DataAccess.Static.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace TimeOffTracker.WebApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class StaticticsController : BaseController
    {
        private IStatisticService _service;

        public StaticticsController(IStatisticService service)
        {
            _service = service;
        }

        [Authorize(Roles = RoleName.employee)]
        [HttpGet]
        public async Task<IEnumerable<UsedDaysStatisticApiModel>> Get()
        {
            return await _service.GetStatisticByUserAsync(int.Parse(this.User.Identity.Name));

        }
    }
}
