using Microsoft.AspNetCore.Mvc;
using TPApp.Data;
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
                    ? null
                    : int.Parse(cateId),
                CurrentPage = page,
                PageSize = pageSize,
                MainDomain = MainDomain
            };


            vm.DichVuOptions.Add(new SelectItemVm
            {
                Value = "",
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



    }
}
