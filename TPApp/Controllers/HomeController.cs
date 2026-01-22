using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.RegularExpressions;
using TPApp.Data;
using TPApp.Entities;
using TPApp.ViewModel;

namespace TPApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        // ===== CONSTANT =====
        private static readonly int[] TinSuKienMenus = { 44, 72, 83, 46 };
        private const int VIDEO_MENU_ID = 70;
        private const int YEU_CAU_MENU_ID = 67;
        private const string MainDomain = "https://localhost:7232/";

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        // ================= INDEX =================
        public IActionResult Index()
        {
            var model = new HomeViewModel
            {
                CongNgheMoiCapNhatHtml = LoadCongNgheMoiCapNhat(10, 5),
                ProductCNMoiCapNhatHtml = LoadCongNgheMoiCapNhat(12, 4),
                TinSuKien = LoadTinSuKien(),
                VideoCongNghe = LoadVideoCongNghe(),
                YeuCauCongNghe = LoadYeuCauCongNghe()
            };

            return View(model);
        }

        // ================= CONG NGHE MOI =================
        private string LoadCongNgheMoiCapNhat(int take, int perSlide)
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
                                 MakeURLFriendly(item.Name)+ '-' + item.ID + ".html";

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
                        ImageUrl = CookedImageURL("460-275", x.Image),
                        Link = $"{MainDomain}{x.MenuId}/{x.QueryString}-{x.Id}.html"
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
                    ImageUrl = CookedImageURL("460-275", x.Image),
                    Link = $"{MainDomain}{x.MenuId}/{x.QueryString}-{x.Id}.html"
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
                .Take(6)
                .ToList();

            return new YeuCauCongNgheVm
            {
                Title = "Yêu cầu công nghệ",
                Col1 = list.Take(3).Select(MapYeuCau).ToList(),
                Col2 = list.Skip(3).Take(3).Select(MapYeuCau).ToList()
            };
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

        private static YeuCauItemVm MapYeuCau(ContentsYeuCau x)
        {
            return new YeuCauItemVm
            {
                Title = x.Title,
                ImageUrl = CookedImageURL("254-170", x.Image),
                Link = $"{MainDomain}{x.MenuId}/yeu-cau/{x.QueryString}-{x.Id}.html"
            };
        }
    }
}
