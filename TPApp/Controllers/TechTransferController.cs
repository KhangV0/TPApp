using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TPApp.Data;
using TPApp.Entities;

namespace TPApp.Controllers
{
// #if false
    [Authorize]
    public class TechTransferController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TechTransferController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /TechTransfer/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: /TechTransfer/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TechTransferRequest model)
        {
            if (ModelState.IsValid)
            {
                // Set metadata
                model.NguoiTao = _userManager.GetUserId(User);
                model.NgayTao = DateTime.Now;
                model.StatusId = 1; // Default status

                _context.TechTransferRequests.Add(model);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Success));
            }
            return View(model);
        }

        // GET: /TechTransfer/Success
        [HttpGet]
        public IActionResult Success()
        {
            return View();
        }
    }
// #endif
}
