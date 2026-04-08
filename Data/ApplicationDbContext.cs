using Microsoft.EntityFrameworkCore;
using FlightBooking.Models;

namespace FlightBooking.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<Flight> Flights { get; set; } = null!;
    public DbSet<Booking> Bookings { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Flight>(entity => {
            entity.Property(f => f.Price).HasColumnType("decimal(18,2)");
        });

        modelBuilder.Entity<Booking>(entity => {
            entity.Property(b => b.TotalAmount).HasColumnType("decimal(18,2)");
        });
    }
}