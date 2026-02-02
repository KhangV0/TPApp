using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TPApp.Data;
using TPApp.Entities;
using TPApp.Helpers;
using TPApp.ViewModel;

namespace TPApp.Controllers
{
    public class ForumController : Controller
    {
        private readonly AppDbContext _context;
        private readonly string _mainDomain;
        private readonly AppSettings _appSettings;

        private const int STATUS_APPROVED = 3;

        public ForumController(AppDbContext context, IOptions<AppSettings> appSettings)
        {
            _context = context;
            _mainDomain = appSettings.Value.MainDomain;
            _appSettings = appSettings.Value;
        }

        // ================= INDEX =================
        public IActionResult Index(int? linhvuc, int? parentid, int page = 1, int pageSize = 10)
        {
            int lang = HttpContext.Session.GetInt32("LanguageId") ?? 1;

            var model = new ForumIndexVm
            {
                Title = "Thảo luận công nghệ",
                LinhVuc = linhvuc,
                ParentId = parentid,
                PageIndex = page,
                PageSize = pageSize,
                Categories = LoadCategories(1)
            };

            if (!linhvuc.HasValue && !parentid.HasValue)
            {
                LoadAllCongNghe(model, lang);
            }
            // ================== TƯ VẤN ==================
            else if (parentid == 2)
            {
                model.Title = "Tư vấn & Hỗ trợ";
                LoadTuVan(model, lang);
            }
            else
            {
                LoadCongNgheByLinhVuc(model, lang);
            }

            model.Pager = BuildPager(model.TotalRecord, page, pageSize, 10);

            model.TotalText = model.TotalRecord > 0
                ? $"Tổng số {model.TotalRecord} thảo luận"
                : "";
            model.PortletNhieunhat = LoadPortletNhieuNhat();
            model.PortletDangMo = LoadPortletDangMo();
            model.PortletTinTuc = LoadPortletTinTuc();
            model.PortletGiaiPhapCongNghe = LoadPortletGiaiPhapCongNghe();

            return View(model);
        }

        
        [HttpGet]
        public IActionResult Detail(int id)
        {
            var vm = new ForumDetailVm();
            int lang = HttpContext.Session.GetInt32("LanguageId") ?? 1;

            // 1. GET DATA
            var p = _context.ForumYCTBs
                .FirstOrDefault(x => x.ForumYCTBId == id);

            if (p == null)
            {
                return Redirect($"{_mainDomain}Errors/404.aspx");
            }

            // 2. MAP TO VM
            vm.Id = p.ForumYCTBId;
            vm.Title = p.Title ?? "";
            vm.Content = p.NoiDung ?? "";
            vm.FullName = p.FullName ?? "";
            vm.TenDonVi = p.TenDonVi ?? "";
            vm.DiaChi = p.DiaChi ?? "";
            
            // Image Logic
            if (string.IsNullOrEmpty(p.HinhDaiDien))
            {
                 vm.HinhDaiDien = $"{_mainDomain}images/TimSanPham.jpg";
            }
            else
            {
                vm.HinhDaiDien = p.HinhDaiDien;
            }

            vm.CreatedDateText = DateToString(p.Created, "mm/dd/yyyy");

            // 3. CHECK LOGIN
            var userId = HttpContext.Session.GetInt32("UserId");
            var username = HttpContext.Session.GetString("Username");

            if (string.IsNullOrEmpty(username))
            {
                vm.IsLoggedIn = false;
                vm.Phone = "Đăng nhập để xem thông tin";
                vm.Email = "Đăng nhập để xem thông tin";
            }
            else
            {
                vm.IsLoggedIn = true;
                vm.Phone = p.Phone ?? "";
                vm.Email = p.Email ?? "";
            }

            // 4. UPDATE VIEW COUNT
            // Logic: Session Key "PageViewPhieuYCTB" + ID
            string sessionKey = $"PageViewPhieuYCTB{id}";
            string? lastView = HttpContext.Session.GetString(sessionKey);

            int updateTime = _appSettings.SettingTimeUpdatePageView; // Retrieved from AppSettings

            bool shouldUpdate = false;
            if (lastView == null)
            {
                shouldUpdate = true;
            }
            else
            {
                 if ((DateTime.Now - DateTime.Parse(lastView)).TotalSeconds >= updateTime)
                 {
                     shouldUpdate = true;
                 }
            }

            if (shouldUpdate)
            {
                p.Viewed = (p.Viewed ?? 0) + 1;
                _context.SaveChanges();
                HttpContext.Session.SetString(sessionKey, DateTime.Now.ToString());
            }

            vm.Viewed = p.Viewed ?? 0;
            vm.Like = p.Like ?? 0;

            // 5. RELATED ITEMS (LoadPhieuYeuCau)
            // Logic: Take 8 newest, Lang=1 (Source code hardcoded 1)
            vm.RelatedItems = _context.ForumYCTBs
                .AsNoTracking()
                .Where(q => q.LanguageId == 1 && q.StatusId == STATUS_APPROVED && q.ForumYCTBId != id) // Added ID check and Status check for safety
                .OrderByDescending(q => q.Created)
                .Take(8)
                .AsEnumerable()
                .Select(x => new ForumItemVm
                {
                    Id = x.ForumYCTBId,
                    Title = x.Title,
                    Url = $"{_mainDomain}chi-tiet-thao-luan-{x.ForumYCTBId}.html",
                    AuthorInfo = x.FullName, // used as name
                    HinhDaiDien = string.IsNullOrEmpty(x.HinhDaiDien) ? $"{_mainDomain}images/TimSanPham.jpg" : x.HinhDaiDien,
                    Phone = x.Phone,
                    Email = x.Email
                })
                .ToList();
                
            // NOTE: ForumItemVm doesn't have Image/Phone fields. 
            // I should use ForumDetailVm or expand ForumItemVm? 
            // The existing ForumItemVm is minimal.
            // I will use ForumItemVm but I might need to fetch the image in the View or add it to VM.
            // Let's Add HinhDaiDien to ForumItemVm or use a dedicated inner class for RelatedItems.
            // Currently ForumItemVm has Title, Url.
            // I will check ForumIndexVm again. It has AuthorInfo. 
            // I will Use ForumItemVm but I need HinhDaiDien.
            // I will ADD HinhDaiDien, Phone, Email to ForumItemVm below.

            // 6. CATEGORIES (Side Menu)
            // Logic: ParentId = 1 (Source code passed 1 to LoadData)
            vm.Categories = _context.Categories
                .Where(x => x.ParentId == 1 && x.MainCate == true)
                .OrderBy(x => x.Sort)
                .Select(x => new CategoryVm
                {
                    Id = x.CatId,
                    Title = x.Title,
                    Url = $"{_mainDomain}thao-luan-{x.CatId}-1.html"
                })
                .ToList();

            // 7. PORTLETS
            vm.PortletNhieuNhat = LoadPortletNhieuNhat();

            // Set Meta
            ViewData["Title"] = vm.Title;
            ViewData["MetaDescription"] = vm.Content; // Source code set MetaKeywords to NoiDung

            return View(vm);
        }

        [HttpPost]
        public IActionResult Like(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId"); // or String "0"?
            string ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";
            
            // Logic: Check LikePage
            // If User logged in: Check (IP + UserId + IdPage + TypeID=1)
            // If Not logged in: Check (IP + IdPage + TypeID=1)
            
            bool alreadyLiked = false;
            if (userId.HasValue)
            {
                 alreadyLiked = _context.Likepages.Any(q => q.IPAddress == ipAddress && q.UserId == userId.ToString() && q.IdPage == id && q.TypeID == 1);
            }
            else
            {
                 alreadyLiked = _context.Likepages.Any(q => q.IPAddress == ipAddress && q.IdPage == id && q.TypeID == 1);
            }

            if (!alreadyLiked)
            {
                var likePage = new Likepage
                {
                    IdPage = id,
                    IPAddress = ipAddress,
                    UserId = userId?.ToString(),
                    TypeID = 1,
                    Like = 1,
                    Created = DateTime.Now
                };
                _context.Likepages.Add(likePage);

                var p = _context.ForumYCTBs.FirstOrDefault(x => x.ForumYCTBId == id);
                if (p != null)
                {
                    p.Like = (p.Like ?? 0) + 1;
                }
                _context.SaveChanges();
            }

            return Redirect($"{_mainDomain}chi-tiet-thao-luan-{id}.html");
        }

        // ================= LOAD ALL =================
        private void LoadAllCongNghe(ForumIndexVm model, int lang)
        {
            var query = _context.ForumYCTBs
                .AsNoTracking()
                .Where(x =>
                    x.LanguageId == lang &&
                    x.StatusId == STATUS_APPROVED
                );

            model.TotalRecord = query.Count();

            model.CNTBItems = query
                .OrderByDescending(x => x.Created)
                .Skip((model.PageIndex - 1) * model.PageSize)
                .Take(model.PageSize)
                .ToList()
                .Select(MapCNTB)
                .ToList();
        }

        // ================= LOAD BY LINH VUC =================
        private void LoadCongNgheByLinhVuc(ForumIndexVm model, int lang)
        {
            int lv = model.LinhVuc ?? 0;

            var query = _context.ForumYCTBs
                .AsNoTracking()
                .Where(x =>
                    x.LanguageId == lang &&
                    x.StatusId == STATUS_APPROVED &&
                    !string.IsNullOrEmpty(x.CategoryId) &&
                    EF.Functions.Like(";" + x.CategoryId + ";", "%;" + lv + ";%")
                );

            model.TotalRecord = query.Count();

            model.CNTBItems = query
                .OrderByDescending(x => x.Created)
                .Skip((model.PageIndex - 1) * model.PageSize)
                .Take(model.PageSize)
                .ToList()
                .Select(MapCNTB)
                .ToList();
        }

        // ================= LOAD TU VAN =================
        private void LoadTuVan(ForumIndexVm model, int lang)
        {
            int lv = model.LinhVuc ?? 0;

            var query = _context.ForumYCDVs
                .AsNoTracking()
                .Where(x =>
                    x.LanguageId == lang &&
                    x.StatusId == STATUS_APPROVED &&
                    (!model.LinhVuc.HasValue ||
                     EF.Functions.Like(";" + x.DichVuId + ";", "%;" + lv + ";%"))
                );

            model.TotalRecord = query.Count();

            model.DVItems = query
                .OrderByDescending(x => x.Created)
                .Skip((model.PageIndex - 1) * model.PageSize)
                .Take(model.PageSize)
                .ToList()
                .Select(MapDV)
                .ToList();
        }

        // ================= MAP =================
        private ForumItemVm MapCNTB(ForumYCTB x)
        {
            return new ForumItemVm
            {
                Id = x.ForumYCTBId,
                Title = x.Title,
                Url = $"{_mainDomain}chi-tiet-thao-luan-{x.ForumYCTBId}.html",
                AuthorInfo = $"<b>{x.LastModifiedBy}</b> lúc <i>{x.LastModified}</i>",
                Viewed = x.Viewed ?? 0,
                Like = x.Like ?? 0,
                Comment = _context.CommentsYCTBs.Count(c =>
                    c.TargetId == x.ForumYCTBId &&
                    c.CommentTypeId == 2 &&
                    c.StatusId == STATUS_APPROVED),
                Categories = LoadItemCategories(x.CategoryId)
            };
        }

        private ForumItemVm MapDV(ForumYCDV x)
        {
            return new ForumItemVm
            {
                Id = x.ForumYCDVId,
                Title = x.Title,
                Url = $"{_mainDomain}chi-tiet-thao-luanDVTV-{x.ForumYCDVId}.html",
                AuthorInfo = $"<b>{x.LastModifiedBy}</b> lúc <i>{x.LastModified}</i>",
                Viewed = x.Viewed ?? 0,
                Like = x.Like ?? 0,
                Comment = x.Comment ?? 0
            };
        }

        // ================= CATEGORY =================
        private List<Category> LoadCategories(int parentId)
        {
            return _context.Categories
                .Where(x => x.ParentId == parentId)
                .OrderBy(x => x.Sort)
                .ToList();
        }

        private List<CategoryVm> LoadItemCategories(string? categoryIds)
        {
            if (string.IsNullOrWhiteSpace(categoryIds))
                return new();

            var ids = categoryIds
                .Split(';', StringSplitOptions.RemoveEmptyEntries)
                .Select(int.Parse)
                .ToList();

            return _context.Categories
                .Where(x => ids.Contains(x.CatId))
                .Select(x => new CategoryVm
                {
                    Title = x.Title,
                    Url = $"{_mainDomain}thao-luan-{x.CatId}-1.html"
                })
                .ToList();
        }

        // ================= PAGER =================
        private PagerVm BuildPager(int total, int pageIndex, int pageSize, int page2Show)
        {
            int totalPage = (int)Math.Ceiling(total / (double)pageSize);

            var pages = Enumerable
                .Range(Math.Max(1, pageIndex - page2Show),
                       Math.Min(totalPage, pageIndex + page2Show) -
                       Math.Max(1, pageIndex - page2Show) + 1);

            return new PagerVm
            {
                CurrentPage = pageIndex,
                TotalPage = totalPage,
                Pages = pages.ToList()
            };
        }

        private ForumPortletNhieuNhatVm LoadPortletNhieuNhat()
        {
            var vm = new ForumPortletNhieuNhatVm
            {
                ActiveTab = 1
            };

            vm.HoiNhieu = _context.PortletHoiNhieu
                .FromSqlRaw("EXEC uspPortletCountTichcuu")
                .AsNoTracking()
                .AsEnumerable() 
                .Take(5)
                .Select(x => new ForumHoiNhieuVm
                {
                    FullName = x.FullName,
                    CountyCTB = x.COUNTYCTB
                })
                .ToList();


            vm.TraLoiNhieu = _context.PortletTraLoiNhieu
                .FromSqlRaw("EXEC uspPortletCountTichcuuTraloi")
                .AsNoTracking()
                .AsEnumerable()
                .Take(5)
                .Select(x => new ForumTraLoiNhieuVm
                {
                    Name = x.Name,
                    CountyCTB = x.COUNTYCTB
                })
                .ToList();

            return vm;
        }

        public ForumPortletDangMoVm LoadPortletDangMo()
        {
            int lang = HttpContext.Session.GetInt32("LanguageId") ?? 1;

            var items = _context.ForumYCTBs
                .AsNoTracking()
                .Where(x =>
                    x.LanguageId == lang &&
                    x.StatusId == STATUS_APPROVED
                )
                .OrderByDescending(x => x.Viewed)
                .Take(5)
                .AsEnumerable()
                .Select(x => new ForumPortletDangMoItemVm
                {
                    Id = x.ForumYCTBId,
                    Title = x.Title,
                    ImageUrl = CookedImageURL("254-170", x.HinhDaiDien, _mainDomain),
                    Tooltip = x.NoiDung,
                    DateText = DateToString(x.Created, "MM/dd/yyyy"),
                    Url = $"{_mainDomain}chi-tiet-thao-luan-{x.ForumYCTBId}.html"
                })
                .ToList();

            return new ForumPortletDangMoVm
            {
                Items = items
            };
        }
        public static string DateToString(object strVal, string strFormat = "mm/dd/yyyy")
        {
            if (strVal == null)
                return "";

            // === CHỈ THÊM ĐOẠN NÀY ===
            string strInput;
            if (strVal is DateTime dt)
            {
                // Giữ đúng logic VB cũ: xử lý chuỗi dd/MM/yyyy
                strInput = dt.ToString("dd/MM/yyyy");
            }
            else
            {
                strInput = strVal.ToString();
            }
            // ========================

            string[] ar = strInput.Split(" ");
            string strcheck;
            string[] a;
            string KQ = "";

            if (ar.Length > 1)
            {
                strcheck = ar[0];
            }
            else
            {
                strcheck = strInput.Trim();
            }

            a = strcheck.Split('/');

            try
            {
                switch (strFormat.ToUpper())
                {
                    case "MM/DD/YYYY":
                        KQ = a[1] + "/" + a[0] + "/" + a[2];
                        break;

                    case "YYYY/MM/DD":
                        KQ = a[2] + "/" + a[1] + "/" + a[0];
                        break;
                }
            }
            catch
            {
            
            }

            return KQ;
        }

        public static string CookedImageURL(string size, string? imageUrl, string mainDomain)
        {
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

        public ForumPortletTinTucVm LoadPortletTinTuc()
        {
            int lang = HttpContext.Session.GetInt32("LanguageId") ?? 1;
            var now = DateTime.Now;

            var items = _context.Contents
                .AsNoTracking()
                .Where(q =>
                    q.MenuId == 44 &&
                    q.LanguageId == lang &&
                    q.StatusId == 3 &&
                    q.IsReport == false &&
                    q.PublishedDate <= now &&
                    (q.eEffectiveDate == null || q.eEffectiveDate >= now)
                )
                .OrderByDescending(q => q.PublishedDate)
                .Take(5)
                .AsEnumerable()
                .Select(q => new ForumTinTucItemVm
                {
                    MenuId = (int)q.MenuId,
                    Id = (int)q.Id,
                    Title = q.Title,
                    QueryString = q.QueryString,
                    ImageUrl = CookedImageURL("254-170", q.Image, _mainDomain),
                    Tooltip = q.Title,
                    DateText = DateToString(q.PublishedDate, "mm/dd/yyyy"),
                    Url = $"{_mainDomain}{q.MenuId}/{q.QueryString}-{q.Id}.html"
                })
                .ToList();

            return new ForumPortletTinTucVm
            {
                Items = items
            };
        }

        public ForumPortletGiaiPhapCongNgheVm LoadPortletGiaiPhapCongNghe()
        {
            int lang = HttpContext.Session.GetInt32("LanguageId") ?? 1;
            var now = DateTime.Now;

            var items = _context.Contents
                .AsNoTracking()
                .Where(q =>
                    q.MenuId == 72 &&
                    q.LanguageId == lang &&
                    q.StatusId == 3 &&
                    q.IsReport == false &&
                    q.PublishedDate <= now &&
                    (q.eEffectiveDate == null || q.eEffectiveDate >= now)
                )
                .OrderByDescending(q => q.PublishedDate)
                .Take(5)
                .AsEnumerable()
                .Select(q => new ForumGiaiPhapCongNgheItemVm
                {
                    MenuId = (int)q.MenuId,
                    Id = (int)q.Id,
                    Title = q.Title,
                    QueryString = q.QueryString,
                    Description = q.Description,
                    ImageUrl = CookedImageURL("254-170", q.Image, _mainDomain),
                    Tooltip = q.Title,
                    DateText = DateToString(q.PublishedDate, "mm/dd/yyyy"),
                    Url = $"{_mainDomain}{q.MenuId}/{q.QueryString}-{q.Id}.html"
                })
                .ToList();

            return new ForumPortletGiaiPhapCongNgheVm
            {
                Items = items
            };
        }


    }
}
