using System.ComponentModel.DataAnnotations;

namespace RaceDayAPI.Models
{
    public class EventCategory
    {
        public int EventCategoryId { get; set; }

        [Required]
        public int EventId { get; set; }

        [Required]
        public int CategoryId { get; set; }

        public decimal? Fee { get; set; }

        public int? Capacity { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        public Event? Event { get; set; }
        public Category? Category { get; set; }
    }
}