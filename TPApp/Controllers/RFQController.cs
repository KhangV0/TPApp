using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TPApp.Data;
using TPApp.Entities;

namespace TPApp.Controllers
{
    [Authorize]
    public class RFQController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public RFQController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /RFQ/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: /RFQ/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RFQRequest model)
        {
            if (ModelState.IsValid)
            {
                // Set metadata
                model.NguoiTao = _userManager.GetUserId(User);
                model.NgayTao = DateTime.Now;
                model.StatusId = 1; // Default status
                model.DaGuiNhaCungUng = false;

                _context.RFQRequests.Add(model);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Details), new { id = model.Id });
            }

            return View(model);
        }

        // GET: /RFQ/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rfq = await _context.RFQRequests
                .FirstOrDefaultAsync(m => m.Id == id);

            if (rfq == null)
            {
                return NotFound();
            }

            // Optional: Check if user is owner or admin
            // var userId = _userManager.GetUserId(User);
            // if (rfq.NguoiTao != userId && !User.IsInRole("Admin")) return Forbid();

            return View(rfq);
        }

        // POST: /RFQ/SendToSuppliers/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendToSuppliers(int id)
        {
            var rfq = await _context.RFQRequests.FindAsync(id);
            if (rfq == null)
            {
                return NotFound();
            }

            rfq.DaGuiNhaCungUng = true;
            rfq.StatusId = 2; // Sent status or similar
            _context.Update(rfq);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = id });
        }
    }
}
