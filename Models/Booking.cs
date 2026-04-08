using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlightBooking.Models
{
    public class Booking
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        // ✅ Đã sửa: Cho phép Null để tránh lỗi khi thanh toán chuyến bay Demo (ID 999)
        public int? FlightId { get; set; } 

        [Display(Name = "Ngày đặt vé")]
        public DateTime BookingDate { get; set; } = DateTime.Now;

        [Required]
        [StringLength(50)]
        [Display(Name = "Trạng thái")]
        public string Status { get; set; } = "Thành công";

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Tổng tiền")]
        public decimal TotalAmount { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        [ForeignKey("FlightId")]
        public virtual Flight? Flight { get; set; }
    }
}