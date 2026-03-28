using System.ComponentModel.DataAnnotations;

namespace FlightBooking.Models;

public class Flight
{
    public int Id { get; set; }

    [Required]
    public string FromCity { get; set; } = string.Empty;

    [Required]
    public string ToCity { get; set; } = string.Empty;

    [Required]
    public DateTime DepartureTime { get; set; }

    [Required]
    public decimal Price { get; set; }
}