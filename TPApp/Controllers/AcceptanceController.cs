using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TPApp.Data;
using TPApp.Entities;

namespace TPApp.Controllers
{
    [Authorize]
    public class AcceptanceController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AcceptanceController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Acceptance/Create
        [HttpGet]
        public IActionResult Create()
        {
            var model = new AcceptanceReport
            {
                NgayNghiemThu = DateTime.Now
            };
            return View(model);
        }

        // POST: /Acceptance/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AcceptanceReport model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    model.TrangThaiKy = "Chưa ký";
                    model.NguoiTao = _userManager.GetUserId(User);
                    model.NgayTao = DateTime.Now;
                    model.StatusId = 1;

                    _context.AcceptanceReports.Add(model);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Details), new { id = model.Id });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Đã xảy ra lỗi: " + ex.Message);
                }
            }

            return View(model);
        }

        // GET: /Acceptance/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var report = await _context.AcceptanceReports.FindAsync(id);
            if (report == null)
            {
                return NotFound();
            }

            return View(report);
        }

        // POST: SignAsBenA
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignAsBenA(int id)
        {
            var report = await _context.AcceptanceReports.FindAsync(id);
            if (report == null) return NotFound();

            if (string.IsNullOrEmpty(report.ChuKyBenA))
            {
                report.ChuKyBenA = User.Identity?.Name ?? "Admin";
                
                if (!string.IsNullOrEmpty(report.ChuKyBenB))
                {
                    report.TrangThaiKy = "Hoàn tất";
                    report.StatusId = 2;
                }
                else
                {
                    report.TrangThaiKy = "Đã ký 1 bên";
                }

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: SignAsBenB
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignAsBenB(int id)
        {
            var report = await _context.AcceptanceReports.FindAsync(id);
            if (report == null) return NotFound();

            if (string.IsNullOrEmpty(report.ChuKyBenB))
            {
                report.ChuKyBenB = User.Identity?.Name ?? "Khách hàng";

                if (!string.IsNullOrEmpty(report.ChuKyBenA))
                {
                    report.TrangThaiKy = "Hoàn tất";
                    report.StatusId = 2;
                }
                else
                {
                    report.TrangThaiKy = "Đã ký 1 bên";
                }

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
