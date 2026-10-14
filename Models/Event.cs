using System.ComponentModel.DataAnnotations;

namespace RaceDayAPI.Models
{
    public class Event
    {
        public int EventId { get; set; }

        [Required]
        [StringLength(255)]
        public string EventName { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        public int OrganiserId { get; set; }

        [Required]
        [StringLength(255)]
        public string Location { get; set; } = string.Empty;

        [StringLength(255)]
        public string? Venue { get; set; }

        [Required]
        public DateTime EventDate { get; set; }

        public DateTime? RegistrationDeadline { get; set; }

        public decimal EntryFee { get; set; } = 0.00m;

        public int? MaxParticipants { get; set; }

        [Required]
        [StringLength(50)]
        public string EventType { get; set; } = string.Empty; // Running, Walking, Cycling

        [StringLength(50)]
        public string? Distance { get; set; }

        [StringLength(50)]
        public string Status { get; set; } = "Pending"; // Pending, Open, Closed, Cancelled, Completed

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Navigation properties (not mapped to database directly)
        public User? Organiser { get; set; }
        public List<EventCategory>? EventCategories { get; set; }
    }
}