using System;

namespace FlightBooking.Models
{
    public class FlightViewModel
    {
        // ID chính của chuyến bay
        public int Id { get; set; } 

        // Mã số chuyến bay (ví dụ: VN-102)
        public string FlightNumber { get; set; } = string.Empty;

        // Tên hãng hàng không
        public string Airline { get; set; } = string.Empty;

        // Thời gian đi và đến
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }

        // Giá vé
        public decimal Price { get; set; }

        // Địa điểm (Dùng cho hiển thị trên View Search)
        public string From { get; set; } = string.Empty;
        public string To { get; set; } = string.Empty;

        // Cột mới thêm để hết gạch đỏ và hiển thị số ghế thực tế
        public int AvailableSeats { get; set; } 

        // Các thuộc tính bổ sung để khớp với logic lọc
        public string FromCity { get; set; } = string.Empty;
        public string ToCity { get; set; } = string.Empty;
        
        // Ngày khởi hành
        public DateTime DepartureDate { get; set; }
    }
}