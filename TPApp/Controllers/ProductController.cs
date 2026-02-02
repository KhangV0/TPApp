using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.RegularExpressions;
using TPApp.Data;
using TPApp.Entities;
using TPApp.Interfaces;
using TPApp.Helpers;
using TPApp.ViewModel;

namespace TPApp.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly AppDbContext _context;
        private readonly string _mainDomain;

        public ProductController(IProductService productService, AppDbContext context, IOptions<AppSettings> appSettings)
        {
            _productService = productService;
            _context = context;
            _mainDomain = appSettings.Value.MainDomain;
        }

        // ================== INDEX (PORT TỪ WEBFORMS) ==================

        public async Task<IActionResult> Index(int catId = 1)
        {
            var lang = HttpContext.Session.GetInt32("LanguageId") ?? 1;

            var categories = _context.Categories
                .Where(x => x.ParentId == catId && x.MainCate == true)
                .OrderBy(x => x.Sort)
                .ToList();

            var model = new ProductIndexViewModel();
            model.NewProducts = await _productService.GetNewProductsAsync(12);

            foreach (var cate in categories)
            {
                var products = await _productService.GetProductsByCategoryAsync(cate.CatId, lang, 4);

                model.Categories.Add(new CategoryBlockVm
                {
                    Category = cate,
                    Products = products
                });
            }

            return View(model);
        }

        // ================== ADD CART (PORT TỪ btnAddCart_Click) ==================
        [HttpPost]
        public IActionResult AddCart(int productId)
        {
            var cartId = HttpContext.Session.GetString("CartId");
            var userId = HttpContext.Session.GetInt32("UserId");

            if (string.IsNullOrEmpty(cartId))
                return Redirect($"{_mainDomain}gio-hang.html");

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
            return Redirect($"{_mainDomain}gio-hang.html");
        }

        // ================== DETAIL (GIỮ NGUYÊN) ==================


        public async Task<IActionResult> Detail(int id)
        {
            // Legacy redirects, could also use _mainDomain if they were internal, but seem specific. Keeping hardcoded external redirects as is for safety unless user wants all converted.
            if (id == 1311)
                return RedirectPermanent("http://techport.vn/2-cong-nghe-thiet-bi/1/thiet-bi-dong-goi-hut-chan-khong-1311.html");

            if (id == 8512)
                return RedirectPermanent("http://techport.vn/2-cong-nghe-thiet-bi/1/thiet-bi-dong-goi-bot-tu-dong-goi-lon--8512.html");

            var product = await _productService.GetProductByIdAsync(id);

            if (product == null)
                return Redirect($"{_mainDomain}cong-nghe-thiet-bi-2.html");

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

        //Helpers to be refactored into CommonService later
        private string GetCategoryTitle(string categoryIds)
        {
            if (string.IsNullOrEmpty(categoryIds)) return "";
            var ids = categoryIds.Split(";").Where(x => !string.IsNullOrEmpty(x)).Select(int.Parse).ToList();
            return _context.Categories.Where(x => ids.Contains(x.CatId)).Select(x => x.Title).FirstOrDefault() ?? "";
        }
        private List<Category> GetIndustries() => _context.Categories.Where(x => x.ParentId == 1).OrderBy(x => x.Sort).ToList();
        private List<NhaCungUng> GetSuppliers() => _context.NhaCungUngs.OrderBy(x => x.FullName).ToList();
        
        private void MapSupplier(ProductDetailViewModel vm, SanPhamCNTB p) {
             if (p.NCUId == null) return;
            var supplier = _context.NhaCungUngs.FirstOrDefault(x => x.CungUngId == p.NCUId);
            if (supplier == null) return;
            vm.SupplierName = supplier.FullName;
            vm.SupplierUrl = $"{_mainDomain}8-dich-vu-cung-ung/{MakeURLFriendly(supplier.FullName)}-{supplier.CungUngId}.html";
        }
        
        private List<VSImage> GetImages(int contentId) => _context.VSImages.Where(x => x.ContentId == contentId && x.StatusId == 1).OrderBy(x => x.Id).ToList();
        private List<Category> GetRelatedCategories(int parentId) => _context.Categories.Where(x => x.ParentId == parentId && x.MainCate == true).OrderBy(x => x.Sort).ToList();
        
        private List<KeywordVm> GetKeywords(int productId) {
             return (from lk in _context.KeywordLienKets join k in _context.KeywordEntities on lk.KeywordId equals k.KeywordID where lk.TargetId == productId && lk.TypeId == 1 select new KeywordVm { KeywordId = (int)k.KeywordID, Keyword = k.Keyword }).Distinct().ToList();
        }
        
        private void MapVideo(ProductDetailViewModel vm, SanPhamCNTB p) {
            if (p.IsYoutube == true && !string.IsNullOrEmpty(p.URL)) { vm.IsYoutube = true; vm.YoutubeEmbedUrl = $"https://www.youtube.com/embed/{p.URL}"; } else { vm.VideoFileUrl = p.URL; }
        }
        private int GetRatingCount(int productId, int typeId) => _context.Ratings.Count(x => x.SanPhamId == productId && x.TypeID == typeId);
        private void UpdateViewCount(SanPhamCNTB p) { p.Viewed = (p.Viewed ?? 0) + 1; _context.SaveChanges(); }


        public async Task<IActionResult> ProductByCate(int cateId, int page = 1, int pageSize = 12)
        {
            var model = new ProductByCateViewModel
            {
                CateId = cateId,
                CurPage = page,
                PageSize = pageSize
            };

            await LoadProductsAsync(model); // Converted to Async
            LoadCategories(model);

            return View(model);
        }

        private async Task LoadProductsAsync(ProductByCateViewModel vm)
        {
            vm.Total = await _productService.GetProductCountByCategoryAsync(vm.CateId);
            var list = await _productService.GetPagedProductsByCategoryAsync(vm.CateId, vm.CurPage, vm.PageSize);

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
                    PriceText = row.OriginalPrice == null ? "" : FormatCurrencyOto((decimal?)row.OriginalPrice, row.Currency),
                    // Using Instance Method or passing domain
                    ImageUrl = string.IsNullOrEmpty(row.QuyTrinhHinhAnh) ? (row.TypeId == 2 ? _mainDomain + "images/sangche.png" : _mainDomain + "images/research.jpg") : CookedImageURL("254-170", row.QuyTrinhHinhAnh),
                    Url = _mainDomain + "2-cong-nghe-thiet-bi/" + row.TypeId + "/" + MakeURLFriendly(row.Name) + "-" + row.ID + ".html"
                };
                vm.Products.Add(item);
            }

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

        private void LoadCategories(ProductByCateViewModel vm)
        {
             var list = _context.Categories.Where(x => x.ParentId == vm.CateId && x.MainCate == true).OrderBy(x => x.Sort).ToList();
            foreach (var row in list) { vm.Categories.Add(new CategoryItemVm { Title = row.Title, Url = _mainDomain + "2-ds-cong-nghe-thiet-bi/" + MakeURLFriendly(row.QueryString) + "-" + row.CatId + ".html" }); }
        }
        private void BuildPager(ProductByCateViewModel vm)
        {
             int totalPage = (vm.Total % vm.PageSize == 0) ? vm.Total / vm.PageSize : vm.Total / vm.PageSize + 1;
            int page2Show = 10;
            IEnumerable<int> left = vm.CurPage <= page2Show ? Enumerable.Range(1, vm.CurPage) : Enumerable.Range(vm.CurPage - page2Show, page2Show);
            IEnumerable<int> right = vm.CurPage + page2Show <= totalPage ? Enumerable.Range(vm.CurPage, page2Show + 1) : Enumerable.Range(vm.CurPage, totalPage - vm.CurPage + 1);
            foreach (var p in left.Union(right)) { vm.Pages.Add(new PageItemVm { Page = p, IsActive = p == vm.CurPage }); }
        }

        [HttpGet]

        public IActionResult AddToCart(int id)
        {
            AddShoppingCart(id);
            HttpContext.Session.SetString("LastURL", Request.Path);
            return Redirect(_mainDomain + "gio-hang.html");
        }

        private void AddShoppingCart(int productId)
        {
             var cartId = HttpContext.Session.GetString("CartId");
            var userId = HttpContext.Session.GetInt32("UserId");
            var check = _context.ShoppingCarts.FirstOrDefault(x => x.CartId == cartId && x.ProductId == productId && x.TypeId == 1);
            if (check == null) {
                _context.ShoppingCarts.Add(new ShoppingCart { CartId = cartId, ProductId = productId, UserId = userId, Quantity = 1, TypeId = 1, DateCreated = DateTime.Now, Domain = _mainDomain });
                _context.SaveChanges();
            }
        }

        [HttpGet]
        public async Task<IActionResult> RelatedProducts(int productId)
        {
            const int languageId = 1;

            var related = await _productService.GetRelatedProductsAsync(productId, languageId, 6);
            
            var relatedVms = related.Select(row => new ProductRelatedItemVm
                {
                    Id = row.ID,
                    Title = row.Name,
                    Star = row.Rating ?? 0,
                    ImageUrl = string.IsNullOrEmpty(row.QuyTrinhHinhAnh) ? (row.TypeId == 2 ? _mainDomain + "images/sangche.png" : _mainDomain + "images/research.jpg") : CookedImageURL("254-170", row.QuyTrinhHinhAnh),
                    PriceText = row.OriginalPrice == null ? "" : FormatCurrencyOto((decimal?)row.OriginalPrice, row.Currency),
                    Url = _mainDomain + "2-cong-nghe-thiet-bi/" + row.TypeId + "/" + MakeURLFriendly(row.Name) + "-" + row.ID + ".html"
                })
                .ToList();

            return PartialView("_ProductRelated", relatedVms);
        }

        // =====================================
        // Changed to INSTANCE method to use _mainDomain
        // NOTE: This breaks static usage in Views if Views call `ProductController.CookedImageURL`.
        // I will keep it STATIC but add a parameter default or overload? 
        // No, static cannot access _mainDomain.
        // I will change it to INSTANCE. 
        // AND I will verify if Views are calling it. YES, my previous edit to `_CongNgheMoiCapNhat.cshtml` called it statically: `TPApp.Controllers.ProductController.CookedImageURL`.
        // This is a breaking change for the View.
        // FIX: I will keep a static version that takes domain as parameter, and an instance version that uses injected domain.
        // OR better: Create a dedicated static helper class for purely functional transformation if possible, or View Service.
        // For now, to satisfy "Make it common" and "Load from AppSettings", I'll provide a static version that *requires* the domain, so callers must provide it (e.g. from their own injection).
        // 
        // actually, `_CongNgheMoiCapNhat.cshtml` can inject `IOptions<AppSettings>` too!
        // That is the "Pro" way.
        // So I will change the static method to accept `mainDomain` string.
        // And update the View to inject options and pass it.
        //
        // Strategy:
        // 1. helper `CookedImageURL(string size, string? imageUrl, string mainDomain)` (static)
        // 2. Controller instance method `CookedImageURL(string size, string? imageUrl)` (calls static with _mainDomain) (convenience)
        // 3. View updates.
        //
        // =====================================
        
        public string CookedImageURL(string size, string? imageUrl) => CookedImageURL(size, imageUrl, _mainDomain);

        public static string CookedImageURL(string size, string? imageUrl, string mainDomain) {
             if (string.IsNullOrWhiteSpace(imageUrl)) return $"{mainDomain.TrimEnd('/')}/images/{size}_noImage.jpg";
            if (!imageUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase)) imageUrl = $"{mainDomain.TrimEnd('/')}/{imageUrl.TrimStart('/')}";
            var fileName = Path.GetFileName(imageUrl);
            if (fileName.StartsWith(size + "-", StringComparison.OrdinalIgnoreCase)) return imageUrl;
            return imageUrl.Replace(fileName, $"{size}-{fileName}");
        }
        
        public static string MakeURLFriendly(string? input) {
             if (string.IsNullOrWhiteSpace(input)) return string.Empty;
            var str = input.ToLower().Trim();
            var old = str;
            const string findText = "ä|à|á|ạ|ả|ã|â|ầ|ấ|ậ|ẩ|ẫ|ă|ằ|ắ|ặ|ẳ|ẵ|ç|è|é|ẹ|ẻ|ẽ|ê|ề|ế|ệ|ể|ễ|ì|í|î|ị|ỉ|ĩ|ö|ò|ó|ọ|ỏ|õ|ô|ồ|ố|ộ|ổ|ỗ|ơ|ờ|ớ|ợ|ở|ỡ|ü|ù|ú|ụ|ủ|ũ|ư|ừ|ứ|ự|ử|ữ|ỳ|ý|ỵ|ỷ|ỹ|đ";
            const string replaceText = "a|a|a|a|a|a|a|a|a|a|a|a|a|a|a|a|a|a|c|e|e|e|e|e|e|e|e|e|e|e|i|i|i|i|i|i|o|o|o|o|o|o|o|o|o|o|o|o|o|o|o|o|o|o|u|u|u|u|u|u|u|u|u|u|u|u|y|y|y|y|y|d";
            var findArr = findText.Split('|'); var replaceArr = replaceText.Split('|');
            for (int i = 0; i < findArr.Length; i++) str = str.Replace(findArr[i], replaceArr[i]);
            str = Regex.Replace(str, @"[^a-z0-9]", "-");
            str = Regex.Replace(str, @"-+", "-").Trim('-');
            if (str.Length < 3) str = old.Replace(" ", "-").Replace(".", "-").Replace("?", "-");
            return str;
        }
        public static string FormatCurrencyOto(decimal? price, string? currency)  { if (price == null) return ""; return string.Format("{0:N0} {1}", price, currency); }
    }
}
