using Microsoft.AspNetCore.Mvc;
using RaceDay_Management_System.Database;
using RaceDay_Management_System;

namespace RaceDay_Management_System.Controllers
{
    /// <summary>
    /// Event management endpoints: browse, create, update, delete events.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class EventsController : ControllerBase
    {
        private readonly EventDatabase _eventDb = new EventDatabase();

        /// <summary>
        /// Gets all events. Public endpoint — no authentication required.
        /// </summary>
        /// <response code="200">Array of all events with organiser info.</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetAllEvents()
        {
            return Ok(_eventDb.get_all_events());
        }

        /// <summary>
        /// Gets a single event by its ID.
        /// </summary>
        /// <param name="id">The event ID.</param>
        /// <response code="200">Event details returned.</response>
        /// <response code="404">Event not found.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetEvent(int id)
        {
            Event ev = _eventDb.get_event_by_id(id);
            if (ev == null) return NotFound(new { message = "Event not found" });
            return Ok(ev);
        }

        /// <summary>
        /// Creates a new event. Restricted to Organisers.
        /// </summary>
        /// <param name="event_name">The event name.</param>
        /// <param name="description">Event description.</param>
        /// <param name="location">Event location (city / area).</param>
        /// <param name="venue">Specific venue.</param>
        /// <param name="event_date">Event date (yyyy-MM-dd HH:mm:ss).</param>
        /// <param name="registration_deadline">Deadline for registration.</param>
        /// <param name="entry_fee">Entry fee in ZAR.</param>
        /// <param name="max_participants">Maximum number of participants.</param>
        /// <param name="event_type">One of: Running, Walking, Cycling.</param>
        /// <param name="distance">Distance (e.g., 42.2 km).</param>
        /// <response code="201">Event created successfully.</response>
        /// <response code="400">Validation errors (bad event_type, etc.).</response>
        /// <response code="401">Not authenticated.</response>
        /// <response code="403">Not an Organiser.</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public IActionResult CreateEvent(string event_name, string description, string location, string venue,
                                          string event_date, string registration_deadline,
                                          decimal entry_fee, int max_participants,
                                          string event_type, string distance)
        {
            int? current_user_id = HttpContext.Session.GetInt32("UserId");
            string? current_role = HttpContext.Session.GetString("Role");

            if (current_user_id == null) return Unauthorized(new { message = "Not authenticated" });
            if (current_role != "Organiser") return StatusCode(403, new { message = "Forbidden - Organisers only" });

            if (event_type != "Running" && event_type != "Walking" && event_type != "Cycling")
                return BadRequest(new { message = "EventType must be 'Running', 'Walking', or 'Cycling'" });

            string result = _eventDb.create_event(event_name, description, current_user_id.Value,
                                                    location, venue, event_date, registration_deadline,
                                                    entry_fee, max_participants, event_type, distance);

            if (result.Contains("successfully")) return Created("", new { message = result });
            return BadRequest(new { message = result });
        }

        /// <summary>
        /// Updates an existing event. Restricted to the event's owner (Organiser).
        /// </summary>
        /// <param name="id">The event ID.</param>
        /// <param name="event_name">Updated event name.</param>
        /// <param name="description">Updated description.</param>
        /// <param name="location">Updated location.</param>
        /// <param name="venue">Updated venue.</param>
        /// <param name="event_date">Updated event date.</param>
        /// <param name="registration_deadline">Updated registration deadline.</param>
        /// <param name="entry_fee">Updated entry fee.</param>
        /// <param name="max_participants">Updated max participants.</param>
        /// <param name="status">Updated status (Pending, Open, Closed, Cancelled, Completed).</param>
        /// <response code="200">Event updated successfully.</response>
        /// <response code="400">Validation errors.</response>
        /// <response code="401">Not authenticated.</response>
        /// <response code="403">Not the event owner.</response>
        /// <response code="404">Event not found.</response>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult UpdateEvent(int id, string event_name, string description, string location, string venue,
                                          string event_date, string registration_deadline,
                                          decimal entry_fee, int max_participants, string status)
        {
            int? current_user_id = HttpContext.Session.GetInt32("UserId");
            string? current_role = HttpContext.Session.GetString("Role");

            if (current_user_id == null) return Unauthorized(new { message = "Not authenticated" });
            if (current_role != "Organiser") return StatusCode(403, new { message = "Forbidden - Organisers only" });

            Event existing = _eventDb.get_event_by_id(id);
            if (existing == null) return NotFound(new { message = "Event not found" });
            if (existing.OrganiserId != current_user_id)
                return StatusCode(403, new { message = "Forbidden - not your event" });

            string result = _eventDb.update_event(id, event_name, description, location, venue,
                                                    event_date, registration_deadline,
                                                    entry_fee, max_participants, status);

            if (result.Contains("successfully")) return Ok(new { message = result });
            return BadRequest(new { message = result });
        }

        /// <summary>
        /// Updates only the status of an event. Restricted to the owner (Organiser).
        /// </summary>
        /// <param name="id">The event ID.</param>
        /// <param name="status">New status (Pending, Open, Closed, Cancelled, Completed).</param>
        /// <response code="200">Event status updated.</response>
        /// <response code="400">Invalid status value.</response>
        /// <response code="401">Not authenticated.</response>
        /// <response code="403">Not the event owner.</response>
        /// <response code="404">Event not found.</response>
        [HttpPatch("{id}/status")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult UpdateEventStatus(int id, string status)
        {
            int? current_user_id = HttpContext.Session.GetInt32("UserId");
            string? current_role = HttpContext.Session.GetString("Role");

            if (current_user_id == null) return Unauthorized(new { message = "Not authenticated" });
            if (current_role != "Organiser") return StatusCode(403, new { message = "Forbidden - Organisers only" });

            Event existing = _eventDb.get_event_by_id(id);
            if (existing == null) return NotFound(new { message = "Event not found" });
            if (existing.OrganiserId != current_user_id)
                return StatusCode(403, new { message = "Forbidden - not your event" });

            string[] valid = { "Pending", "Open", "Closed", "Cancelled", "Completed" };
            if (!valid.Contains(status))
                return BadRequest(new { message = "Invalid status. Must be one of: Pending, Open, Closed, Cancelled, Completed" });

            string result = _eventDb.update_event_status(id, status);
            if (result.Contains("successfully")) return Ok(new { message = result });
            return BadRequest(new { message = result });
        }

        /// <summary>
        /// Deletes an event. Restricted to the owner (Organiser).
        /// </summary>
        /// <param name="id">The event ID.</param>
        /// <response code="204">Event deleted.</response>
        /// <response code="401">Not authenticated.</response>
        /// <response code="403">Not the event owner.</response>
        /// <response code="404">Event not found.</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult DeleteEvent(int id)
        {
            int? current_user_id = HttpContext.Session.GetInt32("UserId");
            string? current_role = HttpContext.Session.GetString("Role");

            if (current_user_id == null) return Unauthorized(new { message = "Not authenticated" });
            if (current_role != "Organiser") return StatusCode(403, new { message = "Forbidden - Organisers only" });

            Event existing = _eventDb.get_event_by_id(id);
            if (existing == null) return NotFound(new { message = "Event not found" });
            if (existing.OrganiserId != current_user_id)
                return StatusCode(403, new { message = "Forbidden - not your event" });

            string result = _eventDb.delete_event(id);
            if (result.Contains("successfully")) return NoContent();
            return BadRequest(new { message = result });
        }
    }
}