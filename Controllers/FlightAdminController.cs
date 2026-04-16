using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FlightBooking.Data;
using FlightBooking.Models;
using Microsoft.AspNetCore.Authorization;

namespace FlightBooking.Controllers
{
    [Authorize(Roles = "Admin")] 
    public class FlightAdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FlightAdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. Xem danh sách chuyến bay
        public async Task<IActionResult> Index()
        {
            var flights = await _context.Flights.ToListAsync();
            return View(flights);
        }

        // 2. Thêm mới chuyến bay (GET)
        public IActionResult Create()
        {
            return View();
        }

        // 3. Xử lý lưu chuyến bay mới (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Flight flight)
        {
            if (ModelState.IsValid)
            {
                _context.Add(flight);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Thêm chuyến bay mới thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(flight);
        }

        // 4. CHỈNH SỬA CHUYẾN BAY (GET)
        public async Task<IActionResult> Edit(int id)
        {
            var flight = await _context.Flights.FindAsync(id);
            if (flight == null) return NotFound();
            return View(flight);
        }

        // 5. Xử lý cập nhật thông tin (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Flight flight)
        {
            if (id != flight.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(flight);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Cập nhật chuyến bay thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Flights.Any(e => e.Id == flight.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(flight);
        }

        // 6. XÓA CHUYẾN BAY (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var flight = await _context.Flights.FindAsync(id);
            if (flight != null)
            {
                _context.Flights.Remove(flight);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã xóa chuyến bay thành công!";
            }
            else
            {
                TempData["Error"] = "Không tìm thấy chuyến bay để xóa.";
            }
            return RedirectToAction(nameof(Index));
        }

        // ==========================================
        // 7. THỐNG KÊ DOANH THU & TỒN KHO (Sửa lỗi GroupBy)
        // ==========================================
        public async Task<IActionResult> Dashboard()
        {
            // Tính toán các con số tổng quát
            var totalRevenue = await _context.Bookings.SumAsync(b => b.TotalAmount);
            var totalTickets = await _context.Bookings.CountAsync();
            var totalAvailableSeats = await _context.Flights.SumAsync(f => f.AvailableSeats);

            // SỬA LỖI TẠI ĐÂY: Lấy dữ liệu thô về List trước (Client-side)
            var rawBookingData = await _context.Bookings
                .Include(b => b.Flight)
                .Select(b => new {
                    FlightNum = b.Flight != null ? b.Flight.FlightNumber : "N/A",
                    Amount = b.TotalAmount
                })
                .ToListAsync(); // Đưa dữ liệu từ SQL về RAM

            // Sau đó mới thực hiện GroupBy trên bộ nhớ để vẽ biểu đồ
            var flightStats = rawBookingData
                .GroupBy(x => x.FlightNum)
                .Select(g => new {
                    Label = g.Key,
                    Value = g.Sum(x => x.Amount)
                })
                .ToList();

            // Truyền dữ liệu ra View
            ViewBag.TotalRevenue = totalRevenue;
            ViewBag.TotalTickets = totalTickets;
            ViewBag.TotalSeats = totalAvailableSeats;
            ViewBag.ChartLabels = flightStats.Select(x => x.Label).ToList();
            ViewBag.ChartValues = flightStats.Select(x => x.Value).ToList();

            return View();
        }
    }
}