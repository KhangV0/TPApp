using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TPApp.Data;
using TPApp.Entities;
using TPApp.Helpers;
using TPApp.ViewModel;

namespace TPApp.Controllers
{
    public class ChuyenGiaController : Controller
    {
        private readonly AppDbContext _context;
        private readonly string       _mainDomain;

        private const int LangId        = 1;
        private const int DefaultPageSize = 16;
        private const int PageWindow     = 5;   // pages shown left/right of current
        private const int CateParentId   = 2;   // Categories.ParentId for DichVu filter

        public ChuyenGiaController(AppDbContext context, IOptions<AppSettings> appSettings)
        {
            _context    = context;
            _mainDomain = appSettings.Value.MainDomain;
        }

        // =====================================================================
        // GET /chuyen-gia
        // GET /chuyen-gia?cateId=5&page=2
        // =====================================================================
        [HttpGet("chuyen-gia.html")]
        public IActionResult Index(int cateId = 0, int page = 1)
        {
            page = Math.Max(1, page);

            // ── Category sidebar ──────────────────────────────────────────
            var categories = _context.Categories
                .AsNoTracking()
                .Where(x => x.ParentId == CateParentId && x.LanguageId == LangId)
                .OrderBy(x => x.Sort)
                .Select(x => new ChuyenGiaCateVm { Id = x.CatId, Title = x.Title })
                .ToList();

            // ── Base query ────────────────────────────────────────────────
            // Filter by category: DichVu is stored as ";id1;id2;id3;"
            IQueryable<NhaTuVan> baseQuery = _context.NhaTuVans
                .AsNoTracking()
                .Where(x => x.LanguageId == LangId && x.StatusId == 3);

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
                .Select(x => new ChuyenGiaItemVm
                {
                    Id       = x.TuVanId,
                    FullName = x.FullName ?? "",
                    Slug     = x.QueryString ?? x.FullName ?? "",
                    CoQuan   = x.CoQuan   ?? "",
                    ChucVu   = x.ChucVu   ?? "",
                    Phone    = x.Phone    ?? "",
                    Email    = x.Email    ?? "",
                    Rating   = x.Rating   ?? 0,
                    ImageUrl = string.IsNullOrEmpty(x.HinhDaiDien)
                        ? $"{_mainDomain}image/NoAvarta.jpg"
                        : $"{x.HinhDaiDien}"
                })
                .ToList();

            var vm = new ChuyenGiaIndexVm
            {
                CateId     = cateId,
                CurrentPage = page,
                PageSize   = DefaultPageSize,
                TotalCount  = totalCount,
                Items       = items,
                Categories  = categories,
                Pages       = BuildPages(page, (int)Math.Ceiling((double)totalCount / DefaultPageSize))
            };

            return View(vm);
        }

        // =====================================================================
        // GET /chuyen-gia/{slug}-{id}.html
        // =====================================================================
        [HttpGet("chuyen-gia/{slug}-{id:int}.html")]
        public IActionResult Detail(string slug, int id)
        {
            var entity = _context.NhaTuVans
                .FirstOrDefault(x =>
                    x.TuVanId    == id    &&
                    x.LanguageId == LangId &&
                    x.StatusId   == 3
                );

            if (entity == null)
                return NotFound();

            // ── Increment view count ──────────────────────────────────────
            entity.Viewed = (entity.Viewed ?? 0) + 1;
            _context.SaveChanges();

            // ── Resolve DichVu text ───────────────────────────────────────
            string dichVuText = ResolveCategoryText(entity.DichVu, LangId);

            // ── Resolve LinhVuc text ──────────────────────────────────────
            string linhVucText = ResolveCategoryText(entity.LinhVucId, LangId);

            // ── NhaTuVan khác (sidebar) ───────────────────────────────────
            var others = _context.NhaTuVans
                .AsNoTracking()
                .Where(x => x.TuVanId != id && x.LanguageId == LangId && x.StatusId == 3)
                .OrderByDescending(x => x.Created)
                .Take(8)
                .Select(x => new ChuyenGiaItemVm
                {
                    Id       = x.TuVanId,
                    FullName = x.FullName ?? "",
                    Slug     = x.QueryString ?? x.FullName ?? "",
                    CoQuan   = x.CoQuan ?? "",
                    Phone    = x.Phone  ?? "",
                    Email    = x.Email  ?? "",
                    Rating   = x.Rating ?? 0,
                    ImageUrl = string.IsNullOrEmpty(x.HinhDaiDien)
                        ? $"{_mainDomain}image/NoAvarta.jpg"
                        : $"{_mainDomain}{x.HinhDaiDien}"
                })
                .ToList();

            // ── Category sidebar ──────────────────────────────────────────
            var categories = _context.Categories
                .AsNoTracking()
                .Where(x => x.ParentId == CateParentId && x.LanguageId == LangId)
                .OrderBy(x => x.Sort)
                .Select(x => new ChuyenGiaCateVm { Id = x.CatId, Title = x.Title })
                .ToList();

            var vm = new ChuyenGiaDetailVm
            {
                Id          = entity.TuVanId,
                Slug        = entity.QueryString ?? entity.FullName ?? "",
                FullName    = entity.FullName    ?? "",
                DateOfBirth = entity.DateOfBirth ?? "",
                DiaChi      = entity.DiaChi      ?? "",
                Phone       = entity.Phone       ?? "",
                Email       = entity.Email       ?? "",
                HocHam      = entity.HocHam      ?? "",
                CoQuan      = entity.CoQuan      ?? "",
                ChucVu      = entity.ChucVu      ?? "",
                DichVuText  = dichVuText,
                LinhVucText = linhVucText,
                KetQuaNghienCuu = entity.KetQuaNghienCuu ?? "",
                Rating      = entity.Rating  ?? 0,
                LuotXem     = entity.Viewed  ?? 0,
                ImageUrl    = string.IsNullOrEmpty(entity.HinhDaiDien)
                    ? $"{_mainDomain}image/NoAvarta.jpg"
                    : $"{_mainDomain}{entity.HinhDaiDien}",
                TuKhoa = string.IsNullOrWhiteSpace(entity.Keywords)
                    ? new List<string>()
                    : entity.Keywords.Split(';', StringSplitOptions.RemoveEmptyEntries).ToList(),
                NhaTuVanKhac = others,
                Categories   = categories
            };

            return View(vm);
        }


        // =====================================================================
        // PRIVATE HELPERS
        // =====================================================================

        /// <summary>
        /// Parses a semicolon-delimited ID string (e.g. ";1;3;7;") into category titles.
        /// </summary>
        private string ResolveCategoryText(string? idString, int langId)
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

            return string.Join(", ",
                _context.Categories
                    .AsNoTracking()
                    .Where(x => ids.Contains(x.CatId) && x.LanguageId == langId)
                    .OrderBy(x => x.Sort)
                    .Select(x => x.Title)
                    .ToList()
            );
        }

        /// <summary>
        /// Builds the visible page number window around <paramref name="current"/>.
        /// Returns at most (PageWindow * 2 + 1) page numbers.
        /// </summary>
        private static List<int> BuildPages(int current, int total)
        {
            if (total <= 1) return total == 1 ? new List<int> { 1 } : new List<int>();
            int from = Math.Max(1, current - PageWindow);
            int to   = Math.Min(total, current + PageWindow);
            return Enumerable.Range(from, to - from + 1).ToList();
        }
    }
}
