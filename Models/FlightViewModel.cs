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

        // Thời gian đi và đến (Bắt buộc kiểu DateTime để dùng được .ToString("HH:mm"))
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }

        // Giá vé
        public decimal Price { get; set; }

        // Địa điểm (Dùng cho hiển thị trên View Search)
        public string From { get; set; } = string.Empty;
        public string To { get; set; } = string.Empty;

        // Các thuộc tính bổ sung để khớp với logic lọc (Where) trong Controller
        public string FromCity { get; set; } = string.Empty;
        public string ToCity { get; set; } = string.Empty;
        
        // Ngày khởi hành (Dùng để so sánh ngày trong hàm Search)
        public DateTime DepartureDate { get; set; }
    }
}