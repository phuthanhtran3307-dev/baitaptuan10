using Microsoft.EntityFrameworkCore;
using FlightBooking.Data;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// 1. Cấu hình Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Cấu hình Cookie Authentication (Xác thực)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login"; 
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied"; 
        options.ExpireTimeSpan = TimeSpan.FromHours(2);      
        options.Cookie.HttpOnly = true;                    
        options.Cookie.IsEssential = true;
        options.SlidingExpiration = true;                   
    });

// 3. Cấu hình Session & Cache
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// 4. Cấu hình MVC & Runtime Compilation
var mvcBuilder = builder.Services.AddControllersWithViews();

// Thêm dòng này để hỗ trợ cập nhật giao diện ngay khi lưu file (Ctrl + S)
#if DEBUG
    mvcBuilder.AddRazorRuntimeCompilation();
#endif

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// --- CẤU HÌNH MIDDLEWARE (THỨ TỰ LÀ QUAN TRỌNG NHẤT) ---

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); 

app.UseRouting();

// ✅ SỬA TẠI ĐÂY: Session nên được đặt trước Authentication để hỗ trợ lưu vết đăng nhập
app.UseSession(); 
app.UseAuthentication(); 
app.UseAuthorization();  

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();