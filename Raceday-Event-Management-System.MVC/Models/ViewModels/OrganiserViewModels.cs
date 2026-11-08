using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Raceday_Event_Management_System.MVC.Models.ViewModels
{
    public class OrganiserDashboardViewModel
    {
        public int TotalEvents { get; set; }
        public int TotalEnrolments { get; set; }
        public List<EventViewModel> RecentEvents { get; set; } = new();
    }

    public class CreateEventViewModel
    {
        [Required]
        public string EventName { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        public string Location { get; set; } = string.Empty;

        public string? Venue { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime EventDate { get; set; } = DateTime.Now.AddDays(30);

        [DataType(DataType.DateTime)]
        public DateTime? RegistrationDeadline { get; set; } = DateTime.Now.AddDays(20);

        [Required]
        [Range(0, 10000)]
        public decimal EntryFee { get; set; }

        [Required]
        [Range(1, 50000)]
        public int MaxParticipants { get; set; }

        [Required]
        public string EventType { get; set; } = "Running";

        public string? Distance { get; set; }

        // File Upload Property
        public IFormFile? BannerImage { get; set; }
    }

    public class ManageCategoriesViewModel
    {
        public int EventId { get; set; }
        public string EventName { get; set; } = string.Empty;

        public List<EventCategoryItem> LinkedCategories { get; set; } = new();
        public List<CategoryViewModel> AvailableCategories { get; set; } = new();

        [Required(ErrorMessage = "Please select a category")]
        public int SelectedCategoryId { get; set; }

        [Required]
        [Range(0, 10000)]
        public decimal Fee { get; set; }

        [Required]
        [Range(1, 50000)]
        public int Capacity { get; set; }
    }

    public class CategoryViewModel
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string CategoryType { get; set; } = string.Empty;
    }

    public class EventEnrolmentViewModel
    {
        public int EnrolmentId { get; set; }
        public string RegistrationNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;

        // Nested objects for API mapping
        public EventViewModel Event { get; set; } = new();
        public ParticipantNested Participant { get; set; } = new();
        public EventCategoryNested EventCategory { get; set; } = new();

        // Helper properties
        public string ParticipantName => $"{Participant?.FirstName} {Participant?.LastName}";
        public string ParticipantEmail => Participant?.Email ?? "";
        public string CategoryName => EventCategory?.Category?.CategoryName ?? "";
    }

    public class ParticipantNested
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    public class CaptureResultViewModel
    {
        public int EnrolmentId { get; set; }
        public string ParticipantName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;

        public string? GunTime { get; set; }
        public string? ChipTime { get; set; }
        public int? OverallPosition { get; set; }
        public int? CategoryPosition { get; set; }
        public double? Pace { get; set; }
        public string Status { get; set; } = "Completed";
    }
}