using System.ComponentModel.DataAnnotations;

namespace FlightBooking.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string? FullName { get; set; }

        [Required]
        public string? Email { get; set; }

        [Required]
        public string? PhoneNumber { get; set; }

        [Required]
        public string? Password { get; set; }

        // Thêm dòng này để phân quyền
        [Required]
        public string Role { get; set; } = "Customer"; 
    }
}