using Microsoft.AspNetCore.Mvc;
using RaceDay_Management_System.Database;
using RaceDay_Management_System;
using RaceDay_Management_System.Enums;
using System.Security.Cryptography;
using System.Text;

namespace RaceDay_Management_System.Controllers
{
    /// <summary>
    /// Authentication endpoints: user registration, login, logout, and profile management.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthDatabase _authDb = new AuthDatabase();

        /// <summary>
        /// Registers a new user account with the selected role (Participant or Organiser).
        /// </summary>
        /// <param name="email">The user's email address (must be unique).</param>
        /// <param name="password">The password (minimum 6 characters).</param>
        /// <param name="confirm_password">Must match the password.</param>
        /// <param name="first_name">User's first name.</param>
        /// <param name="last_name">User's last name.</param>
        /// <param name="phone">Contact phone number (optional).</param>
        /// <param name="date_of_birth">Date of birth in yyyy-MM-dd format (optional).</param>
        /// <param name="role">Role: Participant or Organiser.</param>
        /// <response code="201">User registered successfully.</response>
        /// <response code="400">Passwords do not match, or email/password missing.</response>
        /// <response code="409">Email already registered.</response>
        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public IActionResult Register(string email, string password, string confirm_password,
                                       string first_name, string last_name, string phone,
                                       string date_of_birth, UserRole role)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                return BadRequest(new { message = "Email and password are required" });
            }

            if (password != confirm_password)
            {
                return BadRequest(new { message = "Passwords do not match" });
            }

            if (_authDb.email_exists(email))
            {
                return Conflict(new { message = "Email already registered" });
            }

            string password_hash = HashPassword(password);

            string result = _authDb.register_user(email, password_hash, first_name, last_name,
                                                   phone, date_of_birth, role.ToString());

            if (result.Contains("successfully"))
            {
                return Created("", new { message = result });
            }

            return BadRequest(new { message = result });
        }

        /// <summary>
        /// Authenticates a user and establishes a session.
        /// </summary>
        /// <param name="email">The user's registered email address.</param>
        /// <param name="password">The user's password.</param>
        /// <response code="200">Login successful — returns user details and sets session.</response>
        /// <response code="400">Email or password missing.</response>
        /// <response code="401">Invalid email or password.</response>
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult Login(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                return BadRequest(new { message = "Email and password are required" });
            }

            User user = _authDb.login_user(email);

            if (user == null)
            {
                return Unauthorized(new { message = "Invalid email or password" });
            }

            if (!VerifyPassword(password, user.PasswordHash))
            {
                return Unauthorized(new { message = "Invalid email or password" });
            }

            HttpContext.Session.SetInt32("UserId", user.UserId);
            HttpContext.Session.SetString("Role", user.Role);
            HttpContext.Session.SetString("Email", user.Email);

            return Ok(new
            {
                userId = user.UserId,
                email = user.Email,
                firstName = user.FirstName,
                lastName = user.LastName,
                role = user.Role,
                message = "Login successful"
            });
        }

        /// <summary>
        /// Logs out the current user and clears the session.
        /// </summary>
        /// <response code="200">Logout successful.</response>
        [HttpPost("logout")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return Ok(new { message = "Logged out successfully" });
        }

        /// <summary>
        /// Returns the currently logged-in user's profile.
        /// </summary>
        /// <response code="200">User profile returned.</response>
        /// <response code="401">Not authenticated.</response>
        /// <response code="404">User not found.</response>
        [HttpGet("me")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetCurrentUser()
        {
            int? user_id = HttpContext.Session.GetInt32("UserId");

            if (user_id == null)
            {
                return Unauthorized(new { message = "Not authenticated" });
            }

            UserDatabase userDb = new UserDatabase();
            User user = userDb.get_user_by_id(user_id.Value);

            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            return Ok(new
            {
                userId = user.UserId,
                email = user.Email,
                firstName = user.FirstName,
                lastName = user.LastName,
                phone = user.Phone,
                dateOfBirth = user.DateOfBirth,
                role = user.Role,
                profilePictureUrl = user.ProfilePictureUrl
            });
        }

        /// <summary>
        /// Updates the currently logged-in user's profile.
        /// </summary>
        /// <param name="first_name">New first name.</param>
        /// <param name="last_name">New last name.</param>
        /// <param name="phone">New phone number.</param>
        /// <param name="date_of_birth">New date of birth (yyyy-MM-dd).</param>
        /// <response code="200">Profile updated.</response>
        /// <response code="400">Validation errors.</response>
        /// <response code="401">Not authenticated.</response>
        [HttpPut("me")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult UpdateProfile(string first_name, string last_name, string phone, string date_of_birth)
        {
            int? user_id = HttpContext.Session.GetInt32("UserId");

            if (user_id == null)
            {
                return Unauthorized(new { message = "Not authenticated" });
            }

            UserDatabase userDb = new UserDatabase();
            string result = userDb.update_user_profile(user_id.Value, first_name, last_name, phone, date_of_birth);

            if (result.Contains("successfully"))
            {
                return Ok(new { message = result });
            }

            return BadRequest(new { message = result });
        }

        /// <summary>
        /// Changes the currently logged-in user's password.
        /// </summary>
        /// <param name="current_password">The existing password for verification.</param>
        /// <param name="new_password">The new password.</param>
        /// <param name="confirm_new_password">Must match new password.</param>
        /// <response code="200">Password updated.</response>
        /// <response code="400">Current password incorrect or new passwords mismatch.</response>
        /// <response code="401">Not authenticated.</response>
        /// <response code="404">User not found.</response>
        [HttpPut("password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult ChangePassword(string current_password, string new_password, string confirm_new_password)
        {
            int? user_id = HttpContext.Session.GetInt32("UserId");

            if (user_id == null)
            {
                return Unauthorized(new { message = "Not authenticated" });
            }

            if (new_password != confirm_new_password)
            {
                return BadRequest(new { message = "New passwords do not match" });
            }

            UserDatabase userDb = new UserDatabase();
            User user = userDb.get_user_by_id(user_id.Value);

            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            if (!VerifyPassword(current_password, user.PasswordHash))
            {
                return BadRequest(new { message = "Current password is incorrect" });
            }

            string new_hash = HashPassword(new_password);
            string result = _authDb.update_password(user_id.Value, new_hash);

            if (result.Contains("successfully"))
            {
                return Ok(new { message = result });
            }

            return BadRequest(new { message = result });
        }

        // PASSWORD HASHING HELPERS
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }

        private bool VerifyPassword(string password, string hash)
        {
            return HashPassword(password) == hash;
        }
    }
}