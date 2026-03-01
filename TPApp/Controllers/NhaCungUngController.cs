using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TPApp.Data;
using TPApp.Entities;
using TPApp.Helpers;
using TPApp.Web.Helpers;
using TPApp.ViewModel;

namespace TPApp.Controllers
{
    public class NhaCungUngController : Controller
    {
        private readonly AppDbContext _context;
        private readonly string       _mainDomain;

        private const int LangId          = 1;
        private const int DefaultPageSize  = 16;
        private const int PageWindow       = 5;
        private const int CateParentId     = 2;   // Categories.ParentId for DichVu filter

        public NhaCungUngController(AppDbContext context, IOptions<AppSettings> appSettings)
        {
            _context    = context;
            _mainDomain = appSettings.Value.MainDomain;
        }

        // =====================================================================
        // GET /nha-cung-ung
        // GET /nha-cung-ung?cateId=3&page=1
        // =====================================================================
        [HttpGet("nha-cung-ung.html")]
        public IActionResult Index(int cateId = 0, int page = 1)
        {
            page = Math.Max(1, page);

            // ── Category sidebar ──────────────────────────────────────────
            var categories = _context.Categories
                .AsNoTracking()
                .Where(x => x.ParentId == CateParentId && x.LanguageId == LangId)
                .OrderBy(x => x.Sort)
                .Select(x => new NhaCungUngCateVm { Id = x.CatId, Title = x.Title })
                .ToList();

            // ── Base query ────────────────────────────────────────────────
            IQueryable<NhaCungUng> baseQuery = _context.NhaCungUngs
                .AsNoTracking()
                .Where(x => x.LanguageId == LangId && x.IsActivated == true);

            if (cateId > 0)
            {
                var cateStr = ";" + cateId + ";";
                baseQuery = baseQuery.Where(x =>
                    x.DichVu != null && x.DichVu.Contains(cateStr));
            }

            var orderedQuery = baseQuery.OrderByDescending(x => x.Created);

            // ── Single-pass Count + Page ──────────────────────────────────
            int totalCount = orderedQuery.Count();

            var items = orderedQuery
                .Skip((page - 1) * DefaultPageSize)
                .Take(DefaultPageSize)
                .AsEnumerable()
                .Select(x => new NhaCungUngItemVm
                {
                    Id       = x.CungUngId,
                    FullName = x.FullName ?? "",
                    Slug     = x.QueryString ?? x.FullName ?? "",
                    DiaChi   = x.DiaChi   ?? "",
                    Phone    = x.Phone    ?? "",
                    Email    = x.Email    ?? "",
                    Website  = x.Website  ?? "",
                    Rating   = x.Rating   ?? 0,
                    ImageUrl = ProductController.CookedImageURL("254-170", x.HinhDaiDien, _mainDomain)
                })
                .ToList();

            var vm = new NhaCungUngIndexVm
            {
                CateId      = cateId,
                CurrentPage = page,
                PageSize    = DefaultPageSize,
                TotalCount  = totalCount,
                Items       = items,
                Categories  = categories,
                Pages       = BuildPages(page, (int)Math.Ceiling((double)totalCount / DefaultPageSize))
            };

            return View(vm);
        }

        // =====================================================================
        // GET /nha-cung-ung/{slug}-{id}.html
        // =====================================================================
        [HttpGet("nha-cung-ung/{slug}-{id:int}.html")]
        public IActionResult Detail(string slug, int id)
        {
            var entity = _context.NhaCungUngs
                .FirstOrDefault(x =>
                    x.CungUngId  == id     &&
                    x.LanguageId == LangId  &&
                    x.IsActivated == true
                );

            if (entity == null)
                return NotFound();

            // ── Increment view count (de-dup by session) ──────────────────
            string sessionKey = $"PageViewNCU_{id}";
            if (HttpContext.Session.GetString(sessionKey) == null)
            {
                entity.Viewed = (entity.Viewed ?? 0) + 1;
                HttpContext.Session.SetString(sessionKey, "1");
                _context.SaveChanges();
            }

            // ── Resolve category texts ────────────────────────────────────
            string linhVucText = ResolveCategoryText(entity.LinhVucId, "<br>");
            string dichVuText  = ResolveCategoryText(entity.DichVu,    "<br>");

            // ── Resolve LoaiHinhToChuc ────────────────────────────────────
            var loaiHinhMap = new Dictionary<string, string> {
                {"VienNC","Viện/Trung tâm nghiên cứu"}, {"TruongDH","Trường Đại học"},
                {"DNKHCN","Doanh nghiệp KH&CN"}, {"DNSX","Doanh nghiệp sản xuất"},
                {"ToChucTG","Tổ chức trung gian/Tư vấn"}, {"Khac","Khác"}
            };
            string loaiHinhText = "";
            if (!string.IsNullOrWhiteSpace(entity.LoaiHinhToChuc))
            {
                loaiHinhText = string.Join(", ",
                    entity.LoaiHinhToChuc.Split(';', StringSplitOptions.RemoveEmptyEntries)
                        .Select(code => loaiHinhMap.TryGetValue(code, out var t) ? t : code));
            }

            // ── Lượt đánh giá ─────────────────────────────────────────────
            int luotDanhGia = _context.EntityRatings
                .Count(x => x.EntityId == id && x.EntityType == TPApp.Enums.EntityTypes.NhaCungUng && x.StatusId == 1);

            // ── NhaCungUng khác (sidebar) ─────────────────────────────────
            var others = _context.NhaCungUngs
                .AsNoTracking()
                .Where(x => x.CungUngId != id && x.LanguageId == LangId && x.IsActivated == true)
                .OrderByDescending(x => x.Created)
                .Take(8)
                .AsEnumerable()
                .Select(x => new NhaCungUngItemVm
                {
                    Id       = x.CungUngId,
                    FullName = x.FullName ?? "",
                    Slug     = x.QueryString ?? x.FullName ?? "",
                    DiaChi   = x.DiaChi  ?? "",
                    Phone    = x.Phone   ?? "",
                    Email    = x.Email   ?? "",
                    Website  = x.Website ?? "",
                    Rating   = x.Rating  ?? 0,
                    ImageUrl = ProductController.CookedImageURL("254-170", x.HinhDaiDien, _mainDomain)
                })
                .ToList();

            // ── Category sidebar ──────────────────────────────────────────
            var categories = _context.Categories
                .AsNoTracking()
                .Where(x => x.ParentId == CateParentId && x.LanguageId == LangId)
                .OrderBy(x => x.Sort)
                .Select(x => new NhaCungUngCateVm { Id = x.CatId, Title = x.Title })
                .ToList();

            var vm = new NhaCungUngDetailVm
            {
                Id            = entity.CungUngId,
                Slug          = entity.QueryString ?? entity.FullName ?? "",
                FullName      = entity.FullName      ?? "",
                DiaChi        = entity.DiaChi        ?? "",
                Phone         = entity.Phone         ?? "",
                Fax           = entity.Fax           ?? "",
                Email         = entity.Email         ?? "",
                Website       = entity.Website       ?? "",
                NguoiDaiDien  = entity.NguoiDaiDien  ?? "",
                ChucVu        = entity.ChucVu        ?? "",
                ChucNang      = entity.ChucNangChinh ?? "",
                SanPham       = entity.SanPham        ?? "",
                LinhVucText   = linhVucText,
                DichVuText    = dichVuText,
                TenVietTat         = entity.TenVietTat ?? "",
                LoaiHinhToChucText = loaiHinhText,
                MaSoThue           = entity.MaSoThue ?? "",
                LogoUrl            = !string.IsNullOrEmpty(entity.Logo)
                    ? ImageHtmlHelper.ResolveImageUrl(entity.Logo, _mainDomain, "image/logoT.png") : null,
                VideoUrl           = entity.VideoUrl,
                ChungNhan          = entity.ChungNhan,
                Rating        = entity.Rating ?? 0,
                LuotXem       = entity.Viewed  ?? 0,
                LuotDanhGia   = luotDanhGia,
                ImageUrl      = ImageHtmlHelper.ResolveImageUrl(
                    !string.IsNullOrEmpty(entity.Logo) ? entity.Logo : entity.HinhDaiDien,
                    _mainDomain, "image/logoT.png"),
                NhaCungUngKhac = others,
                Categories     = categories
            };

            // ── Load products of this supplier ───────────────────────────
            vm.Products = _context.SanPhamCNTBs
                .AsNoTracking()
                .Where(p => p.NCUId == id && p.StatusId == 3)
                .OrderByDescending(p => p.PublishedDate)
                .Take(20)
                .AsEnumerable()
                .Select(p => new NhaCungUngProductVm
                {
                    Id          = p.ID,
                    Title       = p.Name ?? "",
                    Code        = p.Code ?? "",
                    Rating      = p.Rating ?? 0,
                    ProductType = p.ProductType,
                    PriceText   = p.OriginalPrice == null ? ""
                        : string.Format("{0:N0} {1}", p.OriginalPrice, p.Currency),
                    ImageUrl    = ProductController.CookedImageURL("254-170", p.QuyTrinhHinhAnh, _mainDomain),
                    Url         = $"/2-cong-nghe-thiet-bi/{p.ProductType}/{ProductController.MakeURLFriendly(p.Name)}-{p.ID}.html"
                })
                .ToList();

            return View(vm);
        }

        // =====================================================================
        // OLD-ROUTE REDIRECTS (301 Permanent)
        // =====================================================================
        [HttpGet("8-dich-vu-cung-ung/{slug}-{id:int}.html")]
        public IActionResult RedirectDetail(string slug, int id)
            => RedirectPermanent($"/nha-cung-ung/{slug}-{id}.html");

        // =====================================================================
        // PRIVATE HELPERS
        // =====================================================================

        /// <summary>
        /// Parses ";id1;id2;" format and resolves to category titles joined by separator.
        /// </summary>
        private string ResolveCategoryText(string? idString, string separator = ", ")
        {
            if (string.IsNullOrWhiteSpace(idString))
                return string.Empty;

            var ids = idString
                .Split(';', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => int.TryParse(s, out var n) ? n : -1)
                .Where(n => n > 0)
                .ToList();

            if (ids.Count == 0)
                return string.Empty;

            return string.Join(separator,
                _context.Categories
                    .AsNoTracking()
                    .Where(x => ids.Contains(x.CatId))
                    .OrderBy(x => x.Sort)
                    .Select(x => x.Title)
                    .ToList()
            );
        }

        private static List<int> BuildPages(int current, int total)
        {
            if (total <= 1) return total == 1 ? new List<int> { 1 } : new List<int>();
            int from = Math.Max(1, current - PageWindow);
            int to   = Math.Min(total, current + PageWindow);
            return Enumerable.Range(from, to - from + 1).ToList();
        }
    }
}
