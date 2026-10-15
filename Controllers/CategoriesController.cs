using Microsoft.AspNetCore.Mvc;
using RaceDay_Management_System.Database;
using RaceDay_Management_System;

namespace RaceDay_Management_System.Controllers
{
    /// <summary>
    /// Category management endpoints: browse, create, update, delete race categories.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly CategoryDatabase _categoryDb = new CategoryDatabase();

        /// <summary>
        /// Gets all categories. Public endpoint.
        /// </summary>
        /// <response code="200">Array of all categories.</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetAllCategories()
        {
            return Ok(_categoryDb.get_all_categories());
        }

        /// <summary>
        /// Gets a single category by ID. Public endpoint.
        /// </summary>
        /// <param name="id">The category ID.</param>
        /// <response code="200">Category returned.</response>
        /// <response code="404">Category not found.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetCategory(int id)
        {
            Category cat = _categoryDb.get_category_by_id(id);
            if (cat == null) return NotFound(new { message = "Category not found" });
            return Ok(cat);
        }

        /// <summary>
        /// Creates a new category. Restricted to Organisers and Admins.
        /// </summary>
        /// <param name="category_name">Category name (e.g., "Marathon").</param>
        /// <param name="category_type">One of: Running, Walking, Cycling.</param>
        /// <param name="distance">Distance (e.g., "42.2 km").</param>
        /// <param name="age_group">Age group (e.g., "18+").</param>
        /// <param name="gender">One of: Male, Female, Mixed.</param>
        /// <param name="standard_fee">Standard fee in ZAR.</param>
        /// <response code="201">Category created.</response>
        /// <response code="400">Validation errors.</response>
        /// <response code="401">Not authenticated.</response>
        /// <response code="403">Insufficient permissions.</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public IActionResult CreateCategory(string category_name, string category_type, string distance,
                                             string age_group, string gender, decimal standard_fee)
        {
            int? current_user_id = HttpContext.Session.GetInt32("UserId");
            string? current_role = HttpContext.Session.GetString("Role");

            if (current_user_id == null) return Unauthorized(new { message = "Not authenticated" });
            if (current_role != "Organiser" && current_role != "Admin")
                return StatusCode(403, new { message = "Forbidden - insufficient permissions" });

            if (category_type != "Running" && category_type != "Walking" && category_type != "Cycling")
                return BadRequest(new { message = "CategoryType must be 'Running', 'Walking', or 'Cycling'" });

            if (!string.IsNullOrEmpty(gender))
            {
                string[] valid = { "Male", "Female", "Mixed" };
                if (!valid.Contains(gender))
                    return BadRequest(new { message = "Gender must be 'Male', 'Female', or 'Mixed'" });
            }

            string result = _categoryDb.create_category(category_name, category_type, distance,
                                                          age_group, gender, standard_fee);
            if (result.Contains("successfully")) return Created("", new { message = result });
            return BadRequest(new { message = result });
        }

        /// <summary>
        /// Updates an existing category. Restricted to Organisers and Admins.
        /// </summary>
        /// <param name="id">The category ID.</param>
        /// <param name="category_name">Updated name.</param>
        /// <param name="category_type">Updated type.</param>
        /// <param name="distance">Updated distance.</param>
        /// <param name="age_group">Updated age group.</param>
        /// <param name="gender">Updated gender.</param>
        /// <param name="standard_fee">Updated standard fee.</param>
        /// <response code="200">Category updated.</response>
        /// <response code="400">Validation errors.</response>
        /// <response code="401">Not authenticated.</response>
        /// <response code="403">Insufficient permissions.</response>
        /// <response code="404">Category not found.</response>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult UpdateCategory(int id, string category_name, string category_type, string distance,
                                             string age_group, string gender, decimal standard_fee)
        {
            int? current_user_id = HttpContext.Session.GetInt32("UserId");
            string? current_role = HttpContext.Session.GetString("Role");

            if (current_user_id == null) return Unauthorized(new { message = "Not authenticated" });
            if (current_role != "Organiser" && current_role != "Admin")
                return StatusCode(403, new { message = "Forbidden - insufficient permissions" });

            Category existing = _categoryDb.get_category_by_id(id);
            if (existing == null) return NotFound(new { message = "Category not found" });

            if (category_type != "Running" && category_type != "Walking" && category_type != "Cycling")
                return BadRequest(new { message = "CategoryType must be 'Running', 'Walking', or 'Cycling'" });

            string result = _categoryDb.update_category(id, category_name, category_type, distance,
                                                          age_group, gender, standard_fee);
            if (result.Contains("successfully")) return Ok(new { message = result });
            return BadRequest(new { message = result });
        }

        /// <summary>
        /// Deletes a category. Restricted to Admin only.
        /// </summary>
        /// <param name="id">The category ID.</param>
        /// <response code="204">Category deleted.</response>
        /// <response code="401">Not authenticated.</response>
        /// <response code="403">Admin only.</response>
        /// <response code="404">Category not found.</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult DeleteCategory(int id)
        {
            int? current_user_id = HttpContext.Session.GetInt32("UserId");
            string? current_role = HttpContext.Session.GetString("Role");

            if (current_user_id == null) return Unauthorized(new { message = "Not authenticated" });
            if (current_role != "Admin") return StatusCode(403, new { message = "Forbidden - Admin only" });

            Category existing = _categoryDb.get_category_by_id(id);
            if (existing == null) return NotFound(new { message = "Category not found" });

            string result = _categoryDb.delete_category(id);
            if (result.Contains("successfully")) return NoContent();
            return BadRequest(new { message = result });
        }
    }
}