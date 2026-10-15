using System.ComponentModel.DataAnnotations;

namespace RaceDay_Management_System
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
        public string EventType { get; set; } = string.Empty;

        [StringLength(50)]
        public string? Distance { get; set; }

        [StringLength(50)]
        public string Status { get; set; } = "Pending";

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Navigation property
        public User? Organiser { get; set; }
    }
}