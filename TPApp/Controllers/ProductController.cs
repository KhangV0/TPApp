using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.RegularExpressions;
using TPApp.Data;
using TPApp.Entities;
using TPApp.ViewModel;

namespace TPApp.Controllers
{
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;
        private const string MainDomain = "https://localhost:7232/";

        public ProductController(AppDbContext context)
        {
            _context = context;
        }

        // ================== INDEX (PORT TỪ WEBFORMS) ==================
        [Route("cong-nghe-thiet-bi-2.html")]
        public IActionResult Index(int catId = 1)
        {
            var lang = HttpContext.Session.GetInt32("LanguageId") ?? 1;

            var categories = _context.Categories
                .Where(x => x.ParentId == catId && x.MainCate == true)
                .OrderBy(x => x.Sort)
                .ToList();

            var model = new ProductIndexViewModel
            {
                ProductCNMoiCapNhatHtml = LoadProductCongNgheMoiCapNhat(12, 4)
            };     

            foreach (var cate in categories)
            {
                var cateKey = ";" + cate.CatId + ";";

                var products = _context.SanPhamCNTBs
                    .Where(x =>
                        x.LanguageId == lang &&
                        x.StatusId == 3 &&
                        x.CategoryId.Contains(cateKey))
                    .OrderByDescending(x => x.Created)
                    .Take(4)
                    .ToList();

                model.Categories.Add(new CategoryBlockVm
                {
                    Category = cate,
                    Products = products
                });
            }

            return View(model);
        }


        private string LoadProductCongNgheMoiCapNhat(int take, int perSlide)
        {
            var list = _context.SanPhamCNTBs
                .Where(x => x.StatusId == 3
                         && x.LanguageId == 1
                         && x.bEffectiveDate <= DateTime.Now
                         && x.eEffectiveDate >= DateTime.Now)
                .OrderByDescending(x => x.Modified)
                .ThenByDescending(x => x.Created)
                .Take(take)
                .ToList();

            return BuildCarouselSlides(list, perSlide);
        }

        private string BuildCarouselSlides(List<SanPhamCNTB> list, int perSlide)
        {
            if (!list.Any()) return "";

            var sb = new StringBuilder();
            int slideIndex = 0;

            for (int i = 0; i < list.Count; i += perSlide)
            {
                slideIndex++;
                var group = list.Skip(i).Take(perSlide);

                sb.Append($"<div class='carousel-item {(slideIndex == 1 ? "active" : "")}'>");
                sb.Append("<div class='row justify-content-center text-center'>");

                foreach (var item in group)
                {
                    string imgUrl = CookedImageURL("254-170", item.QuyTrinhHinhAnh);
                    string url = MainDomain + "2-cong-nghe-thiet-bi/" + item.TypeId + "/" +
                                 MakeURLFriendly(item.Name) + '-' + item.ID + ".html";

                    sb.Append($@"
                        <div class='col-md-2 col-6 mb-4'>
                            <a href='{url}' class='card border-0 tech-card'>
                                <img src='{imgUrl}' class='img-fluid' />
                                <small>{item.Name}</small>
                            </a>
                        </div>");
                }

                sb.Append("</div></div>");
            }

            return sb.ToString();
        }



        // ================== ADD CART (PORT TỪ btnAddCart_Click) ==================
        [HttpPost]
        public IActionResult AddCart(int productId)
        {
            var cartId = HttpContext.Session.GetString("CartId");
            var userId = HttpContext.Session.GetInt32("UserId");

            if (string.IsNullOrEmpty(cartId))
                return Redirect($"{MainDomain}gio-hang.html");

            var check = _context.ShoppingCarts.FirstOrDefault(x =>
                x.CartId == cartId &&
                x.ProductId == productId &&
                x.TypeId == 1);

            if (check == null)
            {
                var cart = new ShoppingCart
                {
                    CartId = cartId,
                    ProductId = productId,
                    UserId = userId,
                    DateCreated = DateTime.Now,
                    Quantity = 1,
                    TypeId = 1,
                    Domain = HttpContext.Request.Host.Value
                };

                _context.ShoppingCarts.Add(cart);
                _context.SaveChanges();
            }

            HttpContext.Session.SetString("LastURL", Request.Path);
            return Redirect($"{MainDomain}gio-hang.html");
        }

        // ================== DETAIL (GIỮ NGUYÊN) ==================
        [Route("{menu:int}-cong-nghe-thiet-bi/{typeId:int}/{slug}-{id:int}.html")]

        public IActionResult Detail(int id)
        {
            if (id == 1311)
                return RedirectPermanent("http://techport.vn/2-cong-nghe-thiet-bi/1/thiet-bi-dong-goi-hut-chan-khong-1311.html");

            if (id == 8512)
                return RedirectPermanent("http://techport.vn/2-cong-nghe-thiet-bi/1/thiet-bi-dong-goi-bot-tu-dong-goi-lon--8512.html");

            var product = _context.SanPhamCNTBs
                .FirstOrDefault(x => x.ID == id && x.LanguageId == 1 && x.StatusId == 3);

            if (product == null)
                return Redirect($"{MainDomain}cong-nghe-thiet-bi-2.html");

            var model = new ProductDetailViewModel
            {
                Product = product,
                TypeId = product.TypeId ?? 0,
                CategoryTitle = GetCategoryTitle(product.CategoryId),
                Industries = GetIndustries(),
                Suppliers = GetSuppliers(),
                Images = GetImages(id),
                RelatedCategories = GetRelatedCategories(1),
                Keywords = GetKeywords(id),
                RatingCount = GetRatingCount(id, product.TypeId ?? 1)
            };

            MapSupplier(model, product);
            MapVideo(model, product);
            UpdateViewCount(product);

            return View(model);
        }

        // ================== HELPERS (GIỮ NGUYÊN) ==================
        private string GetCategoryTitle(string categoryIds)
        {
            if (string.IsNullOrEmpty(categoryIds)) return "";

            var ids = categoryIds.Split(";").Where(x => !string.IsNullOrEmpty(x)).Select(int.Parse).ToList();

            return _context.Categories
                .Where(x => ids.Contains(x.CatId))
                .Select(x => x.Title)
                .FirstOrDefault() ?? "";
        }

        private List<Category> GetIndustries()
        {
            return _context.Categories
                .Where(x => x.ParentId == 1)
                .OrderBy(x => x.Sort)
                .ToList();
        }

        private List<NhaCungUng> GetSuppliers()
        {
            return _context.NhaCungUngs
                .OrderBy(x => x.FullName)
                .ToList();
        }

        private void MapSupplier(ProductDetailViewModel vm, SanPhamCNTB p)
        {
            if (p.NCUId == null) return;

            var supplier = _context.NhaCungUngs.FirstOrDefault(x => x.CungUngId == p.NCUId);
            if (supplier == null) return;

            vm.SupplierName = supplier.FullName;
            vm.SupplierUrl = $"{MainDomain}8-dich-vu-cung-ung/{MakeURLFriendly(supplier.FullName)}-{supplier.CungUngId}.html";
        }

        private List<VSImage> GetImages(int contentId)
        {
            return _context.VSImages
                .Where(x => x.ContentId == contentId && x.StatusId == 1)
                .OrderBy(x => x.Id)
                .ToList();
        }

        private List<Category> GetRelatedCategories(int parentId)
        {
            return _context.Categories
                .Where(x => x.ParentId == parentId && x.MainCate == true)
                .OrderBy(x => x.Sort)
                .ToList();
        }

        private List<KeywordVm> GetKeywords(int productId)
        {
            return (
                from lk in _context.KeywordLienKets
                join k in _context.KeywordEntities on lk.KeywordId equals k.KeywordID
                where lk.TargetId == productId && lk.TypeId == 1
                select new KeywordVm
                {
                    KeywordId = (int)k.KeywordID,
                    Keyword = k.Keyword
                }).Distinct().ToList();
        }

        private void MapVideo(ProductDetailViewModel vm, SanPhamCNTB p)
        {
            if (p.IsYoutube == true && !string.IsNullOrEmpty(p.URL))
            {
                vm.IsYoutube = true;
                vm.YoutubeEmbedUrl = $"https://www.youtube.com/embed/{p.URL}";
            }
            else
            {
                vm.VideoFileUrl = p.URL;
            }
        }

        private int GetRatingCount(int productId, int typeId)
        {
            return _context.Ratings.Count(x => x.SanPhamId == productId && x.TypeID == typeId);
        }

        private void UpdateViewCount(SanPhamCNTB p)
        {
            p.Viewed = (p.Viewed ?? 0) + 1;
            _context.SaveChanges();
        }

        [Route("2-ds-cong-nghe-thiet-bi/{slug}-{cateId:int}.html")]
        public IActionResult ProductByCate(
            int cateId,
            int page = 1,
            int pageSize = 12)
        {
            var model = new ProductByCateViewModel
            {
                CateId = cateId,
                CurPage = page,
                PageSize = pageSize
            };

            LoadProducts(model);
            LoadCategories(model);

            return View(model);
        }

        // =====================================================
        // LOAD PRODUCT GRID (BindToGrid)
        // =====================================================
        private void LoadProducts(ProductByCateViewModel vm)
        {
            string cateToken = ";" + vm.CateId + ";";

            var query = _context.SanPhamCNTBs
                .Where(x => x.StatusId == 3 && x.CategoryId.Contains(cateToken))
                .OrderByDescending(x => x.Created);

            vm.Total = query.Count();

            var list = query
                .Skip((vm.CurPage - 1) * vm.PageSize)
                .Take(vm.PageSize)
                .ToList();

            foreach (var row in list)
            {
                var item = new ProductItemVm
                {
                    ProductId = row.ID,
                    Title = row.Name,
                    Code = row.Code,
                    Star = row.Rating ?? 0,
                    IsSC = row.TypeId == 2,
                    IsNC = row.TypeId == 3,
                    PriceText = row.OriginalPrice == null
                        ? ""
                        : FormatCurrencyOto((decimal?)row.OriginalPrice, row.Currency)
                };

                // ===== IMAGE (GIỮ NGUYÊN LOGIC VB)
                if (string.IsNullOrEmpty(row.QuyTrinhHinhAnh))
                {
                    if (row.TypeId == 2)
                        item.ImageUrl = MainDomain + "images/sangche.png";
                    else
                        item.ImageUrl = MainDomain + "images/research.jpg";
                }
                else
                {
                    item.ImageUrl = CookedImageURL("254-170", row.QuyTrinhHinhAnh);
                }

                // ===== URL
                item.Url = MainDomain +
                           "2-cong-nghe-thiet-bi/" +
                           row.TypeId + "/" +
                           MakeURLFriendly(row.Name) +
                           "-" + row.ID + ".html";

                vm.Products.Add(item);
            }

            // ===== CATEGORY META
            var cate = _context.Categories.FirstOrDefault(x => x.CatId == vm.CateId);
            if (cate != null)
            {
                vm.CateTitle = cate.Title;
                vm.PageTitle = cate.Title;

                ViewData["MetaDescription"] = cate.Description;
                ViewData["MetaKeywords"] = cate.LogoURL;
            }

            BuildPager(vm);
        }

        // =====================================================
        // LOAD SUB CATEGORY
        // =====================================================
        private void LoadCategories(ProductByCateViewModel vm)
        {
            var list = _context.Categories
                .Where(x => x.ParentId == vm.CateId && x.MainCate == true) 
                .OrderBy(x => x.Sort)
                .ToList();

            foreach (var row in list)
            {
                vm.Categories.Add(new CategoryItemVm
                {
                    Title = row.Title,
                    Url = MainDomain +
                          "2-ds-cong-nghe-thiet-bi/" +
                          MakeURLFriendly(row.QueryString) +
                          "-" + row.CatId + ".html"
                });
            }
        }

        // =====================================================
        // PAGER (Create_Pager)
        // =====================================================
        private void BuildPager(ProductByCateViewModel vm)
        {
            int totalPage = (vm.Total % vm.PageSize == 0)
                ? vm.Total / vm.PageSize
                : vm.Total / vm.PageSize + 1;

            int page2Show = 10;

            IEnumerable<int> left =
                vm.CurPage <= page2Show
                    ? Enumerable.Range(1, vm.CurPage)
                    : Enumerable.Range(vm.CurPage - page2Show, page2Show);

            IEnumerable<int> right =
                vm.CurPage + page2Show <= totalPage
                    ? Enumerable.Range(vm.CurPage, page2Show + 1)
                    : Enumerable.Range(vm.CurPage, totalPage - vm.CurPage + 1);

            foreach (var p in left.Union(right))
            {
                vm.Pages.Add(new PageItemVm
                {
                    Page = p,
                    IsActive = p == vm.CurPage
                });
            }
        }

        // =====================================================
        // ADD TO CART
        // =====================================================
        [HttpGet]
        [Route("cart/add/{id:int}")]
        public IActionResult AddToCart(int id)
        {
            AddShoppingCart(id);
            HttpContext.Session.SetString("LastURL", Request.Path);
            return Redirect(MainDomain + "gio-hang.html");
        }

        private void AddShoppingCart(int productId)
        {
            var cartId = HttpContext.Session.GetString("CartId");
            var userId = HttpContext.Session.GetInt32("UserId");

            var check = _context.ShoppingCarts.FirstOrDefault(x =>
                x.CartId == cartId &&
                x.ProductId == productId &&
                x.TypeId == 1);

            if (check == null)
            {
                _context.ShoppingCarts.Add(new ShoppingCart
                {
                    CartId = cartId,
                    ProductId = productId,
                    UserId = userId,
                    Quantity = 1,
                    TypeId = 1,
                    DateCreated = DateTime.Now,
                    Domain = MainDomain
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
        public static string FormatCurrencyOto(decimal? price, string? currency)
        {
            if (price == null) return "";
            return string.Format("{0:N0} {1}", price, currency);
        }

        [HttpGet]
        public IActionResult RelatedProducts(int productId)
        {
            const int languageId = 1;

            var pp = _context.SanPhamCNTBs
                .FirstOrDefault(x =>
                    x.ID == productId &&
                    x.LanguageId == languageId &&
                    x.StatusId == 3);

            if (pp == null || string.IsNullOrWhiteSpace(pp.CategoryId))
                return PartialView("_ProductRelated", new List<ProductRelatedItemVm>());

            var cateIds = pp.CategoryId
                .Split(';', StringSplitOptions.RemoveEmptyEntries)
                .ToList();


            var candidates = _context.SanPhamCNTBs
                .Where(x =>
                    x.LanguageId == languageId &&
                    x.StatusId == 3 &&
                    x.ID != pp.ID &&
                    x.CategoryId != null)
                .AsEnumerable(); // ⬅️ BẮT BUỘC

            var related = candidates
                .Where(x =>
                    cateIds.Any(c => x.CategoryId.Contains(";" + c + ";")))
                .OrderByDescending(x => x.Created)
                .Take(6)
                .Select(row => new ProductRelatedItemVm
                {
                    Id = row.ID,
                    Title = row.Name,
                    Star = row.Rating ?? 0,

                    ImageUrl = string.IsNullOrEmpty(row.QuyTrinhHinhAnh)
                        ? (row.TypeId == 2
                            ? MainDomain + "images/sangche.png"
                            : MainDomain + "images/research.jpg")
                        : CookedImageURL("254-170", row.QuyTrinhHinhAnh),

                    PriceText = row.OriginalPrice == null
                        ? ""
                        : FormatCurrencyOto((decimal?)row.OriginalPrice, row.Currency),

                    Url = MainDomain +
                          "2-cong-nghe-thiet-bi/" +
                          row.TypeId + "/" +
                          MakeURLFriendly(row.Name) +
                          "-" + row.ID + ".html"
                })
                .ToList();

            return PartialView("_ProductRelated", related);
        }

    }
}
