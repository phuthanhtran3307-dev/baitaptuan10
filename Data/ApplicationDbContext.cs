using Microsoft.EntityFrameworkCore;
using FlightBooking.Models;

namespace FlightBooking.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<Flight> Flights { get; set; }
    public DbSet<Booking> Bookings { get; set; }
    public DbSet<User> Users { get; set; } 

    // Thêm đoạn này để sửa lỗi cảnh báo (Warning) trong Terminal của bạn
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Cấu hình kiểu dữ liệu decimal cho cột Price để không bị mất số lẻ
        modelBuilder.Entity<Flight>()
            .Property(f => f.Price)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<Booking>()
            .Property(b => b.TotalAmount)
            .HasColumnType("decimal(18,2)");
    }
}