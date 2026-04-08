using System.ComponentModel.DataAnnotations.Schema;

namespace FlightBooking.Models;

public class Flight
{
    public int Id { get; set; }
    public string FromCity { get; set; } = string.Empty;
    public string ToCity { get; set; } = string.Empty;
    public DateTime DepartureTime { get; set; }
    public decimal Price { get; set; }

    // Các cột này không có trong DB nên phải đánh dấu NotMapped
    [NotMapped]
    public string FlightNumber => "SH-" + Id;

    [NotMapped]
    public string Airline { get; set; } = "SHIN Airways";

    [NotMapped]
    public DateTime ArrivalTime => DepartureTime.AddHours(2);
}