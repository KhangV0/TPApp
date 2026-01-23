using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using TPApp.Data;
using TPApp.Entities;
using TPApp.ViewModel;

namespace TPApp.Controllers
{
    public class TimKiemDoiTacController : Controller
    {
        private readonly AppDbContext _context;

        private const string MainDomain = "https://localhost:7232/";
        private const int DEFAULT_CAT_ID = 3;

        public TimKiemDoiTacController(AppDbContext context)
        {
            _context = context;
        }

        [Route("tim-kiem-doi-tac-11.html")]
        public IActionResult Index()
        {
            var model = new TimKiemDoiTacIndexVm
            {
                Categories = LoadData(DEFAULT_CAT_ID)
            };

            return View(model);
        }

        // ================= DETAIL =================
        [Route("11-tim-kiem-doi-tac/{slug}-{id}.html")]
        public IActionResult Detail(string slug, int id)
        {
            var lang = HttpContext.Session.GetInt32("LanguageId") ?? 1;

            var p = _context.TimKiemDoiTacs
                .FirstOrDefault(x =>
                    x.TimDoiTacId == id &&
                    x.LanguageId == lang &&
                    x.IsActivated == true &&
                    x.StatusId == 3
                );

            if (p == null)
                return NotFound();

            // ====== Tăng lượt xem (giữ logic WebForms) ======
            var sessionKey = $"PageViewTimKiemDoiTac_{id}";
            if (HttpContext.Session.GetString(sessionKey) == null)
            {
                p.Viewed = (p.Viewed ?? 0) + 1;
                _context.SaveChanges();
                HttpContext.Session.SetString(sessionKey, "1");
            }

            var model = new TimKiemDoiTacDetailVm
            {
                TimDoiTacId = p.TimDoiTacId,
                TenSanPham = p.TenSanPham,
                TenDonVi = p.TenDonVi,
                FullName = p.FullName,
                DiaChi = p.DiaChi,
                Phone = p.Phone,
                Email = p.Email,
                MoTa = p.MoTa,
                HinhThuc = p.HinhThuc,
                Rating = p.Rating,
                Viewed = p.Viewed ?? 0,
                CategoryId = p.CategoryId,
                ImageUrl = string.IsNullOrEmpty(p.HinhDaiDien)
                    ? $"{MainDomain}images/research.jpg"
                    : p.HinhDaiDien
            };

            // ====== Sản phẩm liên quan ======
            model.RelatedItems = _context.TimKiemDoiTacs
                .Where(x =>
                    x.LanguageId == lang &&
                    x.IsActivated == true &&
                    x.StatusId == 3 &&
                    x.SanPhamId == p.SanPhamId &&
                    x.TimDoiTacId != id
                )
                .OrderByDescending(x => x.Created)
                .Take(8)
                .Select(x => new TimKiemDoiTacItemVm
                {
                    TimDoiTacId = x.TimDoiTacId,
                    TenSanPham = x.TenSanPham,
                    FullName = x.FullName,
                    Rating = x.Rating,
                    ImageUrl = string.IsNullOrEmpty(x.HinhDaiDien)
                        ? $"{MainDomain}images/research.jpg"
                        : CookedImageURL("254-170", x.HinhDaiDien),
                    Url = $"{MainDomain}11-tim-kiem-doi-tac/" +
                          $"{MakeURLFriendly(x.TenSanPham)}-{x.TimDoiTacId}.html"
                })
                .ToList();

            // ====== Danh mục bên trái ======
            model.Categories = _context.Categories
                .Where(x => x.ParentId == DEFAULT_CAT_ID && (x.MainCate ?? false))
                .OrderBy(x => x.Sort)
                .Select(x => new TimKiemDoiTacCategoryVm
                {
                    CatId = x.CatId,
                    Title = x.Title,
                    Url = $"{MainDomain}11-ds-tim-kiem-doi-tac/" +
                          $"{MakeURLFriendly(x.QueryString)}-{x.CatId}.html",
                    Products = new List<TimKiemDoiTacItemVm>() // không load SP ở đây
                })
                .ToList();

            return View("Detail", model);
        }

        // ================= LIST BY CATEGORY =================
        [Route("11-ds-tim-kiem-doi-tac/{slug}-{cateId}.html")]
        public IActionResult List(string slug, int cateId, int page = 1)
        {
            var lang = HttpContext.Session.GetInt32("LanguageId") ?? 1;
            const int pageSize = 12;

            // GIỮ LOGIC CŨ: categoryId dạng ;18;
            var cateToken = $";{cateId};";

            var query = _context.TimKiemDoiTacs
                .Where(x =>
                    x.LanguageId == lang &&
                    x.StatusId == 3 &&
                    x.IsActivated == true &&
                    x.SanPhamId != null &&
                    EF.Functions.Like(
                        ";" + x.SanPhamId.Replace(" ", "") + ";",
                        $"%{cateToken}%"
                    ))
                .OrderByDescending(x => x.Created);

            var total = query.Count();

            var items = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new TimKiemDoiTacItemVm
                {
                    TimDoiTacId = x.TimDoiTacId,
                    TenSanPham = x.TenSanPham,
                    FullName = x.FullName,
                    Rating = x.Rating,
                    ImageUrl = string.IsNullOrEmpty(x.HinhDaiDien)
                        ? $"{MainDomain}images/research.jpg"
                        : x.HinhDaiDien,
                    Url = $"{MainDomain}11-tim-kiem-doi-tac/" +
                          $"{MakeURLFriendly(x.TenSanPham)}-{x.TimDoiTacId}.html"
                })
                .ToList();

            var cate = _context.Categories.FirstOrDefault(x => x.CatId == cateId);

            var model = new TimKiemDoiTacListVm
            {
                CateId = cateId,
                CateTitle = cate?.Title ?? "",
                Total = total,
                Page = page,
                PageSize = pageSize,
                Items = items,

                Categories = _context.Categories
                    .Where(x => x.ParentId == DEFAULT_CAT_ID && (x.MainCate ?? false))
                    .OrderBy(x => x.Sort)
                    .Select(x => new TimKiemDoiTacCategoryVm
                    {
                        CatId = x.CatId,
                        Title = x.Title,
                        Url = $"{MainDomain}11-ds-tim-kiem-doi-tac/" +
                              $"{MakeURLFriendly(x.QueryString)}-{x.CatId}.html"
                    })
                    .ToList()
            };

            return View("List", model);
        }


        // ================= LOAD DATA =================
        private List<TimKiemDoiTacCategoryVm> LoadData(int catId)
        {
            var lang = HttpContext.Session.GetInt32("LanguageId") ?? 1;

            var categories = _context.Categories
                .Where(x => x.ParentId == catId && x.MainCate == true)
                .OrderBy(x => x.Sort)
                .ToList();

            var result = new List<TimKiemDoiTacCategoryVm>();

            foreach (var c in categories)
            {
                var products = _context.TimKiemDoiTacs
                    .Where(x =>
                        x.LanguageId == lang &&
                        x.IsActivated == true &&         
                        x.StatusId == 3 &&
                        x.SanPhamId.Contains(c.CatId.ToString()) 
                    )
                    .OrderByDescending(x => x.Created)
                    .Take(8)
                    .Select(x => new TimKiemDoiTacItemVm
                    {
                        TimDoiTacId = x.TimDoiTacId,
                        TenSanPham = x.TenSanPham,
                        FullName = x.FullName,
                        Rating = x.Rating,
                        ImageUrl = string.IsNullOrEmpty(x.HinhDaiDien)
                            ? $"{MainDomain}images/research.jpg"
                            : CookedImageURL("254-170", x.HinhDaiDien),
                        Url = $"{MainDomain}11-tim-kiem-doi-tac/" +
                              $"{MakeURLFriendly(x.TenSanPham)}-{x.TimDoiTacId}.html"
                    })
                    .ToList();

                result.Add(new TimKiemDoiTacCategoryVm
                {
                    CatId = c.CatId,
                    Title = c.Title,
                    Url = $"{MainDomain}11-ds-tim-kiem-doi-tac/" +
                          $"{MakeURLFriendly(c.QueryString)}-{c.CatId}.html",
                    Products = products
                });
            }

            return result;
        }

        // ================= ADD CART =================
        [HttpPost]
        public IActionResult AddCart(int productId)
        {
            AddShoppingCart(productId);
            HttpContext.Session.SetString("LastURL", Request.Headers["Referer"].ToString());
            return Redirect($"{MainDomain}gio-hang.html");
        }

        private void AddShoppingCart(int productId)
        {
            var cartId = HttpContext.Session.GetString("CartId");
            var userId = HttpContext.Session.GetInt32("UserId");

            if (string.IsNullOrEmpty(cartId)) return;

            var exists = _context.ShoppingCarts
                .FirstOrDefault(x =>
                    x.CartId == cartId &&
                    x.ProductId == productId &&
                    x.TypeId == 4);

            if (exists == null)
            {
                _context.ShoppingCarts.Add(new ShoppingCart
                {
                    CartId = cartId,
                    ProductId = productId,
                    UserId = userId,
                    Quantity = 1,
                    TypeId = 4,
                    DateCreated = DateTime.Now,
                    Domain = "DomainName" // giữ nguyên logic cũ
                });

                _context.SaveChanges();
            }
        }

        public static string CookedImageURL(string size, string? imageUrl)
        {
            var mainDomain = MainDomain;

            if (string.IsNullOrWhiteSpace(imageUrl))
            {
                return $"{mainDomain.TrimEnd('/')}/images/{size}_noImage.jpg";
            }

            if (!imageUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                imageUrl = $"{mainDomain.TrimEnd('/')}/{imageUrl.TrimStart('/')}";
            }

            var fileName = Path.GetFileName(imageUrl);

            // Tránh double size
            if (fileName.StartsWith(size + "-", StringComparison.OrdinalIgnoreCase))
                return imageUrl;

            return imageUrl.Replace(fileName, $"{size}-{fileName}");
        }

        public static string MakeURLFriendly(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            var str = input.ToLower().Trim();
            var old = str;

            // Bảng chuyển dấu tiếng Việt (giữ logic VB.NET)
            const string findText =
                "ä|à|á|ạ|ả|ã|â|ầ|ấ|ậ|ẩ|ẫ|ă|ằ|ắ|ặ|ẳ|ẵ|" +
                "ç|" +
                "è|é|ẹ|ẻ|ẽ|ê|ề|ế|ệ|ể|ễ|" +
                "ì|í|î|ị|ỉ|ĩ|" +
                "ö|ò|ó|ọ|ỏ|õ|ô|ồ|ố|ộ|ổ|ỗ|ơ|ờ|ớ|ợ|ở|ỡ|" +
                "ü|ù|ú|ụ|ủ|ũ|ư|ừ|ứ|ự|ử|ữ|" +
                "ỳ|ý|ỵ|ỷ|ỹ|" +
                "đ";

            const string replaceText =
                "a|a|a|a|a|a|a|a|a|a|a|a|a|a|a|a|a|a|" +
                "c|" +
                "e|e|e|e|e|e|e|e|e|e|e|" +
                "i|i|i|i|i|i|" +
                "o|o|o|o|o|o|o|o|o|o|o|o|o|o|o|o|o|o|" +
                "u|u|u|u|u|u|u|u|u|u|u|u|" +
                "y|y|y|y|y|" +
                "d";

            var findArr = findText.Split('|');
            var replaceArr = replaceText.Split('|');

            for (int i = 0; i < findArr.Length; i++)
            {
                str = str.Replace(findArr[i], replaceArr[i]);
            }

            // Thay ký tự đặc biệt bằng "-"
            str = Regex.Replace(str, @"[^a-z0-9]", "-");

            // Gom dấu "-"
            str = Regex.Replace(str, @"-+", "-").Trim('-');

            // Trường hợp tiếng Hán / quá ngắn (giữ logic cũ)
            if (str.Length < 3)
            {
                str = old
                    .Replace(" ", "-")
                    .Replace(".", "-")
                    .Replace("?", "-");
            }

            return str;
        }


    }
}
