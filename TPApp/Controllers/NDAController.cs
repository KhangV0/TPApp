using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TPApp.Data;
using TPApp.Entities;

namespace TPApp.Controllers
{
    [Authorize]
    public class NDAController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public NDAController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /NDA/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: /NDA/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(NDAAgreement model)
        {
            if (!model.DaDongY)
            {
                ModelState.AddModelError("DaDongY", "Bạn phải đồng ý điều khoản trước khi tiếp tục.");
            }

            if (ModelState.IsValid)
            {
                // Set metadata
                model.NguoiTao = _userManager.GetUserId(User);
                model.NgayTao = DateTime.Now;
                model.StatusId = 1; // Default status

                _context.NDAAgreements.Add(model);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Success));
            }

            return View(model);
        }

        // GET: /NDA/Success
        [HttpGet]
        public IActionResult Success()
        {
            return View();
        }
    }
}
