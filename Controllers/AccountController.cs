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

        // ==========================================
        // 1. QUẢN TRỊ HỆ THỐNG (Bổ sung mới)
        // ==========================================
        [Authorize(Roles = "Admin")] // Chỉ tài khoản Admin mới có quyền truy cập
        public async Task<IActionResult> AdminDashboard()
        {
            // Lấy danh sách thành viên từ Database để hiển thị lên bảng
            var users = await _context.Users.AsNoTracking().ToListAsync();
            return View(users);
        }

        // ==========================================
        // 2. HỒ SƠ TÀI KHOẢN
        // ==========================================
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return RedirectToAction("Login");

            int userId = int.Parse(userIdStr);
            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return NotFound();
            return View(user);
        }

        // ==========================================
        // 3. ĐĂNG NHẬP
        // ==========================================
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (User.Identity != null && User.Identity.IsAuthenticated) 
                return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            if (ModelState.IsValid)
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email && u.Password == model.Password);
                
                if (user != null)
                {
                    await SignInUser(user);

                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                        return Redirect(returnUrl);

                    // ✅ SỬA ĐỔI: Admin thì chuyển đến trang Dashboard quản lý thay vì History
                    return (user.Role == "Admin") ? RedirectToAction("AdminDashboard") : RedirectToAction("Index", "Home");
                }
                ModelState.AddModelError("", "Email hoặc mật khẩu không chính xác.");
            }
            return View(model);
        }

        // ==========================================
        // 4. ĐĂNG KÝ
        // ==========================================
        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var existingUser = await _context.Users.AnyAsync(u => u.Email == model.Email);
                if (existingUser)
                {
                    ModelState.AddModelError("Email", "Email này đã được sử dụng.");
                    return View(model);
                }
                
                string role = (model.Email.ToLower() == "admin@gmail.com") ? "Admin" : "Customer";
                
                var user = new User { 
                    FullName = model.FullName, 
                    Email = model.Email, 
                    PhoneNumber = model.PhoneNumber, 
                    Password = model.Password, 
                    Role = role 
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                await SignInUser(user);
                return RedirectToAction("Index", "Home");
            }
            return View(model);
        }

        // ==========================================
        // 5. XỬ LÝ THANH TOÁN
        // ==========================================
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessPayment(int flightId, decimal amount)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int currentUserId)) 
                return RedirectToAction("Login");

            var booking = new Booking
            {
                UserId = currentUserId,
                // ✅ GIỮ NGUYÊN: Xử lý ID 999 tránh lỗi Foreign Key
                FlightId = (flightId == 999) ? (int?)null : flightId, 
                TotalAmount = amount,
                BookingDate = DateTime.Now,
                Status = "Thành công" 
            };

            try {
                _context.Bookings.Add(booking);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đặt vé thành công!";
                return RedirectToAction("History", "Flight");
            }
            catch (Exception ex) {
                TempData["Error"] = "Lỗi lưu: " + (ex.InnerException?.Message ?? ex.Message);
                return RedirectToAction("Search", "Flight");
            }
        }

        // ==========================================
        // 6. ĐĂNG XUẤT
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        private async Task SignInUser(User user)
        {
            var claims = new List<Claim> {
                new Claim(ClaimTypes.Name, user.FullName ?? "Người dùng"),
                new Claim(ClaimTypes.Email, user.Email ?? ""),
                new Claim(ClaimTypes.Role, user.Role ?? "Customer"), // Giúp nhận diện Admin
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
        }
    }
}