using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TPApp.Data;
using TPApp.Entities;

namespace TPApp.Areas.Cms.Controllers
{
    // ── DTOs ──
    public class FeedbackListItem
    {
        public int Id { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }
        public int? StatusId { get; set; }
        public string? StatusTitle { get; set; }
        public DateTime? Created { get; set; }
        public int? SiteId { get; set; }
    }

    // ── Controller ──
    [Area("Cms")]
    [Authorize(Policy = "CmsAccess")]
    public class FeedbackAdminController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private const int LogFunctionId = 26; // Feedback

        public FeedbackAdminController(AppDbContext context, IConfiguration configuration)
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
            string? keyword, int? statusId, int? siteId,
            DateTime? createdFrom, DateTime? createdTo,
            string? sortBy, string? sortDir,
            int page = 1, int pageSize = 30)
        {
            var configSiteId = GetSiteId();

            var query = _context.Feedbacks.AsNoTracking()
                .Where(f => f.SiteId == null || f.SiteId == configSiteId);

            // Filters
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var kw = keyword.Trim().ToLower();
                query = query.Where(f =>
                    (f.FullName != null && f.FullName.ToLower().Contains(kw)) ||
                    (f.Email != null && f.Email.ToLower().Contains(kw)) ||
                    (f.Phone != null && f.Phone.Contains(kw)) ||
                    (f.Title != null && f.Title.ToLower().Contains(kw)) ||
                    (f.Content != null && f.Content.ToLower().Contains(kw)));
            }
            if (statusId.HasValue)
                query = query.Where(f => f.StatusId == statusId.Value);
            if (siteId.HasValue)
                query = query.Where(f => f.SiteId == siteId.Value);
            if (createdFrom.HasValue)
                query = query.Where(f => f.Created >= createdFrom.Value);
            if (createdTo.HasValue)
                query = query.Where(f => f.Created <= createdTo.Value.AddDays(1));

            // Sort
            query = sortBy?.ToLower() switch
            {
                "fullname" => sortDir == "desc" ? query.OrderByDescending(f => f.FullName) : query.OrderBy(f => f.FullName),
                "email" => sortDir == "desc" ? query.OrderByDescending(f => f.Email) : query.OrderBy(f => f.Email),
                "title" => sortDir == "desc" ? query.OrderByDescending(f => f.Title) : query.OrderBy(f => f.Title),
                "created" => sortDir == "desc" ? query.OrderByDescending(f => f.Created) : query.OrderBy(f => f.Created),
                _ => query.OrderByDescending(f => f.Created)
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
                .Select(f => new FeedbackListItem
                {
                    Id = f.Id,
                    FullName = f.FullName,
                    Email = f.Email,
                    Phone = f.Phone,
                    Address = f.Address,
                    Title = f.Title,
                    Content = f.Content,
                    StatusId = f.StatusId,
                    Created = f.Created,
                    SiteId = f.SiteId
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
            var entity = await _context.Feedbacks.FindAsync(id);
            if (entity == null)
                return Json(new { success = false, message = "Không tìm thấy phản hồi." });

            _context.Feedbacks.Remove(entity);
            await _context.SaveChangesAsync();

            await WriteLog(3, $"Delete Feedback: ID={id}, From={entity.FullName}");

            return Json(new { success = true, message = "Đã xóa phản hồi #" + id });
        }

        // ── UPDATE STATUS ──
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, int statusId)
        {
            var entity = await _context.Feedbacks.FindAsync(id);
            if (entity == null)
                return Json(new { success = false, message = "Không tìm thấy phản hồi." });

            entity.StatusId = statusId;
            entity.Modified = DateTime.Now;
            entity.Modifier = User.Identity?.Name;
            await _context.SaveChangesAsync();

            var statusTitle = await _context.Statuses.AsNoTracking()
                .Where(s => s.StatusId == statusId).Select(s => s.Title).FirstOrDefaultAsync() ?? "";

            await WriteLog(2, $"UpdateStatus Feedback: ID={id} -> {statusTitle}");

            return Json(new { success = true, message = $"Đã cập nhật trạng thái thành \"{statusTitle}\"" });
        }
    }
}
