using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TPApp.Data;
using TPApp.ViewModel;

public class MenuLeftViewComponent : ViewComponent
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;

    public MenuLeftViewComponent(AppDbContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    public IViewComponentResult Invoke(int menuId)
    {
        var lang = HttpContext.Session.GetInt32("LanguageId") ?? 1;
        var mainDomain = _config["AppSettings:MainDomain"] ?? "";

        int[] listMenuId = { 44, 45 };

        var model = new MenuLeftViewModel
        {
            MenuId = menuId
        };

        var parentMenu = _context.Menus
            .AsNoTracking()
            .FirstOrDefault(x => x.MenuId == menuId && x.LanguageId == lang);

        if (parentMenu == null)
            return View(model);

        model.Header = parentMenu.Title;

        model.Menus = _context.Menus
            .AsNoTracking()
            .Where(x =>
                x.MenuPosition.Contains("4") &&
                listMenuId.Contains(x.MenuId) &&
                x.LanguageId == lang)
            .OrderBy(x => x.Sort)
            .Take(20)
            .Select(x => new MenuItemViewModel
            {
                MenuId = x.MenuId,
                Title = x.Title,
                NavigateUrl = $"{mainDomain}{x.QueryString}-{x.MenuId}.html"
            })
            .ToList();

        return View(model);

    }
}
