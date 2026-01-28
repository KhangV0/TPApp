using Microsoft.AspNetCore.Mvc;
using TPApp.Data;
using TPApp.Entities;
using TPApp.ViewModel;

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
            var vm = new DichVuTuVanIndexVm
            {
                DichVuTuVan = new DichVuTuVanVm
                {
                    MenuId = menuId,
                    SelectedCateId = 0,
                    MainDomain = MainDomain,
                    CurrentPage = 1,
                    PageSize = 16
                }
            };

            return View(vm);
        }


        [HttpGet]
        public IActionResult DichVuTuVan(
            int menuId,
            string? cateId,
            int page = 1
        )
        {
            const int pageSize = 16;
            int lang = HttpContext.Session.GetInt32("LanguageId") ?? 1;

            // ===== giống Page_Load + BindToGrid =====
            var vm = new DichVuTuVanVm
            {
                MenuId = menuId,
                SelectedCateId = string.IsNullOrEmpty(cateId)
                    ? 0
                    : int.Parse(cateId),
                CurrentPage = page,
                PageSize = pageSize,
                MainDomain = MainDomain
            };


            vm.DichVuOptions.Add(new SelectItemVm
            {
                Value = "0",
                Text = "---Chọn dịch vụ tư vấn---"
            });

            vm.DichVuOptions.AddRange(
                _context.Categories
                    .Where(x => x.ParentId == 2)
                    .Select(x => new SelectItemVm
                    {
                        Value = x.CatId.ToString(),
                        Text = x.Title
                    })
                    .ToList()
            );

            string cateStr = ";" + (cateId ?? "") + ";";

            var baseQuery = _context.NhaTuVans
                .Where(q =>
                    q.LanguageId == lang &&
                    q.StatusId == 3 &&
                    (
                        cateStr == ";;" ||
                        q.DichVu.Contains(cateStr)
                    )
                )
                .OrderByDescending(q => q.Created);

            vm.TotalCount = baseQuery.Count();

            vm.Items = baseQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            int totalPage =
                vm.TotalCount % pageSize == 0
                    ? vm.TotalCount / pageSize
                    : vm.TotalCount / pageSize + 1;

            int page2Show = 10;

            IEnumerable<int> leftPages =
                page <= page2Show
                    ? Enumerable.Range(1, page)
                    : Enumerable.Range(page - page2Show, page2Show);

            IEnumerable<int> rightPages =
                page + page2Show <= totalPage
                    ? Enumerable.Range(page, page2Show + 1)
                    : Enumerable.Range(page, totalPage - page + 1);

            vm.Pages = leftPages
                .Union(rightPages)
                .Distinct()
                .ToList();


            return PartialView("DichVuTuVan", vm);
        }

        [HttpGet]
        public IActionResult DichVuCungUng(
            int menuId,
            int industryId = 0,
            int page = 1
)
        {
            const int pageSize = 16;
            const int page2Show = 10;

            int lang = HttpContext.Session.GetInt32("LanguageId") ?? 1;

            var vm = new DichVuTuVanVm
            {
                MenuId = menuId,
                SelectedCateId = industryId,
                CurrentPage = page,
                PageSize = pageSize,
                MainDomain = MainDomain
            };

            // Dropdown
            vm.DichVuOptions.Add(new SelectItemVm
            {
                Value = "0",
                Text = "---Chọn dịch vụ tư vấn---"
            });

            vm.DichVuOptions.AddRange(
                _context.Categories
                    .Where(x => x.ParentId == 2)
                    .OrderBy(x => x.Sort)
                    .Select(x => new SelectItemVm
                    {
                        Value = x.CatId.ToString(),
                        Text = x.Title
                    })
                    .ToList()
            );

            string linhVucStr = ";" + industryId + ";";

            var baseQuery = _context.NhaCungUngs
                .Where(q =>
                    q.LanguageId == lang &&
                    q.IsActivated == true &&
                    (
                        industryId == 0 ||
                        (q.DichVu != null && q.DichVu.Contains(linhVucStr))
                    )
                )
                .OrderByDescending(q => q.Created);

            vm.TotalCount = baseQuery.Count();

            vm.ItemsCungUng = baseQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            int totalPage =
                vm.TotalCount % pageSize == 0
                    ? vm.TotalCount / pageSize
                    : vm.TotalCount / pageSize + 1;

            IEnumerable<int> leftPages =
                page <= page2Show
                    ? Enumerable.Range(1, page)
                    : Enumerable.Range(page - page2Show, page2Show);

            IEnumerable<int> rightPages =
                page + page2Show <= totalPage
                    ? Enumerable.Range(page, page2Show + 1)
                    : Enumerable.Range(page, totalPage - page + 1);

            vm.Pages = leftPages
                .Union(rightPages)
                .Distinct()
                .ToList();

            return PartialView("DichVuCungUng", vm);
        }


        [HttpGet("8-dich-vu-tu-van/{slug}-{id}.html")]
        public IActionResult ChiTietNhaTuVan(int id)
        {
            int lang = HttpContext.Session.GetInt32("LanguageId") ?? 1;

            var entity = _context.NhaTuVans
                .FirstOrDefault(x =>
                    x.TuVanId == id &&
                    x.LanguageId == lang &&
                    x.StatusId == 3
                );

            if (entity == null)
                return NotFound();

            // ===== DỊCH VỤ =====
            string dichVuText = string.Empty;
            if (!string.IsNullOrWhiteSpace(entity.DichVu))
            {
                var dichVuIds = entity.DichVu
                    .Split(';', StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => int.Parse(x))
                    .ToList();

                dichVuText = string.Join(", ",
                    _context.Categories
                        .Where(x =>
                            dichVuIds.Contains(x.CatId) &&
                            x.LanguageId == lang
                        )
                        .OrderBy(x => x.Sort)
                        .Select(x => x.Title)
                        .ToList()
                );
            }

            // ===== LĨNH VỰC =====
            string linhVucText = string.Empty;
            if (!string.IsNullOrWhiteSpace(entity.LinhVucId))
            {
                var linhVucIds = entity.LinhVucId
                    .Split(';', StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => int.Parse(x))
                    .ToList();

                linhVucText = string.Join(", ",
                    _context.Categories
                        .Where(x =>
                            linhVucIds.Contains(x.CatId) &&
                            x.LanguageId == lang
                        )
                        .OrderBy(x => x.Sort)
                        .Select(x => x.Title)
                        .ToList()
                );
            }

            var vm = new ChiTietNhaTuVanVm
            {
                Id = entity.TuVanId,
                LinhVucId = entity.LinhVucId,
                FullName = entity.FullName,
                DiaChi = entity.DiaChi,
                NgaySinh = entity.DateOfBirth,
                Phone = entity.Phone,
                Email = entity.Email,
                HocHam = entity.HocHam,
                CoQuan = entity.CoQuan,
                ChucVu = entity.ChucVu,

                Rating = entity.Rating ?? 0,
                LuotDanhGia = 0,
                LuotXem = entity.Viewed ?? 0,

                ImageUrl = string.IsNullOrEmpty(entity.HinhDaiDien)
                    ? $"{MainDomain}image/NoAvarta.jpg"
                    : $"{MainDomain}{entity.HinhDaiDien}",

                DichVuText = dichVuText,
                LinhVucText = linhVucText,
                KetQuaNghienCuu = entity.KetQuaNghienCuu
            };

            // ===== TAGS =====
            if (!string.IsNullOrWhiteSpace(entity.Keywords))
            {
                vm.TuKhoa = entity.Keywords
                    .Split(';', StringSplitOptions.RemoveEmptyEntries)
                    .ToList();
            }

            // ===== NHÀ TƯ VẤN KHÁC =====
            vm.NhaTuVanKhac = _context.NhaTuVans
                .Where(x =>
                    x.TuVanId != entity.TuVanId &&
                    x.LanguageId == lang &&
                    x.StatusId == 3
                )
                .OrderByDescending(x => x.Created)
                .Take(8)
                .Select(x => new NhaTuVanItemVm
                {
                    Id = x.TuVanId,
                    FullName = x.FullName,
                    ImageUrl = string.IsNullOrEmpty(x.HinhDaiDien)
                        ? $"{MainDomain}image/NoImages.jpg"
                        : $"{MainDomain}{x.HinhDaiDien}",
                    CoQuan = x.CoQuan,
                    Phone = x.Phone,
                    Email = x.Email,
                    Rating = x.Rating ?? 0
                })
                .ToList();

            vm.Categories = _context.Categories
                .Where(x =>
                    x.ParentId == 2 &&
                    x.LanguageId == lang
                )
                .OrderBy(x => x.Sort)
                .Select(x => new CategoryVm
                {
                    Id = x.CatId,
                    Name = x.Title,
                    Url = "/dich-vu-tu-van-8.html"
                })
                .ToList();

            entity.Viewed = (entity.Viewed ?? 0) + 1;
            _context.SaveChanges();

            return View("ChiTietNhaTuVan", vm);
        }

        [HttpGet("8-dich-vu-cung-ung/{slug}-{id}.html")]
        public IActionResult ChiTietNhaCungUng(int id)
        {
            int lang = HttpContext.Session.GetInt32("LanguageId") ?? 1;
            int userId = HttpContext.Session.GetInt32("UserID") ?? 0;

            var entity = _context.NhaCungUngs
                .FirstOrDefault(x =>
                    x.CungUngId == id &&
                    x.LanguageId == lang &&
                    x.StatusId == 3
                );

            if (entity == null)
                return NotFound();

            // ================== LĨNH VỰC ==================
            string linhVucText = "";
            if (!string.IsNullOrWhiteSpace(entity.LinhVucId))
            {
                var ids = entity.LinhVucId
                    .Split(';', StringSplitOptions.RemoveEmptyEntries)
                    .Select(int.Parse)
                    .ToList();

                linhVucText = string.Join("<br>",
                    _context.Categories
                        .Where(x => ids.Contains(x.CatId))
                        .OrderBy(x => x.Sort)
                        .Select(x => x.Title)
                        .ToList()
                );
            }

            // ================== DỊCH VỤ ==================
            string dichVuText = "";
            if (!string.IsNullOrWhiteSpace(entity.DichVu))
            {
                var ids = entity.DichVu
                    .Split(';', StringSplitOptions.RemoveEmptyEntries)
                    .Select(int.Parse)
                    .ToList();

                dichVuText = string.Join("<br>",
                    _context.Categories
                        .Where(x => ids.Contains(x.CatId))
                        .OrderBy(x => x.Sort)
                        .Select(x => x.Title)
                        .ToList()
                );
            }

            // ================== VIEW MODEL ==================
            var vm = new ChiTietNhaCungUngVm
            {
                Id = entity.CungUngId,
                FullName = entity.FullName,
                DiaChi = entity.DiaChi,
                Phone = entity.Phone,
                Fax = entity.Fax,
                Email = entity.Email,
                Website = entity.Website,
                NguoiDaiDien = entity.NguoiDaiDien,
                ChucVu = entity.ChucVu,
                ChucNang = entity.ChucNangChinh,
                SanPham = entity.SanPham,

                LinhVucText = linhVucText,
                DichVuText = dichVuText,

                Rating = entity.Rating ?? 0,
                LuotXem = entity.Viewed ?? 0,

                ImageUrl = string.IsNullOrEmpty(entity.HinhDaiDien)
                    ? $"{MainDomain}/image/logoT.png"
                    : $"{MainDomain}{entity.HinhDaiDien}"
            };

            // ================== NHÀ CUNG ỨNG KHÁC ==================
            vm.NhaCungUngKhac = _context.NhaCungUngs
                .Where(x =>
                    x.CungUngId != entity.CungUngId &&
                    x.LanguageId == lang &&
                    x.StatusId == 3
                )
                .OrderByDescending(x => x.Created)
                .Take(8)
                .Select(x => new NhaCungUngItemVm
                {
                    Id = x.CungUngId,
                    FullName = x.FullName,
                    DiaChi = x.DiaChi,
                    Phone = x.Phone,
                    Email = x.Email,
                    Website = x.Website,
                    Rating = x.Rating ?? 0,
                    ImageUrl = string.IsNullOrEmpty(x.HinhDaiDien)
                        ? $"{MainDomain}image/NoImages.jpg"
                        : $"{MainDomain}{x.HinhDaiDien}"
                })
                .ToList();

            // ================== CATEGORY LEFT ==================
            vm.Categories = _context.Categories
                .Where(x => x.ParentId == 2 && x.MainCate == true)
                .OrderBy(x => x.Sort)
                .Select(x => new CategoryVm
                {
                    Id = x.CatId,
                    Name = x.Title,
                    Url = $"{MainDomain}8-ds-dich-vu-tu-van/{x.QueryString}-{x.CatId}.html"
                })
                .ToList();

            // ================== UPDATE VIEW ==================
            string sessionKey = $"PageViewNCU_{id}";
            if (HttpContext.Session.GetString(sessionKey) == null)
            {
                entity.Viewed = (entity.Viewed ?? 0) + 1;
                HttpContext.Session.SetString(sessionKey, DateTime.Now.ToString());
                _context.SaveChanges();
            }

            // ================== LƯỢT ĐÁNH GIÁ ==================
            vm.LuotDanhGia = _context.Ratings
                .Count(x => x.SanPhamId == id && x.TypeID == 8);

            return View("ChiTietNhaCungUng", vm);
        }


        [HttpGet("8-ds-dich-vu-tu-van/{slug}-{cateId}.html")]
        public IActionResult DanhSachTheoCate(
            int cateId,
            int page = 1
        )
        {
            const int pageSize = 16;
            int lang = HttpContext.Session.GetInt32("LanguageId") ?? 1;

            var vm = new DichVuTuVanIndexVm
            {
                DichVuTuVan = new DichVuTuVanVm
                {
                    MenuId = 8,
                    SelectedCateId = cateId,
                    CurrentPage = page,
                    PageSize = pageSize,
                    MainDomain = MainDomain
                }
            };

            return View("Index", vm);
        }

        [HttpPost]
        public IActionResult AddToCart(int productId)
        {
            var cartId = HttpContext.Session.GetString("CartId");
            int? userId = HttpContext.Session.GetInt32("UserId");

            var exists = _context.ShoppingCarts.FirstOrDefault(x =>
                x.CartId == cartId &&
                x.ProductId == productId &&
                x.TypeId == 2
            );

            if (exists == null)
            {
                _context.ShoppingCarts.Add(new ShoppingCart
                {
                    CartId = cartId,
                    ProductId = productId,
                    UserId = userId,
                    Quantity = 1,
                    DateCreated = DateTime.Now,
                    TypeId = 2,
                    Domain = MainDomain
                });

                _context.SaveChanges();
            }

            return Redirect($"{MainDomain}gio-hang.html");
        }



    }
}
