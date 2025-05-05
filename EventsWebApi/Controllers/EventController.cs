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

            return CreatedAtAction(nameof(GetEvent), new { id = result.Id }, result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllEvents()
        {
            var result = await _eventService.GetAllEventsAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetEvent(Guid Id)
        {
            
            var result = await _eventService.GetEventByIdAsync(Id);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEvent(Guid Id)
        {
            var result = await _eventService.DeleteEventAsync(Id);

            if (!result)
                return NotFound();

            return Ok();
        }

        [HttpPut]
        public async Task<IActionResult> UpdateEvent(UpdateEventRequest requestData)
        {
            var result = await _eventService.UpdateEventAsync(requestData);

            if (result == null)
                return NotFound();

            return Ok(result);
        }
    }
}
