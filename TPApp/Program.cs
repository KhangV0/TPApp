using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TPApp.Data;
using TPApp.Entities;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();

// Increase form value limits for CKEditor content with embedded images
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.ValueLengthLimit = 50 * 1024 * 1024;   // 50 MB per value
    options.MultipartBodyLengthLimit = 50 * 1024 * 1024; // 50 MB total
});

// Increase Kestrel max request body size
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 50 * 1024 * 1024; // 50 MB
});
builder.Services.AddMemoryCache();
builder.Services.AddScoped<TPApp.Services.IWorkflowService, TPApp.Services.WorkflowService>();
builder.Services.AddScoped<TPApp.Services.INotificationQueueService, TPApp.Services.NotificationQueueService>();

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


// --- Configuration ---
builder.Services.Configure<TPApp.Helpers.AppSettings>(builder.Configuration.GetSection("AppSettings"));
builder.Services.Configure<TPApp.Configuration.FeatureFlags>(builder.Configuration.GetSection("FeatureFlags"));

// --- Identity Configuration ---
builder.Services.AddIdentityCore<ApplicationUser>(options =>
{
    // Configure Identity options
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;
    options.User.RequireUniqueEmail = false; 
    options.SignIn.RequireConfirmedAccount = false;
})
.AddSignInManager<SignInManager<ApplicationUser>>() // Add SignInManager for cookie handling
.AddUserStore<TPApp.Services.ApplicationUserStore>() // Use Custom Store
.AddDefaultTokenProviders();

// Add Authentication Cookie
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
})
.AddIdentityCookies();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/dang-nhap.html";
    options.LogoutPath = "/dang-ky.html"; // Or logout logic
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.Cookie.Name = "TPApp.Identity";
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromDays(30);
    options.SlidingExpiration = true;
});

// --- CMS Authorization ---
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CmsAccess", policy =>
        policy.Requirements.Add(new TPApp.Authorization.CmsAdminRequirement()));
});
builder.Services.AddScoped<Microsoft.AspNetCore.Authorization.IAuthorizationHandler, TPApp.Authorization.CmsAdminHandler>();
builder.Services.AddScoped<TPApp.Interfaces.ICmsAccessService, TPApp.Services.CmsAccessService>();
builder.Services.AddScoped<TPApp.Interfaces.ICntbMasterService, TPApp.Services.CntbMasterService>();

// --- Services ---
builder.Services.AddScoped<TPApp.Interfaces.IProductService, TPApp.Services.ProductService>();
builder.Services.AddScoped<TPApp.Interfaces.IDashboardService, TPApp.Services.DashboardService>();
builder.Services.AddScoped<TPApp.Interfaces.IAccountService, TPApp.Services.AccountService>();
builder.Services.AddScoped<TPApp.Interfaces.IProjectService, TPApp.Services.ProjectService>();

// E-Sign Services
builder.Services.AddScoped<TPApp.Interfaces.IESignGateway, TPApp.Services.ESignGateway>();

// Permission System
builder.Services.AddScoped<TPApp.Interfaces.IPermissionService, TPApp.Services.PermissionService>();

// Seller Workflow Services
builder.Services.AddScoped<TPApp.Interfaces.IInvitationService, TPApp.Services.InvitationService>();
builder.Services.AddScoped<TPApp.Interfaces.IProposalService, TPApp.Services.ProposalService>();
builder.Services.AddScoped<TPApp.Interfaces.ISelectionService, TPApp.Services.SelectionService>();
builder.Services.AddScoped<TPApp.Interfaces.IScoringService, TPApp.Services.ScoringService>();
builder.Services.AddScoped<TPApp.Interfaces.IProjectMemberService, TPApp.Services.ProjectMemberService>();
builder.Services.AddScoped<TPApp.Interfaces.IOtpEmailService, TPApp.Services.OtpEmailService>();
builder.Services.AddScoped<TPApp.Interfaces.ISmsSender, TPApp.Services.StubSmsSender>();
builder.Services.AddScoped<TPApp.Interfaces.ILegalReviewService, TPApp.Services.LegalReviewService>();

// Step 6+7: Contract & Digital Signing Services
builder.Services.AddScoped<TPApp.Interfaces.IHashService,              TPApp.Services.HashService>();
builder.Services.AddScoped<TPApp.Interfaces.IContractAuditService,     TPApp.Services.ContractAuditService>();
builder.Services.AddScoped<TPApp.Interfaces.IContractApprovalService,  TPApp.Services.ContractApprovalService>();
builder.Services.AddScoped<TPApp.Interfaces.IContractService,          TPApp.Services.ContractService>();
builder.Services.AddScoped<TPApp.Interfaces.IContractSigningService,   TPApp.Services.ContractSigningService>();
builder.Services.AddScoped<TPApp.Interfaces.IVerificationService,      TPApp.Services.VerificationService>();

// CA signing provider adapters (resolved by name via factory)
builder.Services.AddScoped<TPApp.Interfaces.ISigningProvider, TPApp.Services.Signing.VnptSigningProvider>();
builder.Services.AddScoped<TPApp.Interfaces.ISigningProvider, TPApp.Services.Signing.FptSigningProvider>();
builder.Services.AddScoped<TPApp.Interfaces.ISigningProvider, TPApp.Services.Signing.ViettelSigningProvider>();
builder.Services.AddScoped<TPApp.Services.ISigningProviderFactory, TPApp.Services.SigningProviderFactory>();


// --- AI Semantic Matching Services ---
builder.Services.AddMemoryCache();

// Infrastructure layer
builder.Services.AddHttpClient<TPApp.Infrastructure.AI.IEmbeddingService, TPApp.Infrastructure.AI.OpenAIEmbeddingService>();
builder.Services.AddScoped<TPApp.Infrastructure.Repositories.IEmbeddingRepository, TPApp.Infrastructure.Repositories.EmbeddingRepository>();
builder.Services.AddScoped<TPApp.Infrastructure.Repositories.ISearchLogRepository, TPApp.Infrastructure.Repositories.SearchLogRepository>();

// Application layer
builder.Services.AddScoped<TPApp.Application.Services.IAISupplierMatchingService, TPApp.Application.Services.AISupplierMatchingService>();
builder.Services.AddScoped<TPApp.Application.Services.IProductEmbeddingService, TPApp.Application.Services.ProductEmbeddingService>();
builder.Services.AddScoped<TPApp.Application.Services.ISearchService, TPApp.Application.Services.SearchService>();

// Background service
builder.Services.AddHostedService<TPApp.BackgroundServices.ProductEmbeddingUpdaterService>();

// --- Notification System ---
builder.Services.AddScoped<TPApp.Interfaces.ISystemParameterService, TPApp.Services.SystemParameterService>();
builder.Services.AddScoped<TPApp.Interfaces.IEmailSender, TPApp.Services.GmailEmailSender>();
builder.Services.AddScoped<TPApp.Interfaces.ISmsSender, TPApp.Services.StubSmsSender>(); // Twilio replaced with stub — add Twilio back when SMS is needed
builder.Services.AddScoped<TPApp.Interfaces.INotificationProcessor, TPApp.Services.NotificationProcessor>();
builder.Services.AddHostedService<TPApp.BackgroundServices.NotificationWorker>();


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

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// SignalR Hub
app.MapHub<TPApp.Hubs.NotificationHub>("/notificationHub");


// --- CMS Area Route (MUST be before all other routes) ---
app.MapAreaControllerRoute(
    name: "cms",
    areaName: "Cms",
    pattern: "cms/{controller=Dashboard}/{action=Index}/{id?}"
);

// --- Custom Routes (Moved from Controllers) ---

// 1. Product Routes
app.MapControllerRoute(
    name: "product_index",
    pattern: "cong-nghe-thiet-bi-2.html",
    defaults: new { controller = "Product", action = "Index" }
);

app.MapControllerRoute(
    name: "product_detail",
    pattern: "{menu:int}-cong-nghe-thiet-bi/{typeId:int}/{slug}-{id:int}.html",
    defaults: new { controller = "Product", action = "Detail" }
);

app.MapControllerRoute(
    name: "product_category",
    pattern: "2-ds-cong-nghe-thiet-bi/{slug}-{cateId:int}.html",
    defaults: new { controller = "Product", action = "ProductByCate" }
);

app.MapControllerRoute(
    name: "product_add_cart",
    pattern: "cart/add/{id:int}",
    defaults: new { controller = "Product", action = "AddToCart" }
);

// 2. TimKiemDoiTac Routes
app.MapControllerRoute(
    name: "tkdt_index",
    pattern: "tim-kiem-doi-tac-11.html",
    defaults: new { controller = "TimKiemDoiTac", action = "Index" }
);

app.MapControllerRoute(
    name: "tkdt_detail",
    pattern: "11-tim-kiem-doi-tac/{slug}-{id}.html",
    defaults: new { controller = "TimKiemDoiTac", action = "Detail" }
);

app.MapControllerRoute(
    name: "tkdt_category",
    pattern: "11-ds-tim-kiem-doi-tac/{slug}-{cateId}.html",
    defaults: new { controller = "TimKiemDoiTac", action = "List" }
);

// 3. TiemLucKHCN Routes
app.MapControllerRoute(
    name: "tiemluc_index",
    pattern: "tiem-luc-KHCN.html",
    defaults: new { controller = "TiemLucKHCN", action = "Index" }
);

// 4. NhuCauCongNghe Routes
app.MapControllerRoute(
    name: "nhucau_dang",
    pattern: "yeu-cau-cong-nghe-67.html",
    defaults: new { controller = "Nhucaucongnghe", action = "CateTechNeeds", menuId = 67 }
);

app.MapControllerRoute(
    name: "nhucau_detail",
    pattern: "{menuId:int}/yeu-cau/{slug}-{id:int}.html",
    defaults: new { controller = "Nhucaucongnghe", action = "Detail" }
);

// 5. News Routes
app.MapControllerRoute(
    name: "news_menu",
    pattern: "{queryString:regex(^tin-su-kien|hoi-thao-trinh-dien-cong-nghe$)}-{menuId:int}.html",
    defaults: new { controller = "News", action = "Category" }
);

app.MapControllerRoute(
    name: "news_detail",
    pattern: "{menuId:int}/{queryString}-{id:long}.html",
    defaults: new { controller = "News", action = "Detail" }
);

// 6. Menus Routes (Gioi thieu, quy dinh)
app.MapControllerRoute(
    name: "menu_detail",
    pattern: "{queryString:regex(^gioi-thieu-chung|quy-dinh-chung$)}-{menuId:int}.html",
    defaults: new { controller = "Menu", action = "Detail" }
);

// 7. Forum Routes
app.MapControllerRoute(
    name: "forum_index",
    pattern: "thao-luan.html",
    defaults: new { controller = "Forum", action = "Index" }
);

app.MapControllerRoute(
    name: "forum_detail",
    pattern: "chi-tiet-thao-luan-{id}.html",
    defaults: new { controller = "Forum", action = "Detail" }
);

app.MapControllerRoute(
    name: "forum_category",
    pattern: "thao-luan-{linhvuc:int}-{parentid:int}.html",
    defaults: new { controller = "Forum", action = "Index" }
);

app.MapControllerRoute(
    name: "feedback_index",
    pattern: "lien-he-74.html",
    defaults: new { controller = "Feedback", action = "Index" }
);

// 8. Auth Routes
app.MapControllerRoute(
    name: "login_page",
    pattern: "dang-nhap.html",
    defaults: new { controller = "Account", action = "Login" }
);

app.MapControllerRoute(
    name: "register_page",
    pattern: "dang-ky.html",
    defaults: new { controller = "Account", action = "Register" }
);


// ROUTE MẶC ĐỊNH

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.Run();
