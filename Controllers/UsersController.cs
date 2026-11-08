using Microsoft.AspNetCore.Mvc;
using RaceDay_Management_System.Database;
using RaceDay_Management_System;

namespace RaceDay_Management_System.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly UserDatabase _userDb = new UserDatabase();

        // GET: api/users
        [HttpGet]
        public IActionResult GetAllUsers(int page = 1, int size = 10, string? role = null, string? search = null)
        {
            int? current_user_id = HttpContext.Session.GetInt32("UserId");
            string? current_role = HttpContext.Session.GetString("Role");

            if (current_user_id == null) return Unauthorized(new { message = "Not authenticated" });
            if (current_role != "Organiser" && current_role != "Admin") return StatusCode(403, new { message = "Forbidden - insufficient permissions" });

            if (page < 1) page = 1;
            if (size < 1 || size > 100) size = 10;

            var users = _userDb.get_all_users(page, size, role, search);

            return Ok(new { page = page, size = size, count = users.Count, users = users });
        }

        // GET: api/users/{id}
        [HttpGet("{id}")]
        public IActionResult GetUser(int id)
        {
            int? current_user_id = HttpContext.Session.GetInt32("UserId");
            string? current_role = HttpContext.Session.GetString("Role");

            if (current_user_id == null) return Unauthorized(new { message = "Not authenticated" });
            if (current_user_id != id && current_role != "Admin") return StatusCode(403, new { message = "Forbidden - cannot view other users" });

            User user = _userDb.get_user_by_id(id);
            if (user == null) return NotFound(new { message = "User not found" });

            return Ok(new
            {
                userId = user.UserId,
                email = user.Email,
                firstName = user.FirstName,
                lastName = user.LastName,
                phone = user.Phone,
                dateOfBirth = user.DateOfBirth,
                role = user.Role,
                profilePictureUrl = user.ProfilePictureUrl,
                createdAt = user.CreatedAt
            });
        }

        // GET: api/users/{id}/events
        [HttpGet("{id}/events")]
        public IActionResult GetUserEvents(int id)
        {
            int? current_user_id = HttpContext.Session.GetInt32("UserId");
            if (current_user_id == null) return Unauthorized(new { message = "Not authenticated" });

            User user = _userDb.get_user_by_id(id);
            if (user == null) return NotFound(new { message = "User not found" });
            if (user.Role != "Organiser") return BadRequest(new { message = "This user is not an Organiser" });

            return Ok(_userDb.get_events_by_organiser(id));
        }

        // GET: api/users/{id}/enrolments
        [HttpGet("{id}/enrolments")]
        public IActionResult GetUserEnrolments(int id)
        {
            int? current_user_id = HttpContext.Session.GetInt32("UserId");
            string? current_role = HttpContext.Session.GetString("Role");

            if (current_user_id == null) return Unauthorized(new { message = "Not authenticated" });

            User user = _userDb.get_user_by_id(id);
            if (user == null) return NotFound(new { message = "User not found" });
            if (current_role == "Participant" && current_user_id != id) return StatusCode(403, new { message = "Forbidden - cannot view other users' enrolments" });

            return Ok(_userDb.get_enrolments_by_participant(id));
        }

        // GET: api/users/{id}/results
        [HttpGet("{id}/results")]
        public IActionResult GetUserResults(int id)
        {
            int? current_user_id = HttpContext.Session.GetInt32("UserId");
            if (current_user_id == null) return Unauthorized(new { message = "Not authenticated" });

            User user = _userDb.get_user_by_id(id);
            if (user == null) return NotFound(new { message = "User not found" });

            return Ok(_userDb.get_results_by_participant(id));
        }

        // PUT: api/users/{id}
        [HttpPut("{id}")]
        public IActionResult UpdateUser(int id, string first_name, string last_name, string phone, string date_of_birth)
        {
            int? current_user_id = HttpContext.Session.GetInt32("UserId");
            if (current_user_id == null) return Unauthorized(new { message = "Not authenticated" });
            if (current_user_id != id) return StatusCode(403, new { message = "Forbidden - not your profile" });

            User existing = _userDb.get_user_by_id(id);
            if (existing == null) return NotFound(new { message = "User not found" });

            string result = _userDb.update_user_profile(id, first_name, last_name, phone, date_of_birth);

            if (result.Contains("successfully")) return Ok(new { message = result });
            return BadRequest(new { message = result });
        }

        // POST: api/users/{id}/profile-picture
        [HttpPost("{id}/profile-picture")]
        public IActionResult UploadProfilePicture(int id, string? profile_picture_url = null, IFormFile? file = null)
        {
            int? current_user_id = HttpContext.Session.GetInt32("UserId");
            if (current_user_id == null) return Unauthorized(new { message = "Not authenticated" });
            if (current_user_id != id) return StatusCode(403, new { message = "Forbidden - not your profile" });

            User existing = _userDb.get_user_by_id(id);
            if (existing == null) return NotFound(new { message = "User not found" });

            string? finalUrl = profile_picture_url;

            // If a file was uploaded directly to the API (legacy)
            if (string.IsNullOrEmpty(finalUrl) && file != null && file.Length > 0)
            {
                string[] allowed_extensions = { ".jpg", ".jpeg", ".png", ".gif" };
                string extension = Path.GetExtension(file.FileName).ToLower();

                if (!allowed_extensions.Contains(extension))
                    return BadRequest(new { message = "Invalid file format. Allowed: jpg, jpeg, png, gif" });

                finalUrl = $"https://placeholder.local/profile-pictures/{id}_{Guid.NewGuid()}{extension}";
            }

            if (string.IsNullOrEmpty(finalUrl))
                return BadRequest(new { message = "No file or URL provided" });

            string result = _userDb.update_profile_picture(id, finalUrl);

            if (result.Contains("successfully"))
            {
                return Ok(new { message = result, profilePictureUrl = finalUrl });
            }

            return BadRequest(new { message = result });
        }

        // DELETE: api/users/{id}/profile-picture
        [HttpDelete("{id}/profile-picture")]
        public IActionResult RemoveProfilePicture(int id)
        {
            int? current_user_id = HttpContext.Session.GetInt32("UserId");
            if (current_user_id == null) return Unauthorized(new { message = "Not authenticated" });
            if (current_user_id != id) return StatusCode(403, new { message = "Forbidden - not your profile" });

            User existing = _userDb.get_user_by_id(id);
            if (existing == null) return NotFound(new { message = "User not found" });

            string result = _userDb.update_profile_picture(id, null);

            if (result.Contains("successfully")) return NoContent();
            return BadRequest(new { message = result });
        }
    }
}