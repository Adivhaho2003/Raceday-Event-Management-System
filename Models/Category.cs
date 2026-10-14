using System.ComponentModel.DataAnnotations;

namespace RaceDayAPI.Models
{
    public class Category
    {
        public int CategoryId { get; set; }

        [Required]
        [StringLength(100)]
        public string CategoryName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string CategoryType { get; set; } = string.Empty; // Running, Walking, Cycling

        [StringLength(50)]
        public string? Distance { get; set; }

        [StringLength(50)]
        public string? AgeGroup { get; set; }

        [StringLength(20)]
        public string? Gender { get; set; } // Male, Female, Mixed

        public decimal StandardFee { get; set; } = 0.00m;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}