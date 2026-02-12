using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TPApp.Controllers
{
    [Authorize]
    public class SecureController : Controller
    {
        public IActionResult Index()
        {
            return Content($"Hello {User.Identity?.Name}, this is a secure area!");
        }
    }
}
