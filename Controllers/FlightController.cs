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

    // Mặc định chuyển hướng về trang tìm kiếm
    [AllowAnonymous]
    public IActionResult Index() => RedirectToAction("Search");

    // ==========================================
    // 1. TÌM KIẾM CHUYẾN BAY
    // ==========================================
    [AllowAnonymous]
    public async Task<IActionResult> Search(string? from, string? to, DateTime? date)
    {
        // Giữ lại thông tin tìm kiếm để hiển thị lên View
        ViewBag.From = from;
        ViewBag.To = to;
        ViewBag.SelectedDate = date?.ToString("dd/MM/yyyy") ?? "Tất cả";

        var query = _context.Flights.AsNoTracking().AsQueryable();

        // Lọc dữ liệu chuẩn hóa (không phân biệt hoa thường)
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
                To = f.ToCity 
            }).ToListAsync();

        // Nếu không có kết quả thực tế, hiển thị chuyến bay Demo ID 999
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
        // Xử lý riêng cho chuyến bay ảo ID 999
        if (id == 999)
        {
            return View(CreateMockFlight(from, to, null));
        }

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
            To = f.ToCity
        });
    }

    // ==========================================
    // 3. XỬ LÝ ĐẶT VÉ (Khắc phục lỗi Foreign Key)
    // ==========================================
    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> FinalizeBooking(int flightId, decimal amount)
    {
        // Lấy ID người dùng từ Claims
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdStr, out int currentUserId)) 
            return RedirectToAction("Login", "Account");

        var booking = new Booking
        {
            // ✅ Giải pháp lỗi Foreign Key: Nếu là chuyến bay ảo (999), gán FlightId = null
            // Yêu cầu thuộc tính FlightId trong Model Booking phải là int? (Nullable)
            FlightId = (flightId == 999) ? (int?)null : flightId, 
            UserId = currentUserId,
            BookingDate = DateTime.Now, 
            Status = "Thành công",
            TotalAmount = amount 
        };

        try {
            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();
            
            TempData["Success"] = "Đặt vé thành công!";
            return RedirectToAction("History");
        }
        catch (Exception ex) {
            // Log lỗi chi tiết từ InnerException (thường là lỗi từ SQL Server)
            var errorMsg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            TempData["Error"] = "Lỗi hệ thống khi lưu vé: " + errorMsg;
            return RedirectToAction("Search");
        }
    }

    // ==========================================
    // 4. LỊCH SỬ ĐẶT VÉ (Phân quyền Admin/User)
    // ==========================================
    [Authorize]
    public async Task<IActionResult> History()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdStr, out int currentUserId)) 
            return RedirectToAction("Login", "Account");

        // Kiểm tra quyền Admin thông qua Role Claim
        bool isAdmin = User.IsInRole("Admin");

        var query = _context.Bookings
            .Include(b => b.Flight)
            .Include(b => b.User) // Load thông tin User để Admin có thể xem danh tính khách hàng
            .AsNoTracking()
            .AsQueryable();

        // PHÂN QUYỀN: Nếu không phải Admin thì chỉ hiển thị vé của chính họ
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
    // HÀM PHỤ TRỢ (DỮ LIỆU ẢO)
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
            To = !string.IsNullOrEmpty(to) ? to.ToUpper() : "HÀ NỘI (HAN)" 
        };
    } // Đóng hàm CreateMockFlight
} // Đóng Class FlightController