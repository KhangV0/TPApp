using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IProductService _productService;
        private readonly string _mainDomain;
        
        // ===== CONSTANT =====
        private static readonly int[] TinSuKienMenus = { 44, 72, 83, 46 };
        private const int VIDEO_MENU_ID = 70;
        private const int YEU_CAU_MENU_ID = 67;

        public HomeController(AppDbContext context, IProductService productService, IOptions<AppSettings> appSettings)
        {
            _context = context;
            _productService = productService;
            _mainDomain = appSettings.Value.MainDomain;
        }

        // ================= INDEX =================
        public async Task<IActionResult> Index()
        {
            var model = new HomeViewModel
            {
                CongNgheMoiCapNhatHtml = "", 
                ProductCNMoiCapNhatHtml = "",
                TinSuKien = LoadTinSuKien(), 
                VideoCongNghe = LoadVideoCongNghe(),
                YeuCauCongNghe = LoadYeuCauCongNghe()
            };

            ViewBag.NewTech = await _productService.GetNewProductsAsync(10);
            ViewBag.NewProducts = await _productService.GetNewProductsAsync(12);

            return View(model);
        }

        // ================= TIN SU KIEN =================
        private List<TinSuKienTabVm> LoadTinSuKien()
        {
            var menus = _context.Menus
                .Where(x => TinSuKienMenus.Contains(x.MenuId))
                .OrderBy(x => x.Sort)
                .ToList();

            var result = new List<TinSuKienTabVm>();

            foreach (var m in menus)
            {
                var childIds = uspSelectSubMenu(m.MenuId);

                var items = _context.Contents
                    .Where(x => (x.MenuId == m.MenuId || childIds.Contains(x.MenuId ?? 0))
                             && x.StatusId == 3)
                    .OrderByDescending(x => x.PublishedDate)
                    .Take(3)
                    .Select(x => new TinSuKienItemVm
                    {
                        Title = x.Title,
                        Description = x.Description,
                        ImageUrl = ProductController.CookedImageURL("460-275", x.Image, _mainDomain),
                        Link = $"{_mainDomain}{x.MenuId}/{x.QueryString}-{x.Id}.html"
                    })
                    .ToList();

                result.Add(new TinSuKienTabVm
                {
                    MenuId = m.MenuId,
                    Title = m.Title,
                    Items = items
                });
            }

            return result;
        }

        // ================= VIDEO =================
        private List<VideoVm> LoadVideoCongNghe()
        {
             var childIds = uspSelectSubMenu(VIDEO_MENU_ID);

            return _context.Contents
                .Where(x => (x.MenuId == VIDEO_MENU_ID || childIds.Contains(x.MenuId ?? 0))
                         && x.StatusId == 3)
                .OrderByDescending(x => x.PublishedDate)
                .Take(3)
                .Select(x => new VideoVm
                {
                    Title = x.Title,
                    VideoUrl = ExtractYouTubeUrl(x.Description),
                    ImageUrl = ProductController.CookedImageURL("460-275", x.Image, _mainDomain),
                    Link = $"{_mainDomain}{x.MenuId}/{x.QueryString}-{x.Id}.html"
                })
                .ToList();
        }

        // ================= YEU CAU =================
        private YeuCauCongNgheVm LoadYeuCauCongNghe()
        {
            var childIds = uspSelectSubMenu(YEU_CAU_MENU_ID);

            var list = _context.ContentsYeuCaus
                .Where(x => (x.MenuId == YEU_CAU_MENU_ID || childIds.Contains(x.MenuId ?? 0))
                         && x.StatusId == 3)
                .OrderByDescending(x => x.PublishedDate)
                .Take(7)
                .ToList();

            return new YeuCauCongNgheVm
            {
                Title = "Yêu cầu công nghệ",
                // Passing _mainDomain to MapYeuCau via closure or change method
                // LINQ Select with method group `MapYeuCau` won't work if MapYeuCau needs instance state _mainDomain.
                // Changing to lambda.
                Col1 = list.Take(2).Select(x => MapYeuCau(x)).ToList(),
                Col2 = list.Skip(2).Take(3).Select(x => MapYeuCau(x)).ToList(),
                Col3 = list.Skip(5).Take(2).Select(x => MapYeuCau(x)).ToList()
            };
        }
        
        // Static Helpers preserved/Adapted
        // ... (CookedImageURL removed as we use ProductController's or Common) ...
        // Keeping unique ones.

        public static string ExtractYouTubeUrl(string? desc)
        {
            if (string.IsNullOrEmpty(desc)) return "";
            var idx = desc.IndexOf("https://www.youtube.com", StringComparison.OrdinalIgnoreCase);
            if (idx < 0) return "";
            var end = desc.IndexOf("\"", idx);
            return end < 0 ? "" : desc.Substring(idx, end - idx);
        }

        // ================= DATA HELPERS =================
        private List<int> uspSelectSubMenu(int parentId)
        {
            return _context.Menus
                .Where(x => x.ParentId == parentId)
                .Select(x => x.MenuId)
                .ToList();
        }

        private YeuCauItemVm MapYeuCau(ContentsYeuCau x)
        {
            return new YeuCauItemVm
            {
                Title = x.Title,
                Viewed = x.Viewed,
                ImageUrl = ProductController.CookedImageURL("254-170", x.Image, _mainDomain),
                Link = $"{_mainDomain}{x.MenuId}/yeu-cau/{x.QueryString}-{x.Id}.html"
            };
        }
    }
}
