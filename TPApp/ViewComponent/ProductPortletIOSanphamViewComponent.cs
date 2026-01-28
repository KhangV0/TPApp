using Microsoft.AspNetCore.Mvc;
using TPApp.Controllers;
using TPApp.Data;
using TPApp.ViewModel;

public class ProductPortletIOSanphamViewComponent : ViewComponent
{
    private readonly AppDbContext _context;
    private const string MainDomain = "https://localhost:7232/";

    public ProductPortletIOSanphamViewComponent(AppDbContext context)
    {
        _context = context;
    }

    public IViewComponentResult Invoke()
    {
        var linhVuc = HttpContext.Session.GetString("Linhvuc");

        // QUERY GỐC – KHÔNG PHỤ THUỘC LINH VỰC
        var query = _context.SanPhamCNTBs
            .Where(x =>
                x.LanguageId == 1 &&
                x.StatusId == 3
            );

        // CÓ LINH VỰC → MỚI LỌC
        if (!string.IsNullOrWhiteSpace(linhVuc))
        {
            query = query.Where(x =>
                x.CategoryId != null &&
                x.CategoryId.Contains(";" + linhVuc + ";")
            );
        }

        var products = query
            .OrderByDescending(x => x.Viewed)
            .Take(16)
            .ToList();

        var model = products.Select(row => new ProductPortletItemVm
        {
            Id = row.ID,
            Name = row.Name,
            Code = row.Code,
            Star = row.Rating ?? 0,
            IsSC = row.TypeId == 2,
            IsNC = row.TypeId == 3,

            ImageUrl = string.IsNullOrEmpty(row.QuyTrinhHinhAnh)
                ? (row.TypeId == 2
                    ? MainDomain + "images/sangche.png"
                    : MainDomain + "images/research.jpg")
                : ProductController.CookedImageURL("254-170", row.QuyTrinhHinhAnh),

            PriceText = row.OriginalPrice == null
                ? ""
                : ProductController.FormatCurrencyOto(
                    (decimal?)row.OriginalPrice,
                    row.Currency),

            Url = MainDomain +
                  "2-cong-nghe-thiet-bi/" +
                  row.TypeId + "/" +
                  ProductController.MakeURLFriendly(row.Name) +
                  "-" + row.ID + ".html"
        }).ToList();

        return View(model);
    }
}
