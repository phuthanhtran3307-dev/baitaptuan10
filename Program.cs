using Microsoft.EntityFrameworkCore;
using FlightBooking.Data;
using Microsoft.AspNetCore.Authentication.Cookies; // Đưa lên đầu cho sạch

var builder = WebApplication.CreateBuilder(args);

// 1. Cấu hình Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Cấu hình Cookie Authentication (PHẢI ĐẶT TRƯỚC builder.Build)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login"; 
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
    });

// 3. Cấu hình MVC
builder.Services.AddControllersWithViews();

var app = builder.Build();

// --- CẤU HÌNH MIDDLEWARE (THỨ TỰ RẤT QUAN TRỌNG) ---

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // Load ảnh shin.jpg, css

app.UseRouting();

// ✅ BẮT BUỘC: Thêm 2 dòng này vào giữa UseRouting và MapControllerRoute
app.UseAuthentication(); // Ai đang truy cập?
app.UseAuthorization();  // Họ có quyền làm gì?

// 4. Định nghĩa Route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();