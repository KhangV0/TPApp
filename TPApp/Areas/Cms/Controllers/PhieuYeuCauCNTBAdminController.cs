using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TPApp.Data;
using TPApp.Entities;

namespace TPApp.Areas.Cms.Controllers
{
    // ── DTOs ──
    public class PhieuYeuCauListItem
    {
        public int PhieuYeuCauId { get; set; }
        public string? FullName { get; set; }
        public string? NoiDung { get; set; }
        public string? TenDonVi { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? DiaChi { get; set; }
        public bool? IsActivated { get; set; }
        public int? StatusId { get; set; }
        public string? StatusTitle { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? Created { get; set; }
        public DateTime? Ngayyeucau { get; set; }
        public int? Viewed { get; set; }
    }

    // ── Controller ──
    [Area("Cms")]
    [Authorize(Policy = "CmsAccess")]
    public class PhieuYeuCauCNTBAdminController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private const int LogFunctionId = 25; // PhieuYeuCauCNTB

        public PhieuYeuCauCNTBAdminController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        private int GetSiteId() =>
            int.TryParse(_configuration["AppSettings:SiteId"], out var id) ? id : 1;

        private async Task WriteLog(int eventId, string content)
        {
            _context.Logs.Add(new Log
            {
                FunctionID = LogFunctionId,
                ActTime = DateTime.Now,
                EventID = eventId,
                Content = content,
                ClientIP = HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserName = User.Identity?.Name,
                Domain = HttpContext.Request.Host.Value,
                LanguageId = 1,
                ParentId = 0,
                SiteId = GetSiteId()
            });
            await _context.SaveChangesAsync();
        }

        // ── INDEX ──
        public async Task<IActionResult> Index(
            string? keyword, int? statusId, bool? isActivated,
            int? siteId,
            DateTime? createdFrom, DateTime? createdTo,
            string? sortBy, string? sortDir,
            int page = 1, int pageSize = 30)
        {
            var configSiteId = GetSiteId();

            var query = _context.PhieuYeuCauCNTBs.AsNoTracking()
                .Where(p => p.SiteId == null || p.SiteId == configSiteId);

            // Filters
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var kw = keyword.Trim().ToLower();
                query = query.Where(p =>
                    (p.Title != null && p.Title.ToLower().Contains(kw)) ||
                    (p.FullName != null && p.FullName.ToLower().Contains(kw)) ||
                    (p.TenDonVi != null && p.TenDonVi.ToLower().Contains(kw)) ||
                    (p.Phone != null && p.Phone.Contains(kw)) ||
                    (p.Email != null && p.Email.ToLower().Contains(kw)));
            }
            if (statusId.HasValue)
                query = query.Where(p => p.StatusId == statusId.Value);
            if (isActivated.HasValue)
                query = query.Where(p => p.IsActivated == isActivated.Value);
            if (siteId.HasValue)
                query = query.Where(p => p.SiteId == siteId.Value);
            if (createdFrom.HasValue)
                query = query.Where(p => p.Created >= createdFrom.Value);
            if (createdTo.HasValue)
                query = query.Where(p => p.Created <= createdTo.Value.AddDays(1));

            // Sort
            query = sortBy?.ToLower() switch
            {
                "fullname" => sortDir == "desc" ? query.OrderByDescending(p => p.FullName) : query.OrderBy(p => p.FullName),
                "noidung" => sortDir == "desc" ? query.OrderByDescending(p => p.NoiDung) : query.OrderBy(p => p.NoiDung),
                "email" => sortDir == "desc" ? query.OrderByDescending(p => p.Email) : query.OrderBy(p => p.Email),
                "created" => sortDir == "desc" ? query.OrderByDescending(p => p.Created) : query.OrderBy(p => p.Created),
                "ngayyeucau" => sortDir == "desc" ? query.OrderByDescending(p => p.Ngayyeucau) : query.OrderBy(p => p.Ngayyeucau),
                _ => query.OrderByDescending(p => p.Created)
            };

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            page = Math.Clamp(page, 1, Math.Max(1, totalPages));

            // Statuses
            var statuses = await _context.Statuses.AsNoTracking()
                .OrderBy(s => s.StatusId).ToListAsync();
            var statusDict = statuses.ToDictionary(s => s.StatusId, s => s.Title);

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new PhieuYeuCauListItem
                {
                    PhieuYeuCauId = p.PhieuYeuCauId,
                    FullName = p.FullName,
                    NoiDung = p.NoiDung,
                    TenDonVi = p.TenDonVi,
                    Phone = p.Phone,
                    Email = p.Email,
                    DiaChi = p.DiaChi,
                    IsActivated = p.IsActivated,
                    StatusId = p.StatusId,
                    CreatedBy = p.CreatedBy,
                    Created = p.Created,
                    Ngayyeucau = p.Ngayyeucau,
                    Viewed = p.Viewed
                })
                .ToListAsync();

            // Map status titles
            foreach (var item in items)
            {
                if (item.StatusId.HasValue && statusDict.TryGetValue(item.StatusId.Value, out var sTitle))
                    item.StatusTitle = sTitle;
            }

            ViewBag.TotalCount = totalCount;
            ViewBag.TotalPages = totalPages;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.Keyword = keyword;
            ViewBag.StatusId = statusId;
            ViewBag.IsActivated = isActivated;
            ViewBag.CreatedFrom = createdFrom?.ToString("yyyy-MM-dd");
            ViewBag.CreatedTo = createdTo?.ToString("yyyy-MM-dd");
            ViewBag.Statuses = statuses;
            ViewBag.SortBy = sortBy;
            ViewBag.SortDir = sortDir;
            ViewBag.SiteId = siteId;
            ViewBag.CurrentSiteId = configSiteId;
            ViewBag.Sites = await _context.RootSites.AsNoTracking().OrderBy(s => s.SiteId).ToListAsync();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView("_ListPartial", items);

            return View(items);
        }

        // ── DELETE ──
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _context.PhieuYeuCauCNTBs.FindAsync(id);
            if (entity == null)
                return Json(new { success = false, message = "Không tìm thấy phiếu yêu cầu." });

            _context.PhieuYeuCauCNTBs.Remove(entity);
            await _context.SaveChangesAsync();

            await WriteLog(3, $"Delete PhieuYeuCauCNTB: ID={id}");

            return Json(new { success = true, message = "Đã xóa phiếu yêu cầu #" + id });
        }

        // ── TOGGLE ACTIVATE ──
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActivate(int id)
        {
            var entity = await _context.PhieuYeuCauCNTBs.FindAsync(id);
            if (entity == null)
                return Json(new { success = false, message = "Không tìm thấy phiếu yêu cầu." });

            entity.IsActivated = !(entity.IsActivated ?? false);
            await _context.SaveChangesAsync();

            var status = entity.IsActivated == true ? "Kích hoạt" : "Hủy kích hoạt";
            await WriteLog(2, $"ToggleActivate PhieuYeuCauCNTB: ID={id} -> {status}");

            return Json(new { success = true, message = $"Đã {status.ToLower()} phiếu yêu cầu #{id}", activated = entity.IsActivated });
        }
    }
}
