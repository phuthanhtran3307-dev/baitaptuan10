using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FlightBooking.Data;
using FlightBooking.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace FlightBooking.Controllers;

public class FlightController : Controller
{
    private readonly ApplicationDbContext _context;

    public FlightController(ApplicationDbContext context)
    {
        _context = context;
    }

    [AllowAnonymous]
    public IActionResult Index() => RedirectToAction("Search");

    // ==========================================
    // 1. TÌM KIẾM CHUYẾN BAY (Cập nhật hiển thị ghế)
    // ==========================================
    [AllowAnonymous]
    public async Task<IActionResult> Search(string? from, string? to, DateTime? date)
    {
        ViewBag.From = from;
        ViewBag.To = to;
        ViewBag.SelectedDate = date?.ToString("dd/MM/yyyy") ?? "Tất cả";

        var query = _context.Flights.AsNoTracking().AsQueryable();

        if (!string.IsNullOrEmpty(from))
            query = query.Where(f => f.FromCity.ToLower().Contains(from.ToLower()));
        
        if (!string.IsNullOrEmpty(to))
            query = query.Where(f => f.ToCity.ToLower().Contains(to.ToLower()));

        if (date.HasValue)
            query = query.Where(f => f.DepartureTime.Date == date.Value.Date);

        var results = await query
            .OrderBy(f => f.DepartureTime) 
            .Select(f => new FlightViewModel {
                Id = f.Id, 
                FlightNumber = "SH-" + f.Id, 
                Airline = "SHIN Airways", 
                DepartureTime = f.DepartureTime, 
                ArrivalTime = f.DepartureTime.AddHours(2), 
                Price = f.Price,
                From = f.FromCity, 
                To = f.ToCity,
                // Hiển thị số ghế còn lại ra View
                AvailableSeats = f.AvailableSeats 
            }).ToListAsync();

        if (!results.Any())
        {
            results.Add(CreateMockFlight(from, to, date));
        }

        return View(results); 
    }

    // ==========================================
    // 2. CHI TIẾT CHUYẾN BAY
    // ==========================================
    [AllowAnonymous]
    public async Task<IActionResult> Details(int id, string? from, string? to)
    {
        if (id == 999) return View(CreateMockFlight(from, to, null));

        var f = await _context.Flights.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (f == null) return NotFound();

        return View(new FlightViewModel {
            Id = f.Id,
            FlightNumber = "SH-" + f.Id,
            Airline = "SHIN Airways",
            DepartureTime = f.DepartureTime,
            ArrivalTime = f.DepartureTime.AddHours(2),
            Price = f.Price,
            From = f.FromCity,
            To = f.ToCity,
            AvailableSeats = f.AvailableSeats
        });
    }

    // ==========================================
    // 3. XỬ LÝ ĐẶT VÉ (Bổ sung logic trừ ghế)
    // ==========================================
    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> FinalizeBooking(int flightId, decimal amount)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdStr, out int currentUserId)) 
            return RedirectToAction("Login", "Account");

        // BẮT ĐẦU TRANSACTION ĐỂ ĐẢM BẢO AN TOÀN DỮ LIỆU
        using var transaction = await _context.Database.BeginTransactionAsync();

        try {
            // 1. Kiểm tra nếu là chuyến bay thật thì phải trừ ghế
            if (flightId != 999)
            {
                var flight = await _context.Flights.FindAsync(flightId);
                
                if (flight == null) throw new Exception("Không tìm thấy chuyến bay.");
                
                if (flight.AvailableSeats <= 0)
                {
                    TempData["Error"] = "Rất tiếc, chuyến bay này đã hết chỗ!";
                    return RedirectToAction("Search");
                }

                // Trừ 1 ghế
                flight.AvailableSeats -= 1;
                _context.Flights.Update(flight);
            }

            // 2. Tạo bản ghi đặt vé
            var booking = new Booking
            {
                FlightId = (flightId == 999) ? (int?)null : flightId, 
                UserId = currentUserId,
                BookingDate = DateTime.Now, 
                Status = "Thành công",
                TotalAmount = amount 
            };

            _context.Bookings.Add(booking);
            
            // 3. Lưu tất cả thay đổi
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            
            TempData["Success"] = "Đặt vé thành công! Chỗ ngồi của bạn đã được xác nhận.";
            return RedirectToAction("History");
        }
        catch (Exception ex) {
            await transaction.RollbackAsync();
            var errorMsg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            TempData["Error"] = "Lỗi hệ thống: " + errorMsg;
            return RedirectToAction("Search");
        }
    }

    // ==========================================
    // 4. LỊCH SỬ ĐẶT VÉ
    // ==========================================
    [Authorize]
    public async Task<IActionResult> History()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdStr, out int currentUserId)) 
            return RedirectToAction("Login", "Account");

        bool isAdmin = User.IsInRole("Admin");

        var query = _context.Bookings
            .Include(b => b.Flight)
            .Include(b => b.User)
            .AsNoTracking()
            .AsQueryable();

        if (!isAdmin)
        {
            query = query.Where(b => b.UserId == currentUserId);
        }

        var bookings = await query
            .OrderByDescending(b => b.BookingDate) 
            .ToListAsync();

        return View(bookings);
    }

    // ==========================================
    // HÀM TẠO DỮ LIỆU MOCK
    // ==========================================
    private FlightViewModel CreateMockFlight(string? from, string? to, DateTime? date)
    {
        var depTime = date?.Date.AddHours(8) ?? DateTime.Today.AddDays(1).AddHours(8);
        return new FlightViewModel { 
            Id = 999,
            FlightNumber = "VN-MOCK", 
            Airline = "Vietnam Airlines (Demo)", 
            DepartureTime = depTime, 
            ArrivalTime = depTime.AddHours(2), 
            Price = 1500000, 
            From = !string.IsNullOrEmpty(from) ? from.ToUpper() : "HỒ CHÍ MINH (SGN)", 
            To = !string.IsNullOrEmpty(to) ? to.ToUpper() : "HÀ NỘI (HAN)",
            AvailableSeats = 99 // Chuyến bay ảo luôn còn chỗ
        };
    }
}