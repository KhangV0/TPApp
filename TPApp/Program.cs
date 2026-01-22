using Microsoft.EntityFrameworkCore;
using TPApp.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Cho phép lấy HttpContext trong Razor
builder.Services.AddHttpContextAccessor();

// --- Session cần đăng ký ---
builder.Services.AddDistributedMemoryCache(); // Lưu session trên memory
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Đăng ký DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlServerOptions => sqlServerOptions.UseCompatibilityLevel(120)
    )
);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// --- Bắt buộc: UseSession phải nằm ở đây ---
app.UseSession();

app.UseAuthorization();



//app.MapControllerRoute(
//    name: "trangchu",
//    pattern: "Index.html",
//    defaults: new { controller = "Home", action = "Index" }
//);

//app.MapControllerRoute(
//    name: "menu_detail",
//    pattern: "{menuId:int}/{slug}-{id:int}.html",
//    defaults: new { controller = "Menu", action = "Detail" }
//);

//app.MapControllerRoute(
//    name: "techmart-daily",
//    pattern: "techmart-daily.html",
//    defaults: new { controller = "PhieuDangKy", action = "TechmartDaily" }
//);

//app.MapControllerRoute(
//    name: "hoi-thao-tdcn-gt",
//    pattern: "hoi-thao-gioi-thieu-cntb.html",
//    defaults: new { controller = "HoiThao", action = "Index" }
//);

//app.MapControllerRoute(
//    name: "videocn",
//    pattern: "video.html",
//    defaults: new { controller = "Video", action = "Index" }
//);

//app.MapControllerRoute(
//    name: "tiemluckhcn",
//    pattern: "tiem-luc-KHCN.html",
//    defaults: new { controller = "TiemLucKHCN", action = "Index" }
//);

//app.MapControllerRoute(
//    name: "dangnhucaucongnghe",
//    pattern: "dang-yeu-cau-cong-nghe.html",
//    defaults: new { controller = "NhuCau", action = "DangNhuCauCongNghe" }
//);

///* ===================== MEMBER ===================== */

//app.MapControllerRoute(
//    name: "thanhvien",
//    pattern: "thanh-vien.html",
//    defaults: new { controller = "Member", action = "Index" }
//);

//app.MapControllerRoute(
//    name: "tinnhan",
//    pattern: "tin-nhan.html",
//    defaults: new { controller = "Chatting", action = "Index" }
//);

//app.MapControllerRoute(
//    name: "chitiettinvan",
//    pattern: "chi-tiet-tu-van.html",
//    defaults: new { controller = "Chatting", action = "Detail" }
//);

//app.MapControllerRoute(
//    name: "san-pham-quan-tam",
//    pattern: "san-pham-quan-tam.html",
//    defaults: new { controller = "WishList", action = "Index" }
//);

//app.MapControllerRoute(
//    name: "quanlySanPham",
//    pattern: "dang-tin-ban.html",
//    defaults: new { controller = "Product", action = "Create" }
//);

//app.MapControllerRoute(
//    name: "DanhSachSanPham",
//    pattern: "quan-ly-san-pham.html",
//    defaults: new { controller = "Product", action = "List" }
//);

///* ===================== AUTH ===================== */

//app.MapControllerRoute(
//    name: "dangnhap",
//    pattern: "dang-nhap.html",
//    defaults: new { controller = "Account", action = "Login" }
//);

//app.MapControllerRoute(
//    name: "dangky",
//    pattern: "dang-ky.html",
//    defaults: new { controller = "Account", action = "Register" }
//);

//app.MapControllerRoute(
//    name: "quen-mat-khau",
//    pattern: "quen-mat-khau.html",
//    defaults: new { controller = "Account", action = "ForgotPassword" }
//);

//app.MapControllerRoute(
//    name: "doi-mat-khau",
//    pattern: "doi-mat-khau.html",
//    defaults: new { controller = "Account", action = "ChangePassword" }
//);

//app.MapControllerRoute(
//    name: "thay-doi-mat-khau",
//    pattern: "thay-doi-mat-khau.html",
//    defaults: new { controller = "Account", action = "ResetPassword" }
//);

///* ===================== SEARCH ===================== */

//app.MapControllerRoute(
//    name: "timTinTuc",
//    pattern: "tim-kiem.html",
//    defaults: new { controller = "Search", action = "Index" }
//);

//app.MapControllerRoute(
//    name: "timTinTuc-en",
//    pattern: "search.html",
//    defaults: new { controller = "Search", action = "Index" }
//);

///* ===================== PRODUCT ===================== */

//app.MapControllerRoute(
//    name: "Detail-SPCNTB",
//    pattern: "{MenuId}-cong-nghe-thiet-bi/{TypeId}/{QueryString}-{ProductId}.html",
//    defaults: new { controller = "Product", action = "Detail" }
//);

//app.MapControllerRoute(
//    name: "SPCNTB",
//    pattern: "cong-nghe-thiet-bi/{QueryString}-{SpcntbId}.html",
//    defaults: new { controller = "SanPhamCNTB", action = "Index" }
//);

///* ===================== NEWS ===================== */

//app.MapControllerRoute(
//    name: "tintuc",
//    pattern: "tin-tuc-{MenuId}.html",
//    defaults: new { controller = "News", action = "ByMenu" }
//);

//app.MapControllerRoute(
//    name: "News",
//    pattern: "{MenuId}/{QueryString}-{Id}.html",
//    defaults: new { controller = "News", action = "Detail" }
//);

///* ===================== CONTENT ===================== */

//app.MapControllerRoute(
//    name: "ContentA",
//    pattern: "{QueryString}-{MenuId}.html",
//    defaults: new { controller = "Menu", action = "Detail" }
//);

//app.MapControllerRoute(
//    name: "Content",
//    pattern: "m/{QueryString}-{MenuId}.html",
//    defaults: new { controller = "Menu", action = "Detail" }
//);


// ROUTE MẶC ĐỊNH
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.Run();
