namespace FlightBooking.Models
{
    public class Booking
    {
        public int Id { get; set; }
        public int UserId { get; set; } 
        public string? FlightCode { get; set; } // Phải trùng với tên trong Controller
        public decimal TotalAmount { get; set; } 
        public DateTime BookingDate { get; set; } = DateTime.Now;
        public string Status { get; set; } = "Paid";
    }
}