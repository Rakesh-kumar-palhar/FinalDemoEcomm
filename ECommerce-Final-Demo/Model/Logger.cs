using System.ComponentModel.DataAnnotations;

namespace ECommerce_Final_Demo.Model
{
    public class Logger
    {
        [Key]
        public int Id { get; set; } 

        [Required]
        [StringLength(100)]
        public string? ExceptionType { get; set; } 

        [Required]
        public string? Message { get; set; }

        [Required]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
