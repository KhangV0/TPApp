using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TPApp.Controllers;
using TPApp.Data;
using TPApp.Helpers;
using TPApp.ViewModel;

public class TimKiemDoiTacPortletViewComponent : ViewComponent
{
    private readonly AppDbContext _context;
    private readonly string _mainDomain;

    public TimKiemDoiTacPortletViewComponent(AppDbContext context, IOptions<AppSettings> appSettings)
    {
        _context = context;
        _mainDomain = appSettings.Value.MainDomain;
    }

    public IViewComponentResult Invoke()
    {
        var linhVuc = HttpContext.Session.GetString("Linhvuc");

        // QUERY GỐC – KHÔNG PHỤ THUỘC LINH VỰC
        var query = _context.TimKiemDoiTacs
            .Where(x =>
                x.LanguageId == 1 &&
                x.StatusId == 3
            );

        // CHỈ LỌC KHI CÓ LINH VỰC
        if (!string.IsNullOrWhiteSpace(linhVuc))
        {
            query = query.Where(x =>
                x.CategoryId != null &&
                x.CategoryId.Contains(";" + linhVuc + ";")
            );
        }

        var data = query
            .OrderByDescending(x => x.Viewed)
            .Take(16)
            .ToList();

        if (!data.Any())
            return View(new List<TimKiemDoiTacPortletItemVm>());

        var model = data.Select(x => new TimKiemDoiTacPortletItemVm
        {
            Id = x.TimDoiTacId,
            TenSanPham = x.TenSanPham,
            FullName = x.FullName,
            Star = x.Rating ?? 0,
            ImageUrl = string.IsNullOrEmpty(x.HinhDaiDien)
                ? _mainDomain + "images/research.jpg"
                : x.HinhDaiDien, // Assuming no CookedImageURL needed here as previously it wasn't used
            Url = _mainDomain +
                  "11-tim-kiem-doi-tac/" +
                  ProductController.MakeURLFriendly(x.TenSanPham) +
                  "-" + x.TimDoiTacId + ".html"
        }).ToList();

        return View(model);
    }
}
