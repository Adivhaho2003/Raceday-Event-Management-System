using Microsoft.AspNetCore.Mvc;
using RaceDay_Management_System.Database;
using RaceDay_Management_System;

namespace RaceDay_Management_System.Controllers
{
    /// <summary>
    /// Race result endpoints: view results, leaderboards, and manage results (Organisers only).
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ResultsController : ControllerBase
    {
        private readonly ResultDatabase _resultDb = new ResultDatabase();
        private readonly EventDatabase _eventDb = new EventDatabase();
        private readonly EventCategoryDatabase _eventCategoryDb = new EventCategoryDatabase();

        /// <summary>
        /// Gets all results. Public endpoint.
        /// </summary>
        /// <response code="200">Array of all results.</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetAllResults()
        {
            return Ok(_resultDb.get_all_results());
        }

        /// <summary>
        /// Gets a single result by ID. Public endpoint.
        /// </summary>
        /// <param name="id">The result ID.</param>
        /// <response code="200">Result returned.</response>
        /// <response code="404">Result not found.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetResult(int id)
        {
            Result result = _resultDb.get_result_by_id(id);
            if (result == null) return NotFound(new { message = "Result not found" });
            return Ok(result);
        }

        /// <summary>
        /// Creates a new result. Restricted to the event owner (Organiser).
        /// </summary>
        /// <param name="enrolment_id">The enrolment ID to record a result for.</param>
        /// <param name="gun_time">Gun time (HH:mm:ss).</param>
        /// <param name="chip_time">Chip time (HH:mm:ss).</param>
        /// <param name="overall_position">Overall finishing position.</param>
        /// <param name="category_position">Finishing position within category.</param>
        /// <param name="pace">Pace (minutes per km or km/h).</param>
        /// <param name="status">One of: DNS, DNF, Completed.</param>
        /// <response code="201">Result created.</response>
        /// <response code="400">Validation errors.</response>
        /// <response code="401">Not authenticated.</response>
        /// <response code="403">Not the event owner.</response>
        /// <response code="404">Enrolment or event not found.</response>
        /// <response code="409">Result already exists for this enrolment.</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public IActionResult CreateResult(int enrolment_id, string gun_time, string chip_time,
                                           int? overall_position, int? category_position,
                                           double? pace, string status)
        {
            int? current_user_id = HttpContext.Session.GetInt32("UserId");
            string? current_role = HttpContext.Session.GetString("Role");

            if (current_user_id == null) return Unauthorized(new { message = "Not authenticated" });
            if (current_role != "Organiser") return StatusCode(403, new { message = "Forbidden - Organisers only" });

            string[] valid_status = { "DNS", "DNF", "Completed" };
            if (!valid_status.Contains(status))
                return BadRequest(new { message = "Invalid status. Must be one of: DNS, DNF, Completed" });

            Enrolment en = _resultDb.get_enrolment_by_id(enrolment_id);
            if (en == null) return NotFound(new { message = "Enrolment not found" });

            Event ev = _eventDb.get_event_by_id(en.EventId);
            if (ev == null) return NotFound(new { message = "Event not found" });
            if (ev.OrganiserId != current_user_id)
                return StatusCode(403, new { message = "Forbidden - not your event" });

            if (_resultDb.result_exists_for_enrolment(enrolment_id))
                return Conflict(new { message = "Result already exists for this enrolment" });

            var eventCategories = _eventCategoryDb.get_event_categories(en.EventId);
            var target = eventCategories.FirstOrDefault(ec => ec.EventCategoryId == en.EventCategoryId);
            if (target == null) return BadRequest(new { message = "Category link not found for this enrolment" });

            string result = _resultDb.create_result(enrolment_id, en.ParticipantId, en.EventId, target.CategoryId,
                                                     gun_time, chip_time, overall_position, category_position,
                                                     pace, status);
            if (result.Contains("successfully")) return Created("", new { message = result });
            return BadRequest(new { message = result });
        }

        /// <summary>
        /// Updates an existing result. Restricted to the event owner (Organiser).
        /// </summary>
        /// <param name="id">The result ID.</param>
        /// <param name="gun_time">Updated gun time.</param>
        /// <param name="chip_time">Updated chip time.</param>
        /// <param name="overall_position">Updated overall position.</param>
        /// <param name="category_position">Updated category position.</param>
        /// <param name="pace">Updated pace.</param>
        /// <param name="status">Updated status (DNS, DNF, Completed).</param>
        /// <response code="200">Result updated.</response>
        /// <response code="400">Validation errors.</response>
        /// <response code="401">Not authenticated.</response>
        /// <response code="403">Not the event owner.</response>
        /// <response code="404">Result not found.</response>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult UpdateResult(int id, string gun_time, string chip_time,
                                           int? overall_position, int? category_position,
                                           double? pace, string status)
        {
            int? current_user_id = HttpContext.Session.GetInt32("UserId");
            string? current_role = HttpContext.Session.GetString("Role");

            if (current_user_id == null) return Unauthorized(new { message = "Not authenticated" });
            if (current_role != "Organiser") return StatusCode(403, new { message = "Forbidden - Organisers only" });

            Result existing = _resultDb.get_result_by_id(id);
            if (existing == null) return NotFound(new { message = "Result not found" });

            Event ev = _eventDb.get_event_by_id(existing.EventId);
            if (ev == null || ev.OrganiserId != current_user_id)
                return StatusCode(403, new { message = "Forbidden - not your event" });

            string[] valid_status = { "DNS", "DNF", "Completed" };
            if (!valid_status.Contains(status))
                return BadRequest(new { message = "Invalid status. Must be one of: DNS, DNF, Completed" });

            string result = _resultDb.update_result(id, gun_time, chip_time,
                                                     overall_position, category_position,
                                                     pace, status);
            if (result.Contains("successfully")) return Ok(new { message = result });
            return BadRequest(new { message = result });
        }

        /// <summary>
        /// Deletes a result. Restricted to the event owner (Organiser).
        /// </summary>
        /// <param name="id">The result ID.</param>
        /// <response code="204">Result deleted.</response>
        /// <response code="401">Not authenticated.</response>
        /// <response code="403">Not the event owner.</response>
        /// <response code="404">Result not found.</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult DeleteResult(int id)
        {
            int? current_user_id = HttpContext.Session.GetInt32("UserId");
            string? current_role = HttpContext.Session.GetString("Role");

            if (current_user_id == null) return Unauthorized(new { message = "Not authenticated" });
            if (current_role != "Organiser") return StatusCode(403, new { message = "Forbidden - Organisers only" });

            Result existing = _resultDb.get_result_by_id(id);
            if (existing == null) return NotFound(new { message = "Result not found" });

            Event ev = _eventDb.get_event_by_id(existing.EventId);
            if (ev == null || ev.OrganiserId != current_user_id)
                return StatusCode(403, new { message = "Forbidden - not your event" });

            string result = _resultDb.delete_result(id);
            if (result.Contains("successfully")) return NoContent();
            return BadRequest(new { message = result });
        }

        /// <summary>
        /// Gets the leaderboard for an event. Public endpoint.
        /// </summary>
        /// <param name="eventId">The event ID.</param>
        /// <param name="category_id">Optional category filter.</param>
        /// <param name="limit">Number of top results to return (1-500, default 10).</param>
        /// <response code="200">Leaderboard returned.</response>
        /// <response code="404">Event not found.</response>
        [HttpGet("event/{eventId}/leaderboard")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetLeaderboard(int eventId, int? category_id = null, int limit = 10)
        {
            Event ev = _eventDb.get_event_by_id(eventId);
            if (ev == null) return NotFound(new { message = "Event not found" });

            if (limit < 1 || limit > 500) limit = 10;

            return Ok(_resultDb.get_leaderboard(eventId, category_id, limit));
        }

        /// <summary>
        /// Gets all results for a specific participant. Public endpoint.
        /// </summary>
        /// <param name="participantId">The participant's user ID.</param>
        /// <response code="200">Array of participant's results.</response>
        [HttpGet("participant/{participantId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetResultsByParticipant(int participantId)
        {
            return Ok(_resultDb.get_results_by_participant(participantId));
        }
    }
}