using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FlightBooking.Data;
using FlightBooking.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;

namespace FlightBooking.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // --- 1. TRANG THÔNG TIN TÀI KHOẢN (HIỆN TRANG MỚI) ---
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userIdStr = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return RedirectToAction("Register");

            int userId = int.Parse(userIdStr);
            // Lấy thông tin từ Database để hiển thị
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return NotFound();
            return View(user);
        }

        // --- 2. THANH TOÁN & ĐẶT VÉ ---
        [Authorize]
        [HttpGet]
        public IActionResult Checkout(string flightCode, decimal amount)
        {
            ViewBag.FlightCode = flightCode;
            ViewBag.Amount = amount;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> ProcessPayment(string flightCode, decimal amount, string cardNumber)
        {
            var userIdStr = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return RedirectToAction("Register");

            var booking = new Booking
            {
                UserId = int.Parse(userIdStr),
                FlightCode = flightCode,
                TotalAmount = amount,
                BookingDate = DateTime.Now,
                Status = "Paid" 
            };

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            // Sau khi lưu xong, hiện trang đặt vé thành công
            return RedirectToAction("BookingSuccess");
        }

        [Authorize]
        public IActionResult BookingSuccess()
        {
            return View();
        }

        // --- 3. QUẢN TRỊ & ĐĂNG NHẬP/XUẤT ---
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminDashboard()
        {
            var users = await _context.Users.ToListAsync();
            return View(users);
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated) return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "Email này đã được sử dụng.");
                    return View(model);
                }
                
                string role = (model.Email == "admin@gmail.com") ? "Admin" : "Customer";
                var user = new User { FullName = model.FullName, Email = model.Email, PhoneNumber = model.PhoneNumber, Password = model.Password, Role = role };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                var claims = new List<Claim> {
                    new Claim(ClaimTypes.Name, user.FullName ?? "Người dùng"),
                    new Claim(ClaimTypes.Email, user.Email ?? ""),
                    new Claim(ClaimTypes.Role, user.Role), 
                    new Claim("UserId", user.Id.ToString())
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                return (user.Role == "Admin") ? RedirectToAction("AdminDashboard") : RedirectToAction("Index", "Home");
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }
    }
}