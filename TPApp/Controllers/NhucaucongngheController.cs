using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using TPApp.Data;
using TPApp.Entities;
using TPApp.Enums;
using TPApp.Helpers;
using TPApp.ViewModel;

namespace TPApp.Controllers.FrontEnd
{
    public class NhucaucongngheController : Controller
    {
        private readonly AppDbContext _context;
        private readonly string _mainDomain;

        public NhucaucongngheController(AppDbContext context, IOptions<AppSettings> appSettings)
        {
            _context = context;
            _mainDomain = appSettings.Value.MainDomain;
        }


        public IActionResult CateTechNeeds(
            int menuId,
            string? linhvuc,
            int page = 1)
        {
            var vm = new CateTechNeedsViewModel
            {
                MenuId = menuId,
                SelectedLinhVuc = linhvuc ?? HttpContext.Session.GetString("Linhvucyeucau"),
                CurrentPage = page
            };

            LoadLinhVuc(vm);
            BindToGrid(vm, page);

            HttpContext.Session.SetString("Linhvucyeucau", vm.SelectedLinhVuc ?? "");

            return View("~/Views/Nhucaucongnghe/TechNeedsByMenu.cshtml", vm);

        }

        #region === PRIVATE METHODS (GIỮ NGUYÊN LOGIC) ===

        private void LoadLinhVuc(CateTechNeedsViewModel vm)
        {
            vm.LinhVucs.Add(new SelectListItem
            {
                Text = " --- Chọn lĩnh vực --- ",
                Value = ""
            });

            var list = _context.Categories
                .Where(x => x.ParentId == 1)
                .Select(x => new SelectListItem
                {
                    Value = x.CatId.ToString(),
                    Text = x.Title
                })
                .ToList();

            vm.LinhVucs.AddRange(list);
        }

        private void BindToGrid(CateTechNeedsViewModel vm, int page)
        {
            int pageSize = 9;
            int lang = HttpContext.Session.GetInt32("LanguageId") ?? 1;

            var subMenus = _context.UspSelectSubMenu(vm.MenuId);

            var first = _context.ContentsYeuCaus
                .Where(q =>
                    q.MenuId == vm.MenuId ||
                    subMenus.Contains(q.MenuId ?? 0))
                .Where(q =>
                    q.LanguageId == lang &&
                    q.StatusId == 3 &&
                    q.PublishedDate <= DateTime.Now &&
                    (q.eEffectiveDate >= DateTime.Now || q.eEffectiveDate == null))
                .OrderByDescending(q => q.PublishedDate)
                .FirstOrDefault();



            if (first == null) return;

            vm.FirstItem = MapItem(first);

            // danh sách
            var query = _context.ContentsYeuCaus.Where(q =>
                (q.MenuId == vm.MenuId || subMenus.Contains(q.MenuId ?? 0)) &&
                q.LanguageId == lang &&
                q.StatusId == 3 &&
                q.PublishedDate <= DateTime.Now &&
                (q.eEffectiveDate >= DateTime.Now || q.eEffectiveDate == null));

            if (!string.IsNullOrEmpty(vm.SelectedLinhVuc) && vm.SelectedLinhVuc != ";;")
            {
                query = query.Where(q => q.LinhVucId.Contains(vm.SelectedLinhVuc));
            }

            int total = query.Count();

            vm.Items = query
                .OrderByDescending(q => q.PublishedDate)
                .Skip(((page - 1) * pageSize) + 1)
                .Take(pageSize)
                .AsEnumerable()
                .Select(q => MapItem(q))
                .ToList();

            CreatePager(vm, total, page, pageSize, 10);
        }

        private TechNeedItemVm MapItem(dynamic q)
        {
            int menuId = q.MenuId == null ? 0 : Convert.ToInt32(q.MenuId);
            int id = q.Id == null ? 0 : Convert.ToInt32(q.Id);

            return new TechNeedItemVm
            {
                MenuId = menuId,
                Id = id,
                Title = q.Title,
                QueryString = q.QueryString,
                Image = q.Image,
                Description = q.Description,
                PublishedDate = q.PublishedDate,
                DetailUrl = $"{_mainDomain}{menuId}/yeu-cau/{q.QueryString}-{id}.html"
            };
        }


        private void CreatePager(
            CateTechNeedsViewModel vm,
            int totalRecord,
            int pageIndex,
            int pageSize,
            int page2Show)
        {
            int totalPage =
                totalRecord % pageSize == 0
                    ? totalRecord / pageSize
                    : totalRecord / pageSize + 1;

            vm.TotalPage = totalPage;
            vm.CurrentPage = pageIndex;

            IEnumerable<int> left =
                pageIndex <= page2Show
                    ? Enumerable.Range(1, pageIndex)
                    : Enumerable.Range(pageIndex - page2Show, page2Show);

            IEnumerable<int> right =
                pageIndex + page2Show <= totalPage
                    ? Enumerable.Range(pageIndex, page2Show + 1)
                    : Enumerable.Range(pageIndex, totalPage - pageIndex + 1);

            vm.Pages = left.Union(right).Distinct().ToList();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult GuiPhieuYeuCau(PhieuYeuCauCNViewModel model)
        {
            try
            {
                var p = new PhieuYeuCauCNTB
                {
                    NoiDung = model.NoiDung?.Trim(),
                    FullName = model.FullName?.Trim(),
                    HinhDaiDien = "",
                    DiaChi = "",
                    Phone = model.Phone?.Trim(),
                    Email = model.Email?.Trim(),
                    Created = DateTime.Now,
                    CreatedBy = model.FullName,
                    IsActivated = true,
                    Domain = HttpContext.Request.Host.Value,
                    StatusId = 1,
                    LanguageId = 1,
                    ParentId = 0,
                    Ngayyeucau = DateTime.Now,
                    SiteId = 1
                };

                _context.PhieuYeuCauCNTBs.Add(p);
                _context.SaveChanges();

                // ===== GIỮ NGUYÊN LOGIC ANTI EMAIL ATTACK =====
                if (!(string.IsNullOrEmpty(p.NoiDung)
                    || p.Phone == "555-666-0606"
                    || p.Email!.Contains("@example.com")
                    || p.Email.Contains("@email.tst")))
                {
                    // Send mail ở đây (nếu bạn đã có helper)
                }

                return Redirect(_mainDomain + "page/thanks");
            }
            catch
            {
                TempData["Error"] =
                    "Gửi thất bại hãy kiểm tra lại đường truyền hoặc liên hệ với quản lý.";
                return RedirectToAction(nameof(CateTechNeeds));
            }
        }
        public IActionResult PortletYeuCauMoi()
        {
            var vm = new PortletYeuCauMoiViewModel();

            // === BindToGrid (GIỮ NGUYÊN LOGIC VB.NET) ===
            vm.Items = _context.PhieuYeuCauCNTBs
                .Where(x => x.StatusId == 3)
                .OrderByDescending(x => x.Created)
                .Take(10)
                .ToList();

            return PartialView(
                "_PortletYeuCauMoi.cshtml",
                vm
            );
        }


        public IActionResult Detail(int id)
        {
            var vm = new NhuCauCongNgheDetailViewModel();

            vm.TargetId = id;

            // === LoadData(intID) ===
            var p = _context.ContentsYeuCaus
                .Where(q => q.Id == id && q.StatusId == 3)
                .OrderByDescending(q => q.PublishedDate)
                .Select(q => new ContentYeucauDetailVm
                {
                    Id = (int)q.Id,
                    Title = q.Title,
                    Description = q.Description,
                    Contents = q.Contents,
                    Author = q.Author,
                    QueryString = q.QueryString,
                    MenuId = (int)q.MenuId,
                    Viewed = q.Viewed,
                    Like = q.Like,
                    PublishedDate = q.PublishedDate,
                    TypeId = (int)q.TypeId
                })
                .FirstOrDefault();

            if (p == null)
                return NotFound();

            vm.Detail = p;

            // === Update Viewed (GIỮ LOGIC) ===
            var entity = _context.ContentsYeuCaus.First(x => x.Id == id);
            entity.Viewed = entity.Viewed == null ? 1 : entity.Viewed + 1;
            _context.SaveChanges();

            // === CommentTypeID ===
            vm.CommentTypeId = p.TypeId switch
            {
                7 => (int)CommentType.Yeucaucongnghe,
                8 => (int)CommentType.Yeucautuvan,
                9 => (int)CommentType.Yeucautimkiemdoitac,
                _ => null
            };

            // === Images (TypeId = 4) ===
            if (p.TypeId == 4)
            {
                vm.Images = _context.Albums
                    .Where(x => x.ContensID == p.Id)
                    .ToList();
            }

            // === Relation ===
            int lang = HttpContext.Session.GetInt32("LanguageId") ?? 1;

            var subMenuIds = _context
                .UspSelectSubMenu(p.MenuId)
                .Select(x => (long)x)
                .ToArray();

            vm.Relations = _context.ContentsYeuCaus
                .Where(q =>
                    q.Id != id &&
                    q.StatusId == 3 &&
                    (
                        q.MenuId == p.MenuId ||
                        System.Linq.Enumerable.Contains(subMenuIds, (int)q.MenuId)
                    ) &&
                    q.LanguageId == lang
                )
                .OrderByDescending(q => q.PublishedDate)
                .Take(5)
                .Select(q => new RelationItemVm
                {
                    Id = (int)q.Id,
                    Title = q.Title,
                    QueryString = q.QueryString,
                    MenuId = (int)q.MenuId,
                    PublishedDate = q.PublishedDate
                })
                .ToList();


            ViewData["Title"] = p.Title;
            ViewData["MetaDescription"] = p.Description;
            ViewData["BackLink"] = _mainDomain + "yeu-cau-cong-nghe-67.html";

            return View("Detail", vm);
        }

        #endregion
    }
}
