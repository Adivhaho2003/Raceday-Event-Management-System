using Microsoft.AspNetCore.Mvc;
using RaceDay_Management_System.Database;
using RaceDay_Management_System;

namespace RaceDay_Management_System.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly CategoryDatabase _categoryDb = new CategoryDatabase();

        // ============================================================
        // GET: api/categories
        // Public — anyone can view categories
        // ============================================================
        [HttpGet]
        public IActionResult GetAllCategories()
        {
            var categories = _categoryDb.get_all_categories();
            return Ok(categories);
        }

        // ============================================================
        // GET: api/categories/{id}
        // Public
        // ============================================================
        [HttpGet("{id}")]
        public IActionResult GetCategory(int id)
        {
            Category cat = _categoryDb.get_category_by_id(id);

            if (cat == null)
            {
                return NotFound(new { message = "Category not found" });
            }

            return Ok(cat);
        }

        // ============================================================
        // POST: api/categories
        // Role Required: Admin / Organiser
        // ============================================================
        [HttpPost]
        public IActionResult CreateCategory(string category_name,
                                             string category_type,
                                             string distance,
                                             string age_group,
                                             string gender,
                                             decimal standard_fee)
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

            if (category_type != "Running" && category_type != "Walking" && category_type != "Cycling")
            {
                return BadRequest(new { message = "CategoryType must be 'Running', 'Walking', or 'Cycling'" });
            }

            if (!string.IsNullOrEmpty(gender))
            {
                string[] valid_genders = { "Male", "Female", "Mixed" };
                if (!valid_genders.Contains(gender))
                {
                    return BadRequest(new { message = "Gender must be 'Male', 'Female', or 'Mixed'" });
                }
            }

            string result = _categoryDb.create_category(category_name, category_type, distance,
                                                          age_group, gender, standard_fee);

            if (result.Contains("successfully"))
            {
                return Created("", new { message = result });
            }

            return BadRequest(new { message = result });
        }

        // ============================================================
        // PUT: api/categories/{id}
        // Role Required: Admin / Organiser
        // ============================================================
        [HttpPut("{id}")]
        public IActionResult UpdateCategory(int id,
                                             string category_name,
                                             string category_type,
                                             string distance,
                                             string age_group,
                                             string gender,
                                             decimal standard_fee)
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

            Category existing = _categoryDb.get_category_by_id(id);

            if (existing == null)
            {
                return NotFound(new { message = "Category not found" });
            }

            if (category_type != "Running" && category_type != "Walking" && category_type != "Cycling")
            {
                return BadRequest(new { message = "CategoryType must be 'Running', 'Walking', or 'Cycling'" });
            }

            string result = _categoryDb.update_category(id, category_name, category_type, distance,
                                                          age_group, gender, standard_fee);

            if (result.Contains("successfully"))
            {
                return Ok(new { message = result });
            }

            return BadRequest(new { message = result });
        }

        // ============================================================
        // DELETE: api/categories/{id}
        // Role Required: Admin
        // ============================================================
        [HttpDelete("{id}")]
        public IActionResult DeleteCategory(int id)
        {
            int? current_user_id = HttpContext.Session.GetInt32("UserId");
            string? current_role = HttpContext.Session.GetString("Role");

            if (current_user_id == null)
            {
                return Unauthorized(new { message = "Not authenticated" });
            }

            if (current_role != "Admin")
            {
                return StatusCode(403, new { message = "Forbidden - Admin only" });
            }

            Category existing = _categoryDb.get_category_by_id(id);

            if (existing == null)
            {
                return NotFound(new { message = "Category not found" });
            }

            string result = _categoryDb.delete_category(id);

            if (result.Contains("successfully"))
            {
                return NoContent();
            }

            return BadRequest(new { message = result });
        }
    }
}