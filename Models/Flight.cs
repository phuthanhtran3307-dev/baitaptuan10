using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlightBooking.Models;

public class Flight
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập điểm đi")]
    [Display(Name = "Điểm khởi hành")]
    public string FromCity { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập điểm đến")]
    [Display(Name = "Điểm đến")]
    public string ToCity { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng chọn thời gian")]
    [Display(Name = "Thời gian khởi hành")]
    public DateTime DepartureTime { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Giá vé phải lớn hơn 0")]
    [Display(Name = "Giá vé")]
    public decimal Price { get; set; }

    // THÊM MỚI: Cột này sẽ được lưu xuống Database để quản lý đặt chỗ
    [Required]
    [Display(Name = "Số ghế trống")]
    public int AvailableSeats { get; set; } 

    // Các cột này không có trong DB nên phải đánh dấu NotMapped
    [NotMapped]
    public string FlightNumber => "SH-" + Id;

    [NotMapped]
    public string Airline { get; set; } = "SHIN Airways";

    [NotMapped]
    public string ArrivalTimeDisplay => DepartureTime.AddHours(2).ToString("HH:mm dd/MM/yyyy");
}