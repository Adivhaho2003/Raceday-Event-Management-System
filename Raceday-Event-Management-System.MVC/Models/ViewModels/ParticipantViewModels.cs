namespace Raceday_Event_Management_System.MVC.Models.ViewModels
{
    public class MyEnrolmentViewModel
    {
        public int EnrolmentId { get; set; }
        public int EventId { get; set; }
        public string EventName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string RegistrationNumber { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
    }

    public class MyResultViewModel
    {
        public int ResultId { get; set; }
        public string EventName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string? ChipTime { get; set; }
        public int? OverallPosition { get; set; }
        public int? CategoryPosition { get; set; }
        public double? Pace { get; set; }
    }
}