using Microsoft.AspNetCore.Mvc;
using RaceDay_Management_System.Database;
using RaceDay_Management_System;

namespace RaceDay_Management_System.Controllers
{
    /// <summary>
    /// User profile endpoints: view and manage user profiles, enrolments, and results.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly UserDatabase _userDb = new UserDatabase();

        /// <summary>
        /// Gets a paginated list of all users. Restricted to Organisers and Admins.
        /// </summary>
        /// <param name="page">Page number (default 1).</param>
        /// <param name="size">Page size (1-100, default 10).</param>
        /// <param name="role">Optional role filter (Participant, Organiser, Admin).</param>
        /// <param name="search">Optional search text for name or email.</param>
        /// <response code="200">Paginated list of users returned.</response>
        /// <response code="401">Not authenticated.</response>
        /// <response code="403">Insufficient permissions.</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public IActionResult GetAllUsers(int page = 1, int size = 10, string? role = null, string? search = null)
        {
            int? current_user_id = HttpContext.Session.GetInt32("UserId");
            string? current_role = HttpContext.Session.GetString("Role");

            if (current_user_id == null)
            {
                return Unauthorized(new { message = "Not authenticated" });
            }

            if (current_role != "Organiser" && current_role != "Admin")
            {
                return StatusCode(403, new { message = "Forbidden - insufficient permissions" });
            }

            if (page < 1) page = 1;
            if (size < 1 || size > 100) size = 10;

            var users = _userDb.get_all_users(page, size, role, search);

            return Ok(new { page = page, size = size, count = users.Count, users = users });
        }

        /// <summary>
        /// Gets a specific user's profile by ID.
        /// </summary>
        /// <param name="id">The user's ID.</param>
        /// <response code="200">User profile returned.</response>
        /// <response code="401">Not authenticated.</response>
        /// <response code="403">Cannot view other users.</response>
        /// <response code="404">User not found.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetUser(int id)
        {
            int? current_user_id = HttpContext.Session.GetInt32("UserId");
            string? current_role = HttpContext.Session.GetString("Role");

            if (current_user_id == null) return Unauthorized(new { message = "Not authenticated" });

            if (current_user_id != id && current_role != "Admin")
                return StatusCode(403, new { message = "Forbidden - cannot view other users" });

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

        /// <summary>
        /// Gets all events created by an organiser.
        /// </summary>
        /// <param name="id">The organiser's user ID.</param>
        /// <response code="200">Array of events.</response>
        /// <response code="400">User is not an Organiser.</response>
        /// <response code="401">Not authenticated.</response>
        /// <response code="404">User not found.</response>
        [HttpGet("{id}/events")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetUserEvents(int id)
        {
            int? current_user_id = HttpContext.Session.GetInt32("UserId");
            if (current_user_id == null) return Unauthorized(new { message = "Not authenticated" });

            User user = _userDb.get_user_by_id(id);
            if (user == null) return NotFound(new { message = "User not found" });
            if (user.Role != "Organiser") return BadRequest(new { message = "This user is not an Organiser" });

            return Ok(_userDb.get_events_by_organiser(id));
        }

        /// <summary>
        /// Gets all enrolments for a participant.
        /// </summary>
        /// <param name="id">The participant's user ID.</param>
        /// <response code="200">Array of enrolments with event details.</response>
        /// <response code="401">Not authenticated.</response>
        /// <response code="403">Cannot view other users' enrolments.</response>
        /// <response code="404">User not found.</response>
        [HttpGet("{id}/enrolments")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetUserEnrolments(int id)
        {
            int? current_user_id = HttpContext.Session.GetInt32("UserId");
            string? current_role = HttpContext.Session.GetString("Role");

            if (current_user_id == null) return Unauthorized(new { message = "Not authenticated" });

            User user = _userDb.get_user_by_id(id);
            if (user == null) return NotFound(new { message = "User not found" });

            if (current_role == "Participant" && current_user_id != id)
                return StatusCode(403, new { message = "Forbidden - cannot view other users' enrolments" });

            return Ok(_userDb.get_enrolments_by_participant(id));
        }

        /// <summary>
        /// Gets all race results for a participant.
        /// </summary>
        /// <param name="id">The participant's user ID.</param>
        /// <response code="200">Array of results with event and category details.</response>
        /// <response code="401">Not authenticated.</response>
        /// <response code="404">User not found.</response>
        [HttpGet("{id}/results")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetUserResults(int id)
        {
            int? current_user_id = HttpContext.Session.GetInt32("UserId");
            if (current_user_id == null) return Unauthorized(new { message = "Not authenticated" });

            User user = _userDb.get_user_by_id(id);
            if (user == null) return NotFound(new { message = "User not found" });

            return Ok(_userDb.get_results_by_participant(id));
        }

        /// <summary>
        /// Updates the currently logged-in user's profile.
        /// </summary>
        /// <param name="id">The user ID (must match logged-in user).</param>
        /// <param name="first_name">New first name.</param>
        /// <param name="last_name">New last name.</param>
        /// <param name="phone">New phone number.</param>
        /// <param name="date_of_birth">New date of birth (yyyy-MM-dd).</param>
        /// <response code="200">Profile updated.</response>
        /// <response code="400">Validation errors.</response>
        /// <response code="401">Not authenticated.</response>
        /// <response code="403">Not your profile.</response>
        /// <response code="404">User not found.</response>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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

        /// <summary>
        /// Uploads a profile picture for the user (Part 3 will wire this to Azure Blob Storage).
        /// </summary>
        /// <param name="id">The user ID (must match logged-in user).</param>
        /// <param name="file">The image file (jpg, jpeg, png, gif — max 5 MB).</param>
        /// <response code="200">Profile picture uploaded; returns URL.</response>
        /// <response code="400">Invalid file format or size.</response>
        /// <response code="401">Not authenticated.</response>
        /// <response code="403">Not your profile.</response>
        /// <response code="404">User not found.</response>
        [HttpPost("{id}/profile-picture")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult UploadProfilePicture(int id, IFormFile file)
        {
            int? current_user_id = HttpContext.Session.GetInt32("UserId");
            if (current_user_id == null) return Unauthorized(new { message = "Not authenticated" });
            if (current_user_id != id) return StatusCode(403, new { message = "Forbidden - not your profile" });

            User existing = _userDb.get_user_by_id(id);
            if (existing == null) return NotFound(new { message = "User not found" });

            if (file == null || file.Length == 0) return BadRequest(new { message = "No file provided" });

            string[] allowed = { ".jpg", ".jpeg", ".png", ".gif" };
            string ext = Path.GetExtension(file.FileName).ToLower();
            if (!allowed.Contains(ext))
                return BadRequest(new { message = "Invalid file format. Allowed: jpg, jpeg, png, gif" });

            if (file.Length > 5 * 1024 * 1024)
                return BadRequest(new { message = "File too large. Maximum 5 MB" });

            string placeholder_url = $"https://placeholder.local/profile-pictures/{id}_{Guid.NewGuid()}{ext}";
            string result = _userDb.update_profile_picture(id, placeholder_url);

            if (result.Contains("successfully"))
                return Ok(new { message = result, profilePictureUrl = placeholder_url });

            return BadRequest(new { message = result });
        }

        /// <summary>
        /// Removes the user's profile picture.
        /// </summary>
        /// <param name="id">The user ID (must match logged-in user).</param>
        /// <response code="204">Profile picture removed.</response>
        /// <response code="401">Not authenticated.</response>
        /// <response code="403">Not your profile.</response>
        /// <response code="404">User not found.</response>
        [HttpDelete("{id}/profile-picture")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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