using Microsoft.AspNetCore.Mvc;
using TPApp.Controllers;
using TPApp.Data;
using TPApp.ViewModel;

public class TimKiemDoiTacPortletViewComponent : ViewComponent
{
    private readonly AppDbContext _context;
    private const string MainDomain = "https://localhost:7232/";

    public TimKiemDoiTacPortletViewComponent(AppDbContext context)
    {
        _context = context;
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
                ? MainDomain + "images/research.jpg"
                : x.HinhDaiDien,
            Url = MainDomain +
                  "11-tim-kiem-doi-tac/" +
                  ProductController.MakeURLFriendly(x.TenSanPham) +
                  "-" + x.TimDoiTacId + ".html"
        }).ToList();

        return View(model);
    }
}
