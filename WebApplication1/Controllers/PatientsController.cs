using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using WebApplication1.Data;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("p2")]
    public class PatientsController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IHubContext<DataUpdatesHub> _hubContext;
        private readonly ILogger<PatientsController> _logger;

        public PatientsController(
            ApplicationDbContext dbContext,
            IHubContext<DataUpdatesHub> hubContext,
            ILogger<PatientsController> logger)
        {
            _dbContext = dbContext;
            _hubContext = hubContext;
            _logger = logger;
        }

        public class PatientDto
        {
            public int Id { get; set; }
            public string Name { get; set; }

            public int DaysUsed { get; set; }
        }

        [HttpGet]
        public IEnumerable<PatientDto> Get(string query)
        {
            var result = _dbContext.Patients
                .Select(x => new PatientDto()
                {
                    Id = x.Id,
                    Name = x.Name,
                    DaysUsed = x.DailyStatistics.Count(),
                })
                .OrderByDescending(x => x.DaysUsed)
                .ToList();

            return result;
        }


        [HttpGet("/{id:int}")]
        public PatientDto GetDetails(int id)
        {
            var patient = _dbContext.Patients.Where(x => x.Id == id)
                .Select(x => new PatientDto()
                {
                    Id = x.Id,
                    Name = x.Name,
                    DaysUsed = x.DailyStatistics.Count(),
                })
                .FirstOrDefault();
            return patient;
        }

        [HttpGet("/get2")]
        public async Task Get2(string message)
        {
            await _hubContext.Clients.All.SendAsync("ReceiveMessage", message);
        }
    }
}