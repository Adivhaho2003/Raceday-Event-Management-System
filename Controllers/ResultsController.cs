using Microsoft.AspNetCore.Mvc;
using RaceDay_Management_System.Database;
using RaceDay_Management_System;

namespace RaceDay_Management_System.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ResultsController : ControllerBase
    {
        private readonly ResultDatabase _resultDb = new ResultDatabase();
        private readonly EventDatabase _eventDb = new EventDatabase();
        private readonly EnrolmentDatabase _enrolmentDb = new EnrolmentDatabase();

        // ============================================================
        // GET: api/results
        // Public
        // ============================================================
        [HttpGet]
        public IActionResult GetAllResults()
        {
            var results = _resultDb.get_all_results();
            return Ok(results);
        }

        // ============================================================
        // GET: api/results/{id}
        // Public
        // ============================================================
        [HttpGet("{id}")]
        public IActionResult GetResult(int id)
        {
            Result result = _resultDb.get_result_by_id(id);

            if (result == null)
            {
                return NotFound(new { message = "Result not found" });
            }

            return Ok(result);
        }

        // ============================================================
        // POST: api/results
        // Role Required: Organiser (owner of event)
        // ============================================================
        [HttpPost]
        public IActionResult CreateResult(int enrolment_id,
                                           string gun_time,
                                           string chip_time,
                                           int? overall_position,
                                           int? category_position,
                                           double? pace,
                                           string status)
        {
            int? current_user_id = HttpContext.Session.GetInt32("UserId");
            string? current_role = HttpContext.Session.GetString("Role");

            if (current_user_id == null)
            {
                return Unauthorized(new { message = "Not authenticated" });
            }

            if (current_role != "Organiser")
            {
                return StatusCode(403, new { message = "Forbidden - Organisers only" });
            }

            // Validate status
            string[] valid_statuses = { "DNS", "DNF", "Completed" };
            if (!valid_statuses.Contains(status))
            {
                return BadRequest(new { message = "Invalid status. Must be one of: DNS, DNF, Completed" });
            }

            // Fetch the enrolment
            Enrolment en = _resultDb.get_enrolment_by_id(enrolment_id);

            if (en == null)
            {
                return NotFound(new { message = "Enrolment not found" });
            }

            // Verify event exists and user owns it
            Event ev = _eventDb.get_event_by_id(en.EventId);

            if (ev == null)
            {
                return NotFound(new { message = "Event not found" });
            }

            if (ev.OrganiserId != current_user_id)
            {
                return StatusCode(403, new { message = "Forbidden - not your event" });
            }

            // Check for existing result
            if (_resultDb.result_exists_for_enrolment(enrolment_id))
            {
                return Conflict(new { message = "Result already exists for this enrolment" });
            }

            // We need category_id — get it via the event category link
            var eventCategories = new EventCategoryDatabase().get_event_categories(en.EventId);
            var target = eventCategories.FirstOrDefault(ec => ec.EventCategoryId == en.EventCategoryId);

            if (target == null)
            {
                return BadRequest(new { message = "Category link not found for this enrolment" });
            }

            string result = _resultDb.create_result(enrolment_id, en.ParticipantId, en.EventId, target.CategoryId,
                                                     gun_time, chip_time, overall_position, category_position,
                                                     pace, status);

            if (result.Contains("successfully"))
            {
                return Created("", new { message = result });
            }

            return BadRequest(new { message = result });
        }

        // ============================================================
        // PUT: api/results/{id}
        // Role Required: Organiser (owner of event)
        // ============================================================
        [HttpPut("{id}")]
        public IActionResult UpdateResult(int id,
                                           string gun_time,
                                           string chip_time,
                                           int? overall_position,
                                           int? category_position,
                                           double? pace,
                                           string status)
        {
            int? current_user_id = HttpContext.Session.GetInt32("UserId");
            string? current_role = HttpContext.Session.GetString("Role");

            if (current_user_id == null)
            {
                return Unauthorized(new { message = "Not authenticated" });
            }

            if (current_role != "Organiser")
            {
                return StatusCode(403, new { message = "Forbidden - Organisers only" });
            }

            Result existing = _resultDb.get_result_by_id(id);

            if (existing == null)
            {
                return NotFound(new { message = "Result not found" });
            }

            // Verify ownership
            Event ev = _eventDb.get_event_by_id(existing.EventId);

            if (ev == null || ev.OrganiserId != current_user_id)
            {
                return StatusCode(403, new { message = "Forbidden - not your event" });
            }

            // Validate status
            string[] valid_statuses = { "DNS", "DNF", "Completed" };
            if (!valid_statuses.Contains(status))
            {
                return BadRequest(new { message = "Invalid status. Must be one of: DNS, DNF, Completed" });
            }

            string result = _resultDb.update_result(id, gun_time, chip_time,
                                                     overall_position, category_position,
                                                     pace, status);

            if (result.Contains("successfully"))
            {
                return Ok(new { message = result });
            }

            return BadRequest(new { message = result });
        }

        // ============================================================
        // DELETE: api/results/{id}
        // Role Required: Organiser (owner of event)
        // ============================================================
        [HttpDelete("{id}")]
        public IActionResult DeleteResult(int id)
        {
            int? current_user_id = HttpContext.Session.GetInt32("UserId");
            string? current_role = HttpContext.Session.GetString("Role");

            if (current_user_id == null)
            {
                return Unauthorized(new { message = "Not authenticated" });
            }

            if (current_role != "Organiser")
            {
                return StatusCode(403, new { message = "Forbidden - Organisers only" });
            }

            Result existing = _resultDb.get_result_by_id(id);

            if (existing == null)
            {
                return NotFound(new { message = "Result not found" });
            }

            Event ev = _eventDb.get_event_by_id(existing.EventId);

            if (ev == null || ev.OrganiserId != current_user_id)
            {
                return StatusCode(403, new { message = "Forbidden - not your event" });
            }

            string result = _resultDb.delete_result(id);

            if (result.Contains("successfully"))
            {
                return NoContent();
            }

            return BadRequest(new { message = result });
        }

        // ============================================================
        // GET: api/results/event/{eventId}/leaderboard
        // Public
        // ============================================================
        [HttpGet("event/{eventId}/leaderboard")]
        public IActionResult GetLeaderboard(int eventId, int? category_id = null, int limit = 10)
        {
            Event ev = _eventDb.get_event_by_id(eventId);

            if (ev == null)
            {
                return NotFound(new { message = "Event not found" });
            }

            if (limit < 1 || limit > 500) limit = 10;

            var leaderboard = _resultDb.get_leaderboard(eventId, category_id, limit);
            return Ok(leaderboard);
        }

        // ============================================================
        // GET: api/results/participant/{participantId}
        // Role Required: Participant (own) / Organiser / Public
        // ============================================================
        [HttpGet("participant/{participantId}")]
        public IActionResult GetResultsByParticipant(int participantId)
        {
            var results = _resultDb.get_results_by_participant(participantId);
            return Ok(results);
        }
    }
}