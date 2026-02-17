using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TPApp.Data;
using TPApp.Entities;

namespace TPApp.ViewComponents
{
    public class SellerMenuItemsViewComponent : ViewComponent
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public SellerMenuItemsViewComponent(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            // Menu items moved to _AccountSidebar, return empty
            return Content(string.Empty);
        }
    }
}
