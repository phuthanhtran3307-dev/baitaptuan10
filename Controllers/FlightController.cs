using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FlightBooking.Data;
using FlightBooking.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FlightBooking.Controllers;

public class FlightController : Controller
{
    private readonly ApplicationDbContext _context;

    public FlightController(ApplicationDbContext context)
    {
        _context = context;
    }

    // ==========================================
    // GET: Trang danh sách quản lý (Dành cho Admin)
    // ==========================================
    public async Task<IActionResult> Index(string? searchFrom, string? searchTo)
    {
        var flights = _context.Flights.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchFrom))
            flights = flights.Where(f => f.FromCity.Contains(searchFrom));

        if (!string.IsNullOrWhiteSpace(searchTo))
            flights = flights.Where(f => f.ToCity.Contains(searchTo));

        return View(await flights.ToListAsync());
    }

    // ==========================================
    // GET: Tìm kiếm (Dành cho khách hàng)
    // ==========================================
    public async Task<IActionResult> Search(string? searchFrom, string? searchTo)
    {
        // 1. Lấy dữ liệu từ Database và ánh xạ sang FlightViewModel
        var results = await _context.Flights
            .Where(f => (string.IsNullOrEmpty(searchFrom) || f.FromCity.Contains(searchFrom)) &&
                        (string.IsNullOrEmpty(searchTo) || f.ToCity.Contains(searchTo)))
            .Select(f => new FlightViewModel {
                FlightNumber = "DB-" + f.Id, 
                Airline = "Hãng bay hệ thống",
                DepartureTime = "08:00", 
                ArrivalTime = "10:00",
                Price = f.Price,
                From = f.FromCity,
                To = f.ToCity
            }).ToListAsync();

        // 2. Nếu Database trống, thêm Mock Data để test giao diện đẹp như hình của bạn
        if (!results.Any())
        {
            results = new List<FlightViewModel>
            {
                new FlightViewModel { FlightNumber = "VN-102", Airline = "Vietnam Airlines", DepartureTime = "08:00", ArrivalTime = "10:00", Price = 1500000, From = searchFrom ?? "SGN", To = searchTo ?? "HAN" },
                new FlightViewModel { FlightNumber = "VJ-456", Airline = "Vietjet Air", DepartureTime = "14:30", ArrivalTime = "16:30", Price = 950000, From = searchFrom ?? "SGN", To = searchTo ?? "HAN" },
                new FlightViewModel { FlightNumber = "QH-789", Airline = "Bamboo Airways", DepartureTime = "19:00", ArrivalTime = "21:00", Price = 1200000, From = searchFrom ?? "SGN", To = searchTo ?? "HAN" }
            };
        }

        ViewBag.From = searchFrom;
        ViewBag.To = searchTo;
        
        return View(results);
    }

    // ==========================================
    // GET: Chi tiết chuyến bay (Khi nhấn "Chọn chuyến bay")
    // ==========================================
    public async Task<IActionResult> Details(string id)
    {
        // 1. Nếu ID bắt đầu bằng "DB-", tìm trong Database
        if (id != null && id.StartsWith("DB-"))
        {
            string cleanId = id.Replace("DB-", "");
            if (int.TryParse(cleanId, out int flightId))
            {
                var flight = await _context.Flights.FindAsync(flightId);
                if (flight != null)
                {
                    // Chuyển đổi model sang ViewModel để hiển thị đồng nhất
                    var vm = new FlightViewModel {
                        FlightNumber = id,
                        Airline = "Hãng bay hệ thống",
                        Price = flight.Price,
                        From = flight.FromCity,
                        To = flight.ToCity,
                        DepartureTime = "08:00",
                        ArrivalTime = "10:00"
                    };
                    return View(vm);
                }
            }
        }

        // 2. Nếu không tìm thấy hoặc là Mock Data, trả về dữ liệu giả để không bị lỗi trang
        var mock = new FlightViewModel { 
            FlightNumber = id ?? "N/A", 
            Airline = "Hãng bay xác nhận", 
            Price = 1500000, 
            From = "Điểm đi", 
            To = "Điểm đến",
            DepartureTime = "08:00",
            ArrivalTime = "10:00"
        };
        return View(mock);
    }

    // ==========================================
    // GET/POST: Tạo mới (Dành cho Admin)
    // ==========================================
    public IActionResult Create() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Flight flight)
    {
        if (ModelState.IsValid)
        {
            _context.Flights.Add(flight);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(flight);
    }

    // ==========================================
    // POST: Xóa (Dành cho Admin)
    // ==========================================
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var flight = await _context.Flights.FindAsync(id);
        if (flight != null)
        {
            _context.Flights.Remove(flight);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }
}