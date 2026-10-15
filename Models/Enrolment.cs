using System.ComponentModel.DataAnnotations;

namespace RaceDay_Management_System
{
    public class Enrolment
    {
        public int EnrolmentId { get; set; }

        [Required]
        public int EventId { get; set; }

        [Required]
        public int ParticipantId { get; set; }

        [Required]
        public int EventCategoryId { get; set; }

        [StringLength(50)]
        public string Status { get; set; } = "Registered";

        [Required]
        [StringLength(50)]
        public string RegistrationNumber { get; set; } = string.Empty;

        public DateTime RegistrationDate { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public DateTime? ConfirmationDate { get; set; }

        [StringLength(50)]
        public string PaymentStatus { get; set; } = "Pending";

        // Navigation properties
        public Event? Event { get; set; }
        public User? Participant { get; set; }
        public EventCategory? EventCategory { get; set; }
        public Result? Result { get; set; }
    }
}