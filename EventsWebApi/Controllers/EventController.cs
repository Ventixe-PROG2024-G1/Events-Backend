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
                return BadRequest("Failed to create an event");

            return CreatedAtAction(nameof(GetEvent), new { id = result.Id }, result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllEvents()
        {
            var result = await _eventService.GetAllEventsAsync();
            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetEvent(Guid Id)
        {
            
            var result = await _eventService.GetEventByIdAsync(Id);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteEvent(Guid Id)
        {
            var result = await _eventService.DeleteEventAsync(Id);

            if (!result)
                return NotFound();

            return NoContent();
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateEvent(Guid id, UpdateEventRequest requestData)
        {
            if (requestData.Id != Guid.Empty && id != requestData.Id)
            {
                return BadRequest("ID in URL does not match ID in request body.");
            }


            var result = await _eventService.UpdateEventAsync(id, requestData);

            if (result == null)
                return NotFound();

            return Ok(result);
        }
    }
}
