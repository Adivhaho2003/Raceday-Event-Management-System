using System.ComponentModel.DataAnnotations;

namespace RaceDayAPI.Models
{
    public class Result
    {
        public int ResultId { get; set; }

        [Required]
        public int EnrolmentId { get; set; }

        [Required]
        public int ParticipantId { get; set; }

        [Required]
        public int EventId { get; set; }

        [Required]
        public int CategoryId { get; set; }

        public TimeSpan? GunTime { get; set; }

        public TimeSpan? ChipTime { get; set; }

        public int? OverallPosition { get; set; }

        public int? CategoryPosition { get; set; }

        public double? Pace { get; set; }

        [StringLength(50)]
        public string Status { get; set; } = "Completed"; // DNS, DNF, Completed

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        public Enrolment? Enrolment { get; set; }
        public User? Participant { get; set; }
        public Event? Event { get; set; }
        public Category? Category { get; set; }
    }
}