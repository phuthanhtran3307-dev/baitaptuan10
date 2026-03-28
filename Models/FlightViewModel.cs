namespace FlightBooking.Models // Sửa từ nanamespace thành namespace
{
    public class FlightViewModel
    {
        // Giữ nguyên các thuộc tính bên dưới
        public string? FlightNumber { get; set; } // Số hiệu chuyến bay
        public string? Airline { get; set; }      // Hãng hàng không
        public string? DepartureTime { get; set; } // Giờ đi
        public string? ArrivalTime { get; set; }   // Giờ đến
        public decimal Price { get; set; }        // Giá vé
        public string? From { get; set; }         // Điểm đi
        public string? To { get; set; }           // Điểm đến
    }
}