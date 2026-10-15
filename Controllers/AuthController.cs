using Microsoft.AspNetCore.Mvc;
using RaceDay_Management_System.Database;
using RaceDay_Management_System.Enums;
using System.Security.Cryptography;
using System.Text;

namespace RaceDay_Management_System.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthDatabase _authDb = new AuthDatabase();

        // ============================================================
        // POST: api/auth/register
        // ============================================================
        [HttpPost("register")]
        public IActionResult Register(string email, string password, string confirm_password,
                                       string first_name, string last_name, string phone,
                                       string date_of_birth, UserRole role)   // ← changed
        {
            // Validate passwords match
            if (password != confirm_password)
            {
                return BadRequest(new { message = "Passwords do not match" });
            }

            // No need to validate role anymore — enum guarantees it's only Participant or Organiser

            // Check if email already exists
            if (_authDb.email_exists(email))
            {
                return Conflict(new { message = "Email already registered" });
            }

            // Hash password
            string password_hash = HashPassword(password);

            // Register (convert enum to string for the DB)
            string result = _authDb.register_user(email, password_hash, first_name, last_name,
                                                   phone, date_of_birth, role.ToString());

            if (result.Contains("successfully"))
            {
                return Created("", new { message = result });
            }

            return BadRequest(new { message = result });
        }

        // ============================================================
        // POST: api/auth/login
        // ============================================================
        [HttpPost("login")]
        public IActionResult Login(string email, string password)
        {
            User user = _authDb.login_user(email);

            if (user == null)
            {
                return Unauthorized(new { message = "Invalid email or password" });
            }

            if (!VerifyPassword(password, user.PasswordHash))
            {
                return Unauthorized(new { message = "Invalid email or password" });
            }

            // Store in session
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

        // ============================================================
        // POST: api/auth/logout
        // ============================================================
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return Ok(new { message = "Logged out successfully" });
        }

        // ============================================================
        // GET: api/auth/me
        // ============================================================
        [HttpGet("me")]
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
                role = user.Role
            });
        }

        // ============================================================
        // PUT: api/auth/me
        // ============================================================
        [HttpPut("me")]
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

        // ============================================================
        // PUT: api/auth/password
        // ============================================================
        [HttpPut("password")]
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

        // ============================================================
        // PASSWORD HASHING HELPERS
        // ============================================================
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