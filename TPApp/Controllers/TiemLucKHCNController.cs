using Microsoft.AspNetCore.Mvc;
using TPApp.Data;
using TPApp.ViewModel;

namespace TPApp.Controllers
{
    public class TiemLucKHCNController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        public TiemLucKHCNController(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [HttpGet]
        public IActionResult Index(
            string? t,
            int page = 1,
            string search = "a b c d e f g h i j k l m o p q r s t u v z y"
        )
        {
            const int pageSize = 10;
            const int page2Show = 5;

            var typeName = GetTypeName(t);

            var query = _context.SearchIndexContents
                .Where(x => x.TypeName == typeName && x.Title != "")
                .OrderBy(x => x.Title)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(x =>
                    x.Title.Contains(search) ||
                    x.Description.Contains(search));
            }

            var totalRecord = query.Count();
            var totalPage = (int)Math.Ceiling(totalRecord / (double)pageSize);

            var items = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var vm = new TiemLucKHCNIndexVm
            {
                TypeKey = t,
                TypeName = typeName,
                SearchText = search,
                CurrentPage = page,
                TotalPage = totalPage,
                TotalRecord = totalRecord,
                Items = items,
                Pages = BuildPager(page, totalPage, page2Show)
            };

            return View(vm);
        }

        // ===============================
        // Helper
        // ===============================

        private static string GetTypeName(string? t) => t switch
        {
            "dtnc" => "Tiềm lực Đề tài NCKH",
            "cg" => "Tiềm lực Chuyên gia",
            "ptn" => "Tiềm lực Phòng thí nghiệm",
            "tckhcn" => "Tiềm lực Tổ chức",
            "dnkhcn" => "Tiềm lực Doanh nghiệp",
            "tstt" => "Tài Sản Trí Tuệ",
            _ => ""
        };

        private static List<int> BuildPager(int currentPage, int totalPage, int page2Show)
        {
            var pages = new HashSet<int>();

            var leftStart = Math.Max(1, currentPage - page2Show);
            var leftEnd = currentPage;

            for (int i = leftStart; i <= leftEnd; i++)
                pages.Add(i);

            var rightEnd = Math.Min(totalPage, currentPage + page2Show);
            for (int i = currentPage; i <= rightEnd; i++)
                pages.Add(i);

            return pages.OrderBy(x => x).ToList();
        }
    }
}
