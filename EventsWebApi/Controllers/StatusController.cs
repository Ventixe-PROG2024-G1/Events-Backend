using EventsWebApi.Domain;
using Microsoft.AspNetCore.Mvc;

namespace EventsWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StatusController : ControllerBase
    {
        [HttpGet("eventstatuses")]
        public IActionResult GetEventStatuses()
        {
            var eventStatuses = Enum.GetValues<EventStatus>()
                .Cast<EventStatus>()
                .Select(status => new {
                    Id = (int)status,
                    Name = status.ToString()
                })
                .ToList();
            return Ok(eventStatuses);
        }
    }
}
