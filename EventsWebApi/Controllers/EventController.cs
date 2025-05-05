using EventsWebApi.ApiModels.Requests;
using EventsWebApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EventsWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventController(IEventService eventService) : ControllerBase
    {
        private readonly IEventService _eventService = eventService;

        [HttpPost]
        public async Task<IActionResult> CreateEvent(CreateEventRequest requestData)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _eventService.CreateEventAsync(requestData);

            if (result == null)
                return BadRequest("Failed to create Event");
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllEvents()
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _eventService.GetAllEventsAsync();

            if (result == null)
                return BadRequest("No Events found");

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetEvent(Guid Id)
        {

        }
    }
}
