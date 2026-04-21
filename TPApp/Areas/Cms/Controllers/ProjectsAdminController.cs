using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TPApp.Data;
using TPApp.Entities;
using TPApp.Services;

namespace TPApp.Areas.Cms.Controllers
{
    [Area("Cms")]
    [Authorize(Policy = "CmsAccess")]
    public class ProjectsAdminController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly INotificationQueueService _notificationService;
        private const int LogFunctionId = 20; // Projects

        public ProjectsAdminController(
            AppDbContext context,
            IConfiguration configuration,
            INotificationQueueService notificationService)
        {
            _context = context;
            _configuration = configuration;
            _notificationService = notificationService;
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

        // ─── INDEX ───
        [HttpGet]
        public async Task<IActionResult> Index(
            string? search,
            string? sortBy, string? sortDir,
            int page = 1, int pageSize = 15)
        {
            // Base query — just projects
            var query = _context.Projects.AsNoTracking().AsQueryable();

            // Pre-load all project steps (same approach as DashboardService)
            var allSteps = await _context.ProjectSteps.AsNoTracking()
                .OrderBy(ps => ps.ProjectId).ThenBy(ps => ps.StepNumber)
                .Select(ps => new { ps.ProjectId, ps.StepNumber, ps.StepName, ps.StatusId })
                .ToListAsync();
            var stepsByProject = allSteps.GroupBy(s => s.ProjectId)
                .ToDictionary(g => g.Key, g => g.ToList());

            // Get all members for filtering + display (Buyer=1, Seller=2, Consultant=3)
            var allMembers = await _context.ProjectMembers.AsNoTracking()
                .Where(pm => pm.IsActive)
                .Join(_context.Users.AsNoTracking(),
                    pm => pm.UserId, u => u.Id,
                    (pm, u) => new { pm.Id, pm.ProjectId, pm.Role, u.FullName, UserId = u.Id })
                .ToListAsync();

            // Text search filter
            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();

                // Find project IDs that match via member names (buyer/seller/consultant)
                var memberMatchIds = allMembers
                    .Where(m => m.FullName != null && m.FullName.ToLower().Contains(s))
                    .Select(m => m.ProjectId).Distinct().ToHashSet();

                query = query.Where(p =>
                    p.ProjectCode.ToLower().Contains(s) ||
                    p.ProjectName.ToLower().Contains(s) ||
                    memberMatchIds.Contains(p.Id));
            }

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            // Sort
            bool asc = string.Equals(sortDir, "asc", StringComparison.OrdinalIgnoreCase);
            query = (sortBy?.ToLower()) switch
            {
                "code" => asc ? query.OrderBy(p => p.ProjectCode) : query.OrderByDescending(p => p.ProjectCode),
                "name" => asc ? query.OrderBy(p => p.ProjectName) : query.OrderByDescending(p => p.ProjectName),
                "created" => asc ? query.OrderBy(p => p.CreatedDate) : query.OrderByDescending(p => p.CreatedDate),
                _ => query.OrderByDescending(p => p.CreatedDate)
            };

            var rawList = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Build view models
            var items = rawList.Select(p =>
            {
                // Determine current step using same logic as DashboardService
                var steps = stepsByProject.ContainsKey(p.Id)
                    ? stepsByProject[p.Id] : new();

                int currentStep = 0;
                string stepName = "";

                if (steps.Any())
                {
                    var lastCompleted = steps
                        .Where(s => s.StatusId == 2) // 2 = Completed
                        .OrderByDescending(s => s.StepNumber)
                        .FirstOrDefault();

                    if (lastCompleted == null)
                    {
                        // Nothing completed → show step 1
                        var first = steps.OrderBy(s => s.StepNumber).First();
                        currentStep = first.StepNumber;
                        stepName = first.StepName;
                    }
                    else
                    {
                        var next = steps.FirstOrDefault(s => s.StepNumber > lastCompleted.StepNumber);
                        if (next != null)
                        {
                            currentStep = next.StepNumber;
                            stepName = next.StepName;
                        }
                        else
                        {
                            // All steps completed
                            currentStep = lastCompleted.StepNumber;
                            stepName = lastCompleted.StepName;
                        }
                    }
                }

                var buyers = allMembers
                    .Where(m => m.ProjectId == p.Id && m.Role == 1)
                    .Select(m => m.FullName ?? "").ToList();

                var sellers = allMembers
                    .Where(m => m.ProjectId == p.Id && m.Role == 2)
                    .Select(m => m.FullName ?? "").ToList();

                var consultants = allMembers
                    .Where(m => m.ProjectId == p.Id && m.Role == 3)
                    .Select(m => new ConsultantInfo { Id = m.Id, Name = m.FullName ?? "" })
                    .ToList();

                return new CmsProjectListItem
                {
                    ProjectId = p.Id,
                    ProjectCode = p.ProjectCode,
                    ProjectName = p.ProjectName,
                    CurrentStep = currentStep,
                    CurrentStepName = stepName,
                    BuyerNames = buyers,
                    SellerNames = sellers,
                    Consultants = consultants,
                    CreatedDate = p.CreatedDate
                };
            }).ToList();

            ViewBag.Search = search;
            ViewBag.SortBy = sortBy;
            ViewBag.SortDir = sortDir;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalCount = totalCount;
            ViewBag.TotalPages = totalPages;

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView("_ProjectListPartial", items);

            return View(items);
        }

        // ─── DELETE ───
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null)
                return Json(new { success = false, message = "Không tìm thấy dự án." });

            // Remove related data
            _context.ProjectMembers.RemoveRange(
                _context.ProjectMembers.Where(pm => pm.ProjectId == id));
            _context.ProjectSteps.RemoveRange(
                _context.ProjectSteps.Where(ps => ps.ProjectId == id));
            _context.ProjectWorkflowStates.RemoveRange(
                _context.ProjectWorkflowStates.Where(ws => ws.ProjectId == id));
            _context.ProjectStepStates.RemoveRange(
                _context.ProjectStepStates.Where(ss => ss.ProjectId == id));

            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();

            await WriteLog(3, $"Delete Project: {project.ProjectCode} (ID={id})");
            return Json(new { success = true });
        }

        // ─── ADD CONSULTANT (GET) ───
        [HttpGet]
        public async Task<IActionResult> AddConsultant(int projectId)
        {
            var project = await _context.Projects.AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == projectId);
            if (project == null) return NotFound();

            // Get already assigned consultant user IDs (Role=3)
            var assignedUserIds = await _context.ProjectMembers
                .Where(pm => pm.ProjectId == projectId && pm.Role == 3 && pm.IsActive)
                .Select(pm => pm.UserId)
                .ToListAsync();

            // Get UserId of all active NhaTuVan
            var nhaTuVanUserIds = await _context.NhaTuVans.AsNoTracking()
                .Where(n => n.UserId != null && n.IsActivated == true)
                .Select(n => n.UserId!.Value)
                .ToListAsync();

            // Get Users: IsAdmin=true OR is NhaTuVan, exclude already assigned, same SiteId
            var siteId = GetSiteId();
            var available = await _context.Users.AsNoTracking()
                .Where(u => u.SiteId == siteId
                    && (u.IsAdmin == true || nhaTuVanUserIds.Contains(u.Id))
                    && !assignedUserIds.Contains(u.Id))
                .Select(u => new { u.Id, u.FullName })
                .OrderBy(u => u.FullName)
                .ToListAsync();

            ViewBag.ProjectId = projectId;
            ViewBag.ProjectName = project.ProjectName;
            ViewBag.AvailableConsultants = available
                .Select(a => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = a.Id.ToString(),
                    Text = a.FullName
                }).ToList();

            return PartialView("_AddConsultantPartial");
        }

        // ─── ADD CONSULTANT (POST) ───
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddConsultant(int projectId, int consultantUserId)
        {
            var project = await _context.Projects.FindAsync(projectId);
            if (project == null)
                return Json(new { success = false, message = "Không tìm thấy dự án." });

            // Check if user already exists in this project (any role)
            var existingMember = await _context.ProjectMembers.FirstOrDefaultAsync(pm =>
                pm.ProjectId == projectId && pm.UserId == consultantUserId && pm.IsActive);

            if (existingMember != null)
            {
                var roleName = existingMember.Role switch { 1 => "Buyer", 2 => "Seller", 3 => "Nhà tư vấn", _ => "thành viên" };
                return Json(new { success = false, message = $"Người dùng này đã là {roleName} của dự án." });
            }

            _context.ProjectMembers.Add(new ProjectMember
            {
                ProjectId = projectId,
                UserId = consultantUserId,
                Role = 3, // Consultant
                JoinedDate = DateTime.Now,
                IsActive = true
            });
            await _context.SaveChangesAsync();

            var userName = await _context.Users.AsNoTracking()
                .Where(u => u.Id == consultantUserId)
                .Select(u => u.FullName)
                .FirstOrDefaultAsync();

            await WriteLog(1, $"Add Consultant '{userName}' to Project '{project.ProjectCode}' (ProjectId={projectId})");

            // Notify the consultant
            await _notificationService.QueueAsync(
                consultantUserId,
                projectId,
                "Bạn đã được thêm vào dự án",
                $"Bạn đã được thêm vào dự án <strong>{project.ProjectName}</strong> ({project.ProjectCode}) với vai trò Nhà tư vấn.");

            return Json(new { success = true, message = $"Đã thêm nhà tư vấn '{userName}' vào dự án." });
        }

        // ─── REMOVE CONSULTANT ───
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveConsultant(int id)
        {
            var pm = await _context.ProjectMembers.FindAsync(id);
            if (pm == null || pm.Role != 3)
                return Json(new { success = false, message = "Không tìm thấy bản ghi." });

            var projectCode = await _context.Projects.AsNoTracking()
                .Where(p => p.Id == pm.ProjectId)
                .Select(p => p.ProjectCode)
                .FirstOrDefaultAsync();

            _context.ProjectMembers.Remove(pm);
            await _context.SaveChangesAsync();

            await WriteLog(3, $"Remove Consultant (MemberId={id}) from Project '{projectCode}'");
            return Json(new { success = true });
        }
    }

    // ─── View Models ───
    public class CmsProjectListItem
    {
        public int ProjectId { get; set; }
        public string ProjectCode { get; set; } = "";
        public string ProjectName { get; set; } = "";
        public int CurrentStep { get; set; }
        public string CurrentStepName { get; set; } = "";
        public List<string> BuyerNames { get; set; } = new();
        public List<string> SellerNames { get; set; } = new();
        public List<ConsultantInfo> Consultants { get; set; } = new();
        public DateTime CreatedDate { get; set; }
    }

    public class ConsultantInfo
    {
        public int Id { get; set; } // ProjectMember.Id
        public string Name { get; set; } = "";
    }
}
