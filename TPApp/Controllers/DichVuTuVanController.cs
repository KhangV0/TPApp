using Microsoft.AspNetCore.Mvc;
using TPApp.Data;

namespace TPApp.Controllers
{
    public class DichVuTuVanController : Controller
    {
        private readonly AppDbContext _context;

        private const string MainDomain = "https://localhost:7232/";

        public DichVuTuVanController(AppDbContext context)
        {
            _context = context;
        }

        // ================= INDEX =================
        [HttpGet("dich-vu-tu-van-{menuId}.html")]
        public IActionResult Index(int menuId)
        {    

            ViewBag.MainDomain = MainDomain;
            ViewBag.MenuId = menuId;

            return View();
        }
    }
}
