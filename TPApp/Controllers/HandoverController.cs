using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using TPApp.Data;
using TPApp.Entities;

namespace TPApp.Controllers
{
    [Authorize]
    public class HandoverController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HandoverController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Handover/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Handover/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(HandoverReport model, string DanhMucThietBiJson, string DanhMucHoSoJson)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Assign JSON strings
                    model.DanhMucThietBiJson = DanhMucThietBiJson;
                    model.DanhMucHoSoJson = DanhMucHoSoJson;

                    // Set Metadata
                    model.NguoiTao = _userManager.GetUserId(User);
                    model.NgayTao = DateTime.Now;
                    model.StatusId = 1;

                    _context.HandoverReports.Add(model);
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

        // GET: /Handover/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var report = await _context.HandoverReports.FindAsync(id);
            if (report == null)
            {
                return NotFound();
            }

            return View(report);
        }
    }
}
