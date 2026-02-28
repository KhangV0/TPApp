using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TPApp.Controllers;
using TPApp.Data;
using TPApp.Entities;
using TPApp.Helpers;
using TPApp.ViewModel;

/// <summary>
/// Renders a portlet of related SanPhamCNTB items based on shared categories.
/// Uses EXISTS subquery — no JOIN, no DISTINCT — for optimal SQL Server execution plan.
///
/// Usage:
///   @await Component.InvokeAsync("ProductPortletIOSanpham", new { productId = Model.Product.ID })
///   @await Component.InvokeAsync("ProductPortletIOSanpham", new { productId = Model.Product.ID, productType = 1 })
/// </summary>
public class ProductPortletIOSanphamViewComponent : ViewComponent
{
    private readonly AppDbContext _context;
    private readonly string _mainDomain;

    public ProductPortletIOSanphamViewComponent(AppDbContext context, IOptions<AppSettings> appSettings)
    {
        _context = context;
        _mainDomain = appSettings.Value.MainDomain;
    }

    /// <param name="productId">ID of the currently viewed SanPhamCNTB. Pass 0 when no product context (e.g. NhaTuVan page).</param>
    /// <param name="productType">Optional TypeId to narrow results (1=CongNghe, 2=SangChe, 3=ThietBi).</param>
    public IViewComponentResult Invoke(int productId = 0, int? productType = null)
    {
        // ── Fallback path: no product context ──────────────────────────────────
        if (productId == 0)
        {
            return TopProducts(productType, excludeId: null);
        }

        // ── Materialize category IDs for this product (one small, fast query) ────
        // Uses index on SanPhamCNTBCategory(SanPhamCNTBId).
        var categoryIds = _context.SanPhamCNTBCategories
            .Where(m => m.SanPhamCNTBId == productId)
            .Select(m => m.CatId)
            .ToList();   // small list, typically 1-5 entries

        if (categoryIds.Count == 0)
        {
            // Edge case: product exists but has no categories → top products fallback
            return TopProducts(productType, excludeId: productId);
        }

        // ── EXISTS pattern: no JOIN, no DISTINCT ───────────────────────────────
        // EF Core translates .Any(m => categoryIds.Contains(m.CatId)) to:
        //   EXISTS (SELECT 1 FROM SanPhamCNTBCategory m
        //           WHERE m.SanPhamCNTBId = p.ID
        //             AND m.CatId IN (1, 3, 7))   ← literal IN list
        // → Index seek on IX_SanPhamCNTBCategory_CatId, no hash join, no sort for DISTINCT.
        var products = _context.SanPhamCNTBs
            .AsNoTracking()
            .Where(p =>
                p.LanguageId == 1 &&
                p.StatusId   == 3 &&
                p.ID         != productId &&
                (!productType.HasValue || p.TypeId == productType.Value) &&
                _context.SanPhamCNTBCategories
                    .Any(m => m.SanPhamCNTBId == p.ID && categoryIds.Contains(m.CatId))
            )
            .OrderByDescending(p => p.Viewed)
            .Take(16)
            .ToList();

        return View(MapToVm(products));
    }

    // ── Fallback: top N products by Viewed ─────────────────────────────────────
    private IViewComponentResult TopProducts(int? productType, int? excludeId)
    {
        var q = _context.SanPhamCNTBs
            .AsNoTracking()
            .Where(p => p.LanguageId == 1 && p.StatusId == 3);

        if (productType.HasValue) q = q.Where(p => p.TypeId == productType.Value);
        if (excludeId.HasValue)   q = q.Where(p => p.ID != excludeId.Value);

        return View(MapToVm(q.OrderByDescending(p => p.Viewed).Take(16).ToList()));
    }

    // ── ViewModel mapping ───────────────────────────────────────────────────────
    private List<ProductPortletItemVm> MapToVm(List<SanPhamCNTB> products) =>
        products.Select(row => new ProductPortletItemVm
        {
            Id       = row.ID,
            Name     = row.Name,
            Code     = row.Code,
            Star     = row.Rating ?? 0,
            IsSC     = row.TypeId == 2,
            IsNC     = row.TypeId == 3,

            ImageUrl = string.IsNullOrEmpty(row.QuyTrinhHinhAnh)
                ? (row.TypeId == 2
                    ? _mainDomain + "images/sangche.png"
                    : _mainDomain + "images/research.jpg")
                : ProductController.CookedImageURL("254-170", row.QuyTrinhHinhAnh, _mainDomain),

            PriceText = row.OriginalPrice == null
                ? ""
                : ProductController.FormatCurrencyOto((decimal?)row.OriginalPrice, row.Currency),

            Url = _mainDomain +
                  "2-cong-nghe-thiet-bi/" +
                  row.TypeId + "/" +
                  ProductController.MakeURLFriendly(row.Name) +
                  "-" + row.ID + ".html"
        }).ToList();
}
